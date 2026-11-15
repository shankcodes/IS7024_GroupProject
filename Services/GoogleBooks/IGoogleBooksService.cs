namespace IS7024_GroupProject.Services.GoogleBooks;

public interface IGoogleBooksService
{
    // Searches Google Books by title or author. Never throws for API problems;
    // check the returned Status instead.
    Task<BookSearchResult> SearchAsync(string? query, CancellationToken cancellationToken = default);
}
