using IS7024_GroupProject.Data;
using IS7024_GroupProject.Models;
using IS7024_GroupProject.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IS7024_GroupProject.Tests;

public sealed class PersistenceTests
{
    [Fact]
    public async Task CoreTablesAreAvailableInDbContext()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        Assert.NotNull(context.Books);
        Assert.NotNull(context.ReadingLists);
        Assert.NotNull(context.ReadingGoals);
        Assert.True(await context.Database.CanConnectAsync());
    }

    [Fact]
    public async Task ReadingListEntryPersistsAcrossContexts()
    {
        await using var connection = await CreateConnectionAsync();

        await using (var context = await CreateContextAsync(connection))
        {
            var user = CreateUser(1);
            var book = new Book
            {
                Title = "The Martian",
                Author = "Andy Weir",
                GoogleBooksId = "the-martian-test"
            };

            context.Users.Add(user);
            context.Books.Add(book);
            context.ReadingLists.Add(new ReadingList
            {
                User = user,
                Book = book,
                Status = ReadingStatus.WantToRead
            });

            await context.SaveChangesAsync();
        }

        await using (var restartedContext = await CreateContextAsync(connection))
        {
            var readingListEntry = await restartedContext.ReadingLists
                .Include(entry => entry.Book)
                .SingleAsync();

            Assert.Equal("The Martian", readingListEntry.Book.Title);
            Assert.Equal(ReadingStatus.WantToRead, readingListEntry.Status);
        }
    }

    [Fact]
    public async Task BookAndReadingGoalCanBePersisted()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var user = CreateUser(1);
        var book = new Book
        {
            Title = "The Hobbit",
            Author = "J.R.R. Tolkien",
            GoogleBooksId = "the-hobbit-test"
        };

        context.Users.Add(user);
        context.Books.Add(book);
        context.ReadingGoals.Add(new ReadingGoal
        {
            User = user,
            Year = DateTime.Today.Year,
            TargetBooks = 12
        });

        await context.SaveChangesAsync();

        Assert.Single(await context.Books.ToListAsync());
        Assert.Equal(12, (await context.ReadingGoals.SingleAsync()).TargetBooks);
    }

    [Fact]
    public async Task AddBookToList_CreatesWantToReadEntryWithCover()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var user = CreateUser(1);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var store = new Services.ReadingListStore(context);
        bool added = await store.AddBookAsync(
            user.Id,
            "The Martian",
            "Andy Weir",
            "the-martian",
            coverUrl: "https://covers.example/the-martian.jpg");

        Assert.True(added);
        var entry = await context.ReadingLists.Include(item => item.Book).SingleAsync();
        Assert.Equal("The Martian", entry.Book.Title);
        Assert.Equal("Andy Weir", entry.Book.Author);
        Assert.Equal("https://covers.example/the-martian.jpg", entry.Book.CoverUrl);
        Assert.Equal(ReadingStatus.WantToRead, entry.Status);
    }

    [Fact]
    public async Task AddBookToListTwice_ReturnsFalseAndKeepsOneCopy()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var user = CreateUser(1);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var store = new Services.ReadingListStore(context);
        Assert.True(await store.AddBookAsync(user.Id, "The Martian", "Andy Weir", "the-martian"));
        Assert.False(await store.AddBookAsync(user.Id, "The Martian", "Andy Weir", "the-martian"));

        Assert.Equal(1, await context.Books.CountAsync());
        Assert.Equal(1, await context.ReadingLists.CountAsync());
    }

    [Fact]
    public async Task AddExistingSharedBook_DoesNotOverwriteItsMetadataFromPostedValues()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var user = CreateUser(1);
        var book = new Book
        {
            Title = "Canonical Title",
            Author = "Canonical Author",
            GoogleBooksId = "shared-book",
            CoverUrl = "https://covers.example/canonical.jpg"
        };
        context.Users.Add(user);
        context.Books.Add(book);
        await context.SaveChangesAsync();

        var store = new Services.ReadingListStore(context);
        bool added = await store.AddBookAsync(
            user.Id,
            "Forged Title",
            "Forged Author",
            book.GoogleBooksId,
            coverUrl: "https://attacker.example/cover.jpg");

        Assert.True(added);
        Assert.Equal("Canonical Title", book.Title);
        Assert.Equal("Canonical Author", book.Author);
        Assert.Equal("https://covers.example/canonical.jpg", book.CoverUrl);
    }

    [Fact]
    public async Task DuplicateAdd_DoesNotChangeReadingStatusOrReview()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var user = CreateUser(1);
        var book = new Book
        {
            Title = "The Martian",
            Author = "Andy Weir",
            GoogleBooksId = "the-martian"
        };
        var entry = new ReadingList
        {
            User = user,
            Book = book,
            Status = ReadingStatus.Reading
        };
        var review = new BookReview
        {
            User = user,
            Book = book,
            Rating = 4,
            Summary = "An engaging read",
            Review = "The story kept me interested."
        };
        context.Users.Add(user);
        context.Books.Add(book);
        context.ReadingLists.Add(entry);
        context.BookReviews.Add(review);
        await context.SaveChangesAsync();

        var store = new Services.ReadingListStore(context);
        bool added = await store.AddBookAsync(
            user.Id,
            "Forged Title",
            "Forged Author",
            book.GoogleBooksId);

        Assert.False(added);
        Assert.Equal(ReadingStatus.Reading, entry.Status);
        Assert.Equal(4, review.Rating);
        Assert.Equal("An engaging read", review.Summary);
        Assert.Equal("The story kept me interested.", review.Review);
        Assert.Single(await context.ReadingLists.ToListAsync());
        Assert.Single(await context.BookReviews.ToListAsync());
    }

    [Fact]
    public async Task ReadingListEntryIncludesCoverTitleAndAuthor()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var user = CreateUser(1);
        var book = new Book
        {
            Title = "Dune",
            Author = "Frank Herbert",
            GoogleBooksId = "dune",
            CoverUrl = "https://covers.example/dune.jpg"
        };
        context.Users.Add(user);
        context.Books.Add(book);
        context.ReadingLists.Add(new ReadingList
        {
            User = user,
            Book = book,
            Status = ReadingStatus.WantToRead
        });
        await context.SaveChangesAsync();

        var store = new Services.ReadingListStore(context);
        var entries = await store.GetForUserAsync(user.Id);
        var entry = Assert.Single(entries);

        Assert.Equal("Dune", entry.Book.Title);
        Assert.Equal("Frank Herbert", entry.Book.Author);
        Assert.Equal("https://covers.example/dune.jpg", entry.Book.CoverUrl);
    }

    private static async Task<SqliteConnection> CreateConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        return connection;
    }

    private static async Task<ApplicationDbContext> CreateContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private static User CreateUser(int userId)
    {
        return new User
        {
            Id = userId,
            UserName = $"reader{userId}@example.com",
            NormalizedUserName = $"READER{userId}@EXAMPLE.COM",
            Email = $"reader{userId}@example.com",
            NormalizedEmail = $"READER{userId}@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString()
        };
    }
}
