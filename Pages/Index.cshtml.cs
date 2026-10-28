using IS7024_GroupProject.Models;
using IS7024_GroupProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IS7024_GroupProject.Pages;

public class IndexModel(ShortcutEntryStore entryStore) : PageModel
{
    public IReadOnlyList<ShortcutEntry> Entries { get; private set; } = [];

    [BindProperty]
    public EntryInput Input { get; set; } = new();

    public void OnGet()
    {
        LoadEntries();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            LoadEntries();
            return Page();
        }

        entryStore.Add(new ShortcutEntry
        {
            FirstName = Input.FirstName.Trim(),
            LastName = Input.LastName.Trim(),
            Major = Input.Major.Trim(),
            FavoriteKeyboardShortcut = Input.FavoriteKeyboardShortcut.Trim(),
            ShortcutUsage = Input.ShortcutUsage.Trim()
        });

        return RedirectToPage();
    }

    private void LoadEntries()
    {
        Entries = entryStore.GetAll();
    }

    public sealed class EntryInput
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Major { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Favorite keyboard shortcut")]
        public string FavoriteKeyboardShortcut { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        [Display(Name = "Where can this shortcut be used?")]
        public string ShortcutUsage { get; set; } = string.Empty;
    }
}
