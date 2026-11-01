namespace IS7024_GroupProject.Models;

public sealed class ReviewLike
{
    public int ReviewLikeId { get; set; }

    public int BookReviewId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public BookReview BookReview { get; set; } = null!;

    public User User { get; set; } = null!;
}