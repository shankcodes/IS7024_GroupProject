using System.ComponentModel.DataAnnotations;

namespace IS7024_GroupProject.Models;

public sealed class ReviewComment
{
    public int ReviewCommentId { get; set; }

    public int BookReviewId { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ReviewModerationStatus ModerationStatus { get; set; } = ReviewModerationStatus.Pending;

    [MaxLength(1000)]
    public string? ModerationReason { get; set; }

    public DateTime? ModeratedAt { get; set; }

    public BookReview BookReview { get; set; } = null!;

    public User User { get; set; } = null!;
}