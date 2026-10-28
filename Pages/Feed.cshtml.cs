using IS7024_GroupProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IS7024_GroupProject.Pages;

public class FeedModel(ShortcutEntryStore entryStore) : PageModel
{
    public IActionResult OnGet()
    {
        return new JsonResult(entryStore.GetAll());
    }
}
