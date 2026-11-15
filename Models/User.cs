using Microsoft.AspNetCore.Identity;

namespace IS7024_GroupProject.Models;

public sealed class User : IdentityUser<int>
{
    public ICollection<ReadingList> ReadingLists { get; set; } = [];

    public ICollection<ReadingGoal> ReadingGoals { get; set; } = [];

    public ICollection<BookReview> BookReviews { get; set; } = [];

    public ICollection<ReviewComment> ReviewComments { get; set; } = [];

    public ICollection<ReviewLike> ReviewLikes { get; set; } = [];
}