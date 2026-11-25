using System.Security.Claims;
using IS7024_GroupProject.Data;
using IS7024_GroupProject.Models;
using IS7024_GroupProject.Pages;
using IS7024_GroupProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace IS7024_GroupProject.Tests.Pages;

public sealed class ReadingListModelTests
{
    [Fact]
    public async Task OnGetAsync_LoadsOnlyTheAuthenticatedReadersEntries()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var reader = CreateUser(1);
        var otherReader = CreateUser(2);
        var readerBook = CreateBook("The Martian", "the-martian-page");
        var otherBook = CreateBook("Dune", "dune-page");
        context.Users.AddRange(reader, otherReader);
        context.Books.AddRange(readerBook, otherBook);
        context.ReadingLists.AddRange(
            new ReadingList { User = reader, Book = readerBook, Status = ReadingStatus.Reading },
            new ReadingList { User = otherReader, Book = otherBook, Status = ReadingStatus.Finished, FinishedDate = DateTime.Today });
        await context.SaveChangesAsync();

        var model = CreateModel(context, reader);

        await model.OnGetAsync(CancellationToken.None);

        var entry = Assert.Single(model.Entries);
        Assert.Equal("The Martian", entry.Book.Title);
        Assert.Equal(ReadingStatus.Reading, entry.Status);
    }

    [Fact]
    public async Task OnPostUpdateStatusAsync_UpdatesOwnedEntryAndFinishedDate()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var reader = CreateUser(1);
        var book = CreateBook("The Martian", "the-martian-update-page");
        var entry = new ReadingList { User = reader, Book = book, Status = ReadingStatus.WantToRead };
        context.Users.Add(reader);
        context.Books.Add(book);
        context.ReadingLists.Add(entry);
        await context.SaveChangesAsync();

        var model = CreateModel(context, reader);
        model.StatusInput = new ReadingListModel.StatusFormInput
        {
            ReadingListId = entry.ReadingListId,
            Status = ReadingStatus.Reading
        };

        var readingResult = await model.OnPostUpdateStatusAsync(CancellationToken.None);

        Assert.IsType<RedirectToPageResult>(readingResult);
        Assert.Equal(ReadingStatus.Reading, entry.Status);
        Assert.Null(entry.FinishedDate);

        model.StatusInput.Status = ReadingStatus.Finished;
        var finishedResult = await model.OnPostUpdateStatusAsync(CancellationToken.None);

        Assert.IsType<RedirectToPageResult>(finishedResult);
        Assert.Equal(ReadingStatus.Finished, entry.Status);
        Assert.Equal(DateTime.Today, entry.FinishedDate);

        model.StatusInput.Status = ReadingStatus.WantToRead;
        await model.OnPostUpdateStatusAsync(CancellationToken.None);

        Assert.Equal(ReadingStatus.WantToRead, entry.Status);
        Assert.Null(entry.FinishedDate);
    }

    [Fact]
    public async Task OnPostUpdateStatusAsync_RejectsInvalidStatusAndOtherReadersEntry()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var reader = CreateUser(1);
        var otherReader = CreateUser(2);
        var book = CreateBook("The Martian", "the-martian-owner-page");
        var entry = new ReadingList { User = otherReader, Book = book, Status = ReadingStatus.WantToRead };
        context.Users.AddRange(reader, otherReader);
        context.Books.Add(book);
        context.ReadingLists.Add(entry);
        await context.SaveChangesAsync();

        var model = CreateModel(context, reader);
        model.StatusInput = new ReadingListModel.StatusFormInput
        {
            ReadingListId = entry.ReadingListId,
            Status = (ReadingStatus)999
        };

        var invalidResult = await model.OnPostUpdateStatusAsync(CancellationToken.None);

        Assert.IsType<PageResult>(invalidResult);
        Assert.False(model.ModelState.IsValid);
        Assert.Equal(ReadingStatus.WantToRead, entry.Status);

        model.ModelState.Clear();
        model.StatusInput.Status = ReadingStatus.Reading;
        var ownershipResult = await model.OnPostUpdateStatusAsync(CancellationToken.None);

        Assert.IsType<RedirectToPageResult>(ownershipResult);
        Assert.Equal("That book could not be found on your list.", model.StatusMessage);
        Assert.Equal(ReadingStatus.WantToRead, entry.Status);
    }

    [Fact]
    public async Task OnPostUpdateStatusAsync_WhenSignedOutChallenges()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var model = CreateModel(context, new ClaimsPrincipal(new ClaimsIdentity()));
        model.StatusInput = new ReadingListModel.StatusFormInput
        {
            ReadingListId = 1,
            Status = ReadingStatus.Reading
        };

        var result = await model.OnPostUpdateStatusAsync(CancellationToken.None);

        Assert.IsType<ChallengeResult>(result);
    }

    [Fact]
    public async Task OnPostRemoveAsync_RemovesOwnedEntryAndChallengesSignedOutReader()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var reader = CreateUser(1);
        var book = CreateBook("The Martian", "the-martian-remove-page");
        var entry = new ReadingList { User = reader, Book = book, Status = ReadingStatus.Reading };
        context.Users.Add(reader);
        context.Books.Add(book);
        context.ReadingLists.Add(entry);
        await context.SaveChangesAsync();

        var model = CreateModel(context, reader);
        model.RemoveInput = new ReadingListModel.RemoveFormInput { ReadingListId = entry.ReadingListId };

        var result = await model.OnPostRemoveAsync(CancellationToken.None);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("Removed from your list.", model.StatusMessage);
        Assert.False(await context.ReadingLists.AnyAsync(item => item.ReadingListId == entry.ReadingListId));
        Assert.True(await context.Books.AnyAsync(item => item.BookId == book.BookId));

        var signedOutModel = CreateModel(context, new ClaimsPrincipal(new ClaimsIdentity()));
        signedOutModel.RemoveInput = new ReadingListModel.RemoveFormInput { ReadingListId = entry.ReadingListId };

        var signedOutResult = await signedOutModel.OnPostRemoveAsync(CancellationToken.None);

        Assert.IsType<ChallengeResult>(signedOutResult);
    }

    [Fact]
    public async Task OnPostRemoveAsync_RejectsInvalidEntryId()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var reader = CreateUser(1);
        context.Users.Add(reader);
        await context.SaveChangesAsync();

        var model = CreateModel(context, reader);
        model.RemoveInput = new ReadingListModel.RemoveFormInput { ReadingListId = 0 };
        model.ModelState.AddModelError("RemoveInput.ReadingListId", "The field is invalid.");

        var result = await model.OnPostRemoveAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
    }

    private static ReadingListModel CreateModel(ApplicationDbContext dbContext, User user)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())],
            authenticationType: "Test"));
        return CreateModel(dbContext, principal);
    }

    private static ReadingListModel CreateModel(ApplicationDbContext dbContext, ClaimsPrincipal principal)
    {
        var userManager = new UserManager<User>(
            new UserStore<User, IdentityRole<int>, ApplicationDbContext, int>(dbContext),
            Options.Create(new IdentityOptions()),
            new PasswordHasher<User>(),
            [new UserValidator<User>()],
            [new PasswordValidator<User>()],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<UserManager<User>>.Instance);
        var model = new ReadingListModel(userManager, new ReadingListStore(dbContext));
        var services = new ServiceCollection()
            .AddLogging()
            .AddMvcCore()
            .AddDataAnnotations()
            .Services
            .BuildServiceProvider();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
            User = principal
        };
        var pageContext = new PageContext
        {
            HttpContext = httpContext
        };
        pageContext.ViewData = new ViewDataDictionary(
            new EmptyModelMetadataProvider(),
            pageContext.ModelState);
        model.PageContext = pageContext;
        return model;
    }

    private static Book CreateBook(string title, string googleBooksId) => new()
    {
        Title = title,
        Author = "Andy Weir",
        GoogleBooksId = googleBooksId
    };

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

    private static User CreateUser(int userId) => new()
    {
        Id = userId,
        UserName = $"reader{userId}@example.com",
        NormalizedUserName = $"READER{userId}@EXAMPLE.COM",
        Email = $"reader{userId}@example.com",
        NormalizedEmail = $"READER{userId}@EXAMPLE.COM",
        SecurityStamp = Guid.NewGuid().ToString()
    };
}
