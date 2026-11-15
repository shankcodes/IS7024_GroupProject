using IS7024_GroupProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IS7024_GroupProject.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<Book> Books => Set<Book>();

    public DbSet<ReadingList> ReadingLists => Set<ReadingList>();

    public DbSet<ReadingGoal> ReadingGoals => Set<ReadingGoal>();

    public DbSet<BookReview> BookReviews => Set<BookReview>();

    public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();

    public DbSet<ReviewLike> ReviewLikes => Set<ReviewLike>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ReadingList>()
            .Property(readingList => readingList.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ReadingList>()
            .HasOne(readingList => readingList.User)
            .WithMany(user => user.ReadingLists)
            .HasForeignKey(readingList => readingList.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReadingGoal>()
            .HasOne(readingGoal => readingGoal.User)
            .WithMany(user => user.ReadingGoals)
            .HasForeignKey(readingGoal => readingGoal.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookReview>()
            .HasOne(bookReview => bookReview.User)
            .WithMany(user => user.BookReviews)
            .HasForeignKey(bookReview => bookReview.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookReview>()
            .HasOne(bookReview => bookReview.Book)
            .WithMany(book => book.Reviews)
            .HasForeignKey(bookReview => bookReview.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookReview>()
            .Property(bookReview => bookReview.ModerationStatus)
            .HasConversion<string>();

        modelBuilder.Entity<ReviewComment>()
            .HasOne(reviewComment => reviewComment.BookReview)
            .WithMany(bookReview => bookReview.Comments)
            .HasForeignKey(reviewComment => reviewComment.BookReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReviewComment>()
            .HasOne(reviewComment => reviewComment.User)
            .WithMany(user => user.ReviewComments)
            .HasForeignKey(reviewComment => reviewComment.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReviewComment>()
            .Property(reviewComment => reviewComment.ModerationStatus)
            .HasConversion<string>();

        modelBuilder.Entity<ReviewLike>()
            .HasOne(reviewLike => reviewLike.BookReview)
            .WithMany(bookReview => bookReview.Likes)
            .HasForeignKey(reviewLike => reviewLike.BookReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReviewLike>()
            .HasOne(reviewLike => reviewLike.User)
            .WithMany(user => user.ReviewLikes)
            .HasForeignKey(reviewLike => reviewLike.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}