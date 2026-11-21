using IS7024_GroupProject.Data;
using IS7024_GroupProject.Models;
using IS7024_GroupProject.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IS7024_GroupProject.Tests;

public sealed class PersistenceTests
{
    [Fact]
    public async Task CoreTablesAreAvailableInDbContext()
    {
        await using var context = CreateContext("core-tables");

        Assert.NotNull(context.Books);
        Assert.NotNull(context.ReadingLists);
        Assert.NotNull(context.ReadingGoals);
        Assert.True(await context.Database.EnsureCreatedAsync());
    }

    [Fact]
    public async Task ReadingListEntryPersistsAcrossContexts()
    {
        const string databaseName = "reading-list-persistence";

        await using (var context = CreateContext(databaseName))
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

        await using (var restartedContext = CreateContext(databaseName))
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
        await using var context = CreateContext("book-and-goal");
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
        await using var context = CreateContext("add-the-martian");
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
        await using var context = CreateContext("duplicate-book");
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
    public async Task ReadingListEntryIncludesCoverTitleAndAuthor()
    {
        await using var context = CreateContext("dune-reading-list");
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

    private static ApplicationDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options);
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
