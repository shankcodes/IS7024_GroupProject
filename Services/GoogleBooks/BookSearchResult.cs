
namespace IS7024_GroupProject.Services.GoogleBooks;

public enum BookSearchStatus
{
    Success,            // At least one book was found.
    NoResults,          // Google Books answered, but nothing matched.
    EmptyQuery,         // The search box was empty; no API call was made.
    ServiceUnavailable  // Google Books returned an error or timed out (BN-12 falls back to Open Library).
}

// What GoogleBooksService.SearchAsync returns. The page only needs to show Message and Books.
public class BookSearchResult
{
    public const string EmptyQueryMessage = "Please enter a title or author.";
    public const string NoResultsMessage = "No books found.";
    public const string ServiceUnavailableMessage = "Book search is not available right now. Please try again later.";

    public BookSearchStatus Status { get; private set; }

    public List<BookSearchItem> Books { get; private set; } = new();

    // Text to show the reader, or null when books were found.
    public string? Message { get; private set; }

    public bool IsSuccess => Status == BookSearchStatus.Success;

    public static BookSearchResult Success(List<BookSearchItem> books) =>
        new() { Status = BookSearchStatus.Success, Books = books };

    public static BookSearchResult NoResults() =>
        new() { Status = BookSearchStatus.NoResults, Message = NoResultsMessage };

    public static BookSearchResult EmptyQuery() =>
        new() { Status = BookSearchStatus.EmptyQuery, Message = EmptyQueryMessage };

    public static BookSearchResult ServiceUnavailable() =>
        new() { Status = BookSearchStatus.ServiceUnavailable, Message = ServiceUnavailableMessage };
}
