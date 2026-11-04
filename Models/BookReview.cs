using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace IS7024_GroupProject.Models;

[Index(nameof(UserId), nameof(BookId), IsUnique = true)]
public sealed class BookReview
{
    public int BookReviewId { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    [Range(1, 5)]
    public int? Rating { get; set; }

    [MaxLength(200)]
    public string? Summary { get; set; }

    [MaxLength(4000)]
    public string? Review { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ReviewModerationStatus ModerationStatus { get; set; } = ReviewModerationStatus.Pending;

    [MaxLength(1000)]
    public string? ModerationReason { get; set; }

    public DateTime? ModeratedAt { get; set; }

    public ICollection<ReviewComment> Comments { get; set; } = [];

    public ICollection<ReviewLike> Likes { get; set; } = [];

    public User User { get; set; } = null!;

    public Book Book { get; set; } = null!;
}