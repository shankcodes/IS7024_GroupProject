
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

    [BindProperty]
    public StatusFormInput StatusInput { get; set; } = new();

    [BindProperty]
    public RemoveFormInput RemoveInput { get; set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadEntriesAsync(cancellationToken);
    }

    // Add a book to My List
    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        // Validate only the Add Book form
        ModelState.Remove(nameof(StatusInput));
        ModelState.Remove(nameof(RemoveInput));

        if (!TryValidateModel(Input, nameof(Input)))
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

        StatusMessage = added
            ? "Added to your list."
            : "This book is already on your list.";

        return RedirectToPage();
    }

    // Update the status of a saved book
    public async Task<IActionResult> OnPostUpdateStatusAsync(
        CancellationToken cancellationToken)
    {
        // Clear unrelated form validation
        ModelState.Clear();

        if (StatusInput.ReadingListId < 1)
        {
            ModelState.AddModelError(
                "StatusInput.ReadingListId",
                "Invalid book selection.");
        }

        if (StatusInput.Status is null ||
            !Enum.IsDefined(StatusInput.Status.Value))
        {
            ModelState.AddModelError(
                "StatusInput.Status",
                "Select a valid reading status.");
        }

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

        bool updated = await readingListStore.UpdateStatusAsync(
            user.Id,
            StatusInput.ReadingListId,
            StatusInput.Status!.Value,
            cancellationToken);

        StatusMessage = updated
            ? "Reading status updated."
            : "That book could not be found on your list.";

        return RedirectToPage();
    }

    // Remove a saved book
    public async Task<IActionResult> OnPostRemoveAsync(
        CancellationToken cancellationToken)
    {
        // Clear unrelated form validation
        ModelState.Clear();

        if (RemoveInput.ReadingListId < 1)
        {
            ModelState.AddModelError(
                "RemoveInput.ReadingListId",
                "Invalid book selection.");

            await LoadEntriesAsync(cancellationToken);
            return Page();
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        bool removed = await readingListStore.RemoveBookAsync(
            user.Id,
            RemoveInput.ReadingListId,
            cancellationToken);

        StatusMessage = removed
            ? "Removed from your list."
            : "That book could not be found on your list.";

        return RedirectToPage();
    }

    // Load books belonging to the signed-in user
    private async Task LoadEntriesAsync(
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);

        if (user is not null)
        {
            Entries = await readingListStore.GetForUserAsync(user.Id);
        }
    }

    // Add Book form
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

    // Update Status form
    public sealed class StatusFormInput
    {
        [Range(1, int.MaxValue)]
        public int ReadingListId { get; set; }

        public ReadingStatus? Status { get; set; }
    }

    // Remove Book form
    public sealed class RemoveFormInput
    {
        [Range(1, int.MaxValue)]
        public int ReadingListId { get; set; }
    }
}
    