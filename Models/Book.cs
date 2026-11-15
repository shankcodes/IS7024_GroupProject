using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace IS7024_GroupProject.Models;

[Index(nameof(GoogleBooksId), IsUnique = true)]
public sealed class Book
{
    public int BookId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    public string? CoverUrl { get; set; }

    [Required]
    public string GoogleBooksId { get; set; } = string.Empty;

    public ICollection<ReadingList> ReadingLists { get; set; } = [];

    public ICollection<BookReview> Reviews { get; set; } = [];
}