using System.ComponentModel.DataAnnotations;
using IS7024_GroupProject.Models;
using IS7024_GroupProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IS7024_GroupProject.Services.GoogleBooks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace IS7024_GroupProject.Pages;

public class IndexModel : PageModel
{
    private readonly IGoogleBooksService googleBooksService;
    private readonly UserManager<User>? userManager;
    private readonly ReadingListStore? readingListStore;

    [ActivatorUtilitiesConstructor]
    public IndexModel(
        IGoogleBooksService googleBooksService,
        UserManager<User> userManager,
        ReadingListStore readingListStore)
    {
        this.googleBooksService = googleBooksService;
        this.userManager = userManager;
        this.readingListStore = readingListStore;
    }

    public IndexModel(IGoogleBooksService googleBooksService)
    {
        this.googleBooksService = googleBooksService;
    }

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
        if (userManager is null || readingListStore is null)
        {
            return Challenge();
        }

        if (!TryValidateModel(AddToListInput))
        {
            return BadRequest();
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        bool added = await readingListStore.AddBookAsync(
            user.Id,
            AddToListInput.Title,
            AddToListInput.Author,
            AddToListInput.GoogleBooksId,
            cancellationToken,
            AddToListInput.CoverUrl);

        if (!added)
        {
            StatusMessage = "This book is already on your list.";
            return RedirectToPage(new { Query });
        }

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
