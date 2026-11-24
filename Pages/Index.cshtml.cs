using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using IS7024_GroupProject.Data;
using IS7024_GroupProject.Models;
using IS7024_GroupProject.Services;
using IS7024_GroupProject.Services.GoogleBooks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IS7024_GroupProject.Pages;

public sealed class IndexModel(ApplicationDbContext dbContext, IGoogleBooksService googleBooksService) : PageModel
{
    public const string NoCoverUrl = "/images/no-cover.svg";
    public const string InvalidGoalMessage = "Goal must be at least 1 book.";

    private readonly ReadingListStore readingListStore = new(dbContext);

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    [BindProperty]
    public int? GoalTarget { get; set; }

    [BindProperty]
    public BookInput AddToListInput { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public BookSearchResult? SearchResult { get; private set; }

    public bool HasSearched { get; private set; }

    public int CurrentYear { get; private set; } = DateTime.Today.Year;

    public int FinishedBookCount { get; private set; }

    public int GoalProgressPercentage { get; private set; }

    public ReadingGoal? CurrentGoal { get; private set; }

    public IReadOnlyList<ReadingList> CurrentlyReading { get; private set; } = [];

    public IReadOnlyList<ReadingList> ReadNext { get; private set; } = [];

    public IReadOnlyList<ReadingList> RecentlyFinished { get; private set; } = [];

    public IReadOnlyList<IGrouping<int, ReadingList>> RecentlyFinishedByMonth { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadDashboardAsync(cancellationToken);

        if (Query is null)
        {
            return;
        }

        HasSearched = true;
        SearchResult = string.IsNullOrWhiteSpace(Query)
            ? BookSearchResult.EmptyQuery()
            : await googleBooksService.SearchAsync(Query, cancellationToken);
    }

    public async Task<IActionResult> OnPostSaveGoalAsync(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out int userId))
        {
            return Challenge();
        }

        if (GoalTarget is null or < 1)
        {
            ModelState.AddModelError(nameof(GoalTarget), InvalidGoalMessage);
            await LoadDashboardAsync(cancellationToken);
            return Page();
        }

        int year = DateTime.Today.Year;
        ReadingGoal? goal = await dbContext.ReadingGoals
            .SingleOrDefaultAsync(item => item.UserId == userId && item.Year == year, cancellationToken);

        if (goal is null)
        {
            dbContext.ReadingGoals.Add(new ReadingGoal
            {
                UserId = userId,
                Year = year,
                TargetBooks = GoalTarget.Value
            });
        }
        else
        {
            goal.TargetBooks = GoalTarget.Value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToPage();
    }

    public static TagBuilder CreateCoverImage(string? coverUrl, string title, string cssClass)
    {
        var image = new TagBuilder("img");
        image.Attributes["src"] = GetUsableCoverUrl(coverUrl);
        image.Attributes["alt"] = $"Cover of {title}";
        image.Attributes["onerror"] = "this.onerror=null;this.src='/images/no-cover.svg';";
        image.AddCssClass(cssClass);
        return image;
    }

    public static string GetUsableCoverUrl(string? coverUrl)
    {
        if (string.IsNullOrWhiteSpace(coverUrl)
            || !Uri.TryCreate(coverUrl.Trim(), UriKind.Absolute, out Uri? uri)
            || !uri.IsWellFormedOriginalString()
            || string.IsNullOrEmpty(uri.Host)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return NoCoverUrl;
        }

        return uri.AbsoluteUri;
    }

    public static string GetMonthName(int month) =>
        CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

    private async Task LoadDashboardAsync(CancellationToken cancellationToken)
    {
        CurrentYear = DateTime.Today.Year;

        if (!TryGetCurrentUserId(out int userId))
        {
            return;
        }

        DateTime yearStart = new(CurrentYear, 1, 1);
        DateTime nextYearStart = yearStart.AddYears(1);

        // Currently Reading: Show up to 5 books
        CurrentlyReading = await dbContext.ReadingLists
            .AsNoTracking()
            .Include(entry => entry.Book)
            .Where(entry => entry.UserId == userId &&
                            entry.Status == ReadingStatus.Reading)
            .OrderBy(entry => entry.Book.Title)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Read Next: Show up to 5 books
        ReadNext = await dbContext.ReadingLists
            .AsNoTracking()
            .Include(entry => entry.Book)
            .Where(entry => entry.UserId == userId &&
                            entry.Status == ReadingStatus.WantToRead)
            .OrderBy(entry => entry.Book.Title)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Recently Finished: Show the 5 most recently finished books
        RecentlyFinished = await dbContext.ReadingLists
            .AsNoTracking()
            .Include(entry => entry.Book)
            .Where(entry => entry.UserId == userId
                && entry.Status == ReadingStatus.Finished
                && entry.FinishedDate >= yearStart
                && entry.FinishedDate < nextYearStart)
            .OrderByDescending(entry => entry.FinishedDate)
            .Take(5)
            .ToListAsync(cancellationToken);

        RecentlyFinishedByMonth = RecentlyFinished
            .GroupBy(entry => entry.FinishedDate!.Value.Month)
            .OrderByDescending(group => group.Key)
            .ToArray();

        // Count ALL finished books for accurate reading goal progress
        FinishedBookCount = await dbContext.ReadingLists
            .AsNoTracking()
            .CountAsync(entry => entry.UserId == userId
                && entry.Status == ReadingStatus.Finished
                && entry.FinishedDate >= yearStart
                && entry.FinishedDate < nextYearStart,
                cancellationToken);

        CurrentGoal = await dbContext.ReadingGoals
            .AsNoTracking()
            .SingleOrDefaultAsync(
                goal => goal.UserId == userId && goal.Year == CurrentYear,
                cancellationToken);

        GoalProgressPercentage = CurrentGoal is { TargetBooks: > 0 }
            ? Math.Min(
                100,
                (int)Math.Round(
                    FinishedBookCount * 100d / CurrentGoal.TargetBooks,
                    MidpointRounding.AwayFromZero))
            : 0;
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;

        return User.Identity?.IsAuthenticated == true
            && int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out userId)
            && userId > 0;
    }

    public async Task<IActionResult> OnPostAddToListAsync(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out int userId)
            || !await dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken))
        {
            return Challenge();
        }

        if (!TryValidateModel(AddToListInput))
        {
            return BadRequest();
        }

        bool added = await readingListStore.AddBookAsync(
            userId,
            AddToListInput.Title,
            AddToListInput.Author,
            AddToListInput.GoogleBooksId,
            AddToListInput.CoverUrl,
            cancellationToken);

        if (!added)
        {
            StatusMessage = "This book is already on your list.";
            return RedirectToPage(new { Query });
        }

        StatusMessage = "Added to your list.";
        return RedirectToPage("/ReadingList");
    }

    public sealed class BookInput
    {
        [Required]
        [StringLength(250)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string GoogleBooksId { get; set; } = string.Empty;

        [StringLength(2048)]
        public string? CoverUrl { get; set; }
    }
}
