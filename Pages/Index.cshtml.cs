using System.Globalization;
using System.Security.Claims;
using IS7024_GroupProject.Data;
using IS7024_GroupProject.Models;
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

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    [BindProperty]
    public int? GoalTarget { get; set; }

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

    public static string GetMonthName(int month) => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

    private async Task LoadDashboardAsync(CancellationToken cancellationToken)
    {
        CurrentYear = DateTime.Today.Year;
        if (!TryGetCurrentUserId(out int userId))
        {
            return;
        }

        DateTime yearStart = new(CurrentYear, 1, 1);
        DateTime nextYearStart = yearStart.AddYears(1);

        CurrentlyReading = await dbContext.ReadingLists
            .AsNoTracking()
            .Include(entry => entry.Book)
            .Where(entry => entry.UserId == userId && entry.Status == ReadingStatus.Reading)
            .OrderBy(entry => entry.Book.Title)
            .ToListAsync(cancellationToken);

        ReadNext = await dbContext.ReadingLists
            .AsNoTracking()
            .Include(entry => entry.Book)
            .Where(entry => entry.UserId == userId && entry.Status == ReadingStatus.WantToRead)
            .OrderBy(entry => entry.Book.Title)
            .ToListAsync(cancellationToken);

        RecentlyFinished = await dbContext.ReadingLists
            .AsNoTracking()
            .Include(entry => entry.Book)
            .Where(entry => entry.UserId == userId
                && entry.Status == ReadingStatus.Finished
                && entry.FinishedDate >= yearStart
                && entry.FinishedDate < nextYearStart)
            .OrderByDescending(entry => entry.FinishedDate)
            .ToListAsync(cancellationToken);
        RecentlyFinishedByMonth = RecentlyFinished
            .GroupBy(entry => entry.FinishedDate!.Value.Month)
            .OrderByDescending(group => group.Key)
            .ToArray();

        FinishedBookCount = RecentlyFinished.Count;
        CurrentGoal = await dbContext.ReadingGoals
            .AsNoTracking()
            .SingleOrDefaultAsync(goal => goal.UserId == userId && goal.Year == CurrentYear, cancellationToken);

        GoalProgressPercentage = CurrentGoal is { TargetBooks: > 0 }
            ? Math.Min(100, (int)Math.Round(FinishedBookCount * 100d / CurrentGoal.TargetBooks, MidpointRounding.AwayFromZero))
            : 0;
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;
        return User.Identity?.IsAuthenticated == true
            && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId)
            && userId > 0;
    }
}
