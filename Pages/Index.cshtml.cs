using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IS7024_GroupProject.Services.GoogleBooks;

namespace IS7024_GroupProject.Pages;

public class IndexModel(IGoogleBooksService googleBooksService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

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
}
