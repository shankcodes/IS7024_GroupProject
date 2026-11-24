using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using IS7024_GroupProject.Data;
using IS7024_GroupProject.Models;
using IS7024_GroupProject.Services;
using IS7024_GroupProject.Services.GoogleBooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IS7024_GroupProject.Pages;

public class IndexModel(
    ApplicationDbContext dbContext,
    IGoogleBooksService googleBooksService) : PageModel
{
    private readonly ReadingListStore readingListStore = new(dbContext);

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    [BindProperty]
    public BookInput AddToListInput { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public BookSearchResult? SearchResult { get; private set; }

    public bool HasSearched { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (Query is null)
        {
            return;
        }

        HasSearched = true;

        if (string.IsNullOrWhiteSpace(Query))
        {
            SearchResult = BookSearchResult.EmptyQuery();
            return;
        }

        SearchResult = await googleBooksService.SearchAsync(Query, cancellationToken);
    }

    public async Task<IActionResult> OnPostAddToListAsync(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true
            || !int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId)
            || userId <= 0
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
