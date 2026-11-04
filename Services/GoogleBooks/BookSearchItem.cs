namespace IS7024_GroupProject.Services.GoogleBooks;

// One book in the Google Books search results.
// This is not a database entity: when a reader clicks "Add to List" (BN-9),
// these values are copied into the Book entity from Models/Book.cs.
public class BookSearchItem
{
    // ISBN-13 if Google Books has one, otherwise the ISBN-10.
    // BN-12 uses it to look the same book up in Open Library.
    public string Isbn13 { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? CoverUrl { get; set; }

    // Matches Book.GoogleBooksId, which is unique in the Books table.
    public string GoogleBooksId { get; set; } = string.Empty;
}
