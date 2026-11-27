using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using IS7024_GroupProject.Data;
using IS7024_GroupProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IS7024_GroupProject.Pages;

public sealed class BookDetailsModel : PageModel
{
    private readonly ApplicationDbContext dbContext;

    public BookDetailsModel(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [ActivatorUtilitiesConstructor]
    public BookDetailsModel(
        ApplicationDbContext dbContext,
        UserManager<User> userManager)
        : this(dbContext)
    {
    }

    public Book? Book { get; private set; }

    public bool CanRate { get; private set; }

    public decimal AverageRating { get; private set; }

    public int RatingCount { get; private set; }

    [BindProperty]
    public ReviewFormInput ReviewInput { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken cancellationToken)
    {
        await LoadAsync(id, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveReviewAsync(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out int userId))
        {
            return Challenge();
        }

        if (ReviewInput.BookId < 1)
        {
            return NotFound();
        }

        bool isFinished = await dbContext.ReadingLists
            .AsNoTracking()
            .AnyAsync(
                entry => entry.UserId == userId
                    && entry.BookId == ReviewInput.BookId
                    && entry.Status == ReadingStatus.Finished,
                cancellationToken);

        if (!isFinished)
        {
            ModelState.AddModelError(
                nameof(ReviewInput.Rating),
                "Finish this book to rate it.");
            await LoadAsync(ReviewInput.BookId, cancellationToken);
            return Page();
        }

        if (ReviewInput.Rating is null or < 1 or > 5)
        {
            ModelState.AddModelError(
                nameof(ReviewInput.Rating),
                "Select a rating from 1 to 5.");
        }

        if (!TryValidateModel(ReviewInput))
        {
            await LoadAsync(ReviewInput.BookId, cancellationToken);
            return Page();
        }

        BookReview? review = await dbContext.BookReviews
            .SingleOrDefaultAsync(
                item => item.UserId == userId
                    && item.BookId == ReviewInput.BookId,
                cancellationToken);

        if (review is null)
        {
            review = new BookReview
            {
                UserId = userId,
                BookId = ReviewInput.BookId
            };
            dbContext.BookReviews.Add(review);
        }

        review.Rating = ReviewInput.Rating;
        review.Summary = ReviewInput.Summary;
        review.Review = ReviewInput.Review;
        review.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        StatusMessage = "Your rating and review were saved.";
        return RedirectToPage(new { id = ReviewInput.BookId });
    }

    private async Task LoadAsync(int bookId, CancellationToken cancellationToken)
    {
        Book = await dbContext.Books
            .AsNoTracking()
            .SingleOrDefaultAsync(
                book => book.BookId == bookId,
                cancellationToken);

        if (Book is null)
        {
            return;
        }

        var ratingAggregate = await dbContext.BookReviews
            .AsNoTracking()
            .Where(review => review.BookId == bookId && review.Rating.HasValue)
            .GroupBy(review => review.BookId)
            .Select(group => new
            {
                Count = group.Count(),
                Sum = group.Sum(review => review.Rating!.Value)
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (ratingAggregate is null)
        {
            RatingCount = 0;
            AverageRating = 0m;
        }
        else
        {
            RatingCount = ratingAggregate.Count;
            AverageRating = decimal.Round(
                (decimal)ratingAggregate.Sum / RatingCount,
                1,
                MidpointRounding.AwayFromZero);
        }

        ReviewInput.BookId = bookId;

        if (TryGetCurrentUserId(out int userId))
        {
            CanRate = await dbContext.ReadingLists
                .AsNoTracking()
                .AnyAsync(
                    entry => entry.UserId == userId
                        && entry.BookId == bookId
                        && entry.Status == ReadingStatus.Finished,
                    cancellationToken);

            BookReview? myReview = await dbContext.BookReviews
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    review => review.UserId == userId
                        && review.BookId == bookId,
                    cancellationToken);

            if (myReview is not null)
            {
                ReviewInput.Rating = myReview.Rating;
                ReviewInput.Summary = myReview.Summary;
                ReviewInput.Review = myReview.Review;
            }
        }
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

    public sealed class ReviewFormInput
    {
        [Range(1, int.MaxValue)]
        public int BookId { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        [StringLength(200)]
        public string? Summary { get; set; }

        [StringLength(4000)]
        public string? Review { get; set; }
    }
}
