using System.ComponentModel.DataAnnotations;
using IS7024_GroupProject.Models;
using IS7024_GroupProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IS7024_GroupProject.Pages;

[Authorize]
public sealed class ReadingListModel(
    UserManager<User> userManager,
    ReadingListStore readingListStore) : PageModel
{
    public IReadOnlyList<ReadingList> Entries { get; private set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public BookInput Input { get; set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadEntriesAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadEntriesAsync(cancellationToken);
            return Page();
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        bool added = await readingListStore.AddBookAsync(
            user.Id,
            Input.Title.Trim(),
            Input.Author.Trim(),
            Input.GoogleBooksId.Trim(),
            cancellationToken: cancellationToken);

        StatusMessage = added ? "Added to your list." : "This book is already on your list.";
        return RedirectToPage();
    }

    private async Task LoadEntriesAsync(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is not null)
        {
            Entries = await readingListStore.GetForUserAsync(user.Id);
        }
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
        [Display(Name = "Google Books ID")]
        public string GoogleBooksId { get; set; } = string.Empty;
    }
}