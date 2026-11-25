
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
    public StatusFormInput StatusInput { get; set; } = new();

    [BindProperty]
    public RemoveFormInput RemoveInput { get; set; } = new();

    // Load the signed-in user's reading list
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadEntriesAsync(cancellationToken);
    }

    // Update the reading status of a saved book
    public async Task<IActionResult> OnPostUpdateStatusAsync(
        CancellationToken cancellationToken)
    {
        // Both forms are bound on every POST; validate only this form's input below
        ModelState.Clear();

        // Validate the selected book
        if (StatusInput.ReadingListId < 1)
        {
            ModelState.AddModelError(
                "StatusInput.ReadingListId",
                "Invalid book selection.");
        }

        // Validate the selected reading status
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

    // Remove a book from the signed-in user's list
    public async Task<IActionResult> OnPostRemoveAsync(
        CancellationToken cancellationToken)
    {
        // Both forms are bound on every POST; validate only this form's input below
        ModelState.Clear();

        // Validate the selected book
        if (RemoveInput.ReadingListId < 1)
        {
            ModelState.AddModelError(
                "RemoveInput.ReadingListId",
                "Invalid book selection.");
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

        bool removed = await readingListStore.RemoveBookAsync(
            user.Id,
            RemoveInput.ReadingListId,
            cancellationToken);

        StatusMessage = removed
            ? "Removed from your list."
            : "That book could not be found on your list.";

        return RedirectToPage();
    }

    // Remove validation entries belonging to an unrelated form
    private void RemoveUnrelatedModelState(string prefix)
    {
        var keys = ModelState.Keys
            .Where(key =>
                key == prefix ||
                key.StartsWith(
                    prefix + ".",
                    StringComparison.Ordinal))
            .ToList();

        foreach (var key in keys)
        {
            ModelState.Remove(key);
        }
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
    