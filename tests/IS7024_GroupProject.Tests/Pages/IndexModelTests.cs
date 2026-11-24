using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using IS7024_GroupProject.Data;
using IS7024_GroupProject.Models;
using IS7024_GroupProject.Pages;
using IS7024_GroupProject.Services.GoogleBooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IS7024_GroupProject.Tests;

public sealed class IndexModelTests
{
    [Theory]
    [InlineData("The Hobbit")]
    [InlineData("Andy Weir")]
    [InlineData("fantasy")]
    [InlineData("reviewed by readers")]
    public async Task OnGetAsync_WithQuery_ForwardsQueryAndExposesServiceResults(string query)
    {
        await using SqliteConnection connection = await CreateConnectionAsync();
        await using var dbContext = CreateContext(connection);
        var service = new FakeGoogleBooksService
        {
            Result = BookSearchResult.Success(
            [
                new BookSearchItem
                {
                    Title = "The Hobbit",
                    Author = "J.R.R. Tolkien",
                    Isbn13 = "9780547928227",
                    CoverUrl = "https://books.google.com/cover.jpg",
                    GoogleBooksId = "hobbit"
                }
            ])
        };
        var model = CreateModel(dbContext, service);
        model.Query = query;

        await model.OnGetAsync(CancellationToken.None);

        Assert.True(model.HasSearched);
        Assert.Equal(query, service.LastQuery);
        Assert.Equal("The Hobbit", Assert.Single(model.SearchResult!.Books).Title);
    }

    [Fact]
    public async Task OnGetAsync_WithEmptyQuery_ShowsMessageWithoutCallingService()
    {
        await using SqliteConnection connection = await CreateConnectionAsync();
        await using var dbContext = CreateContext(connection);
        var service = new FakeGoogleBooksService();
        var model = CreateModel(dbContext, service);
        model.Query = "   ";

        await model.OnGetAsync(CancellationToken.None);

        Assert.True(model.HasSearched);
        Assert.Equal(BookSearchStatus.EmptyQuery, model.SearchResult!.Status);
        Assert.Equal(BookSearchResult.EmptyQueryMessage, model.SearchResult.Message);
        Assert.Null(service.LastQuery);
    }

    [Theory]
    [InlineData(BookSearchStatus.NoResults, BookSearchResult.NoResultsMessage)]
    [InlineData(BookSearchStatus.ServiceUnavailable, BookSearchResult.ServiceUnavailableMessage)]
    public async Task OnGetAsync_PreservesExistingSearchFeedback(BookSearchStatus status, string message)
    {
        await using SqliteConnection connection = await CreateConnectionAsync();
        await using var dbContext = CreateContext(connection);
        var service = new FakeGoogleBooksService
        {
            Result = status == BookSearchStatus.NoResults
                ? BookSearchResult.NoResults()
                : BookSearchResult.ServiceUnavailable()
        };
        var model = CreateModel(dbContext, service);
        model.Query = "unknown title";

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(status, model.SearchResult!.Status);
        Assert.Equal(message, model.SearchResult.Message);
    }

    [Fact]
    public async Task OnGetAsync_AuthenticatedReader_LoadsOnlyTheirBooksAndYearlyGoal()
    {
        await using SqliteConnection connection = await CreateConnectionAsync();
        await using var dbContext = CreateContext(connection);
        int year = DateTime.Today.Year;
        User reader = CreateUser(1);
        User otherReader = CreateUser(2);
        Book readingBook = CreateBook("Reading now", "reading-now");
        Book nextBook = CreateBook("Read next", "read-next");
        Book finishedBook = CreateBook("Finished this year", "finished-this-year");
        Book februaryBook = CreateBook("Finished in February", "finished-february");
        Book undatedBook = CreateBook("Undated", "undated");
        Book oldBook = CreateBook("Finished earlier", "finished-earlier");
        Book futureBook = CreateBook("Finished next year", "finished-next-year");
        Book otherReadingBook = CreateBook("Other reader is reading", "other-reading");
        Book otherNextBook = CreateBook("Other reader wants to read", "other-next");
        Book otherBook = CreateBook("Other reader book", "other-reader-book");
        dbContext.Users.AddRange(reader, otherReader);
        dbContext.Books.AddRange(readingBook, nextBook, finishedBook, februaryBook, undatedBook, oldBook, futureBook, otherReadingBook, otherNextBook, otherBook);
        dbContext.ReadingLists.AddRange(
            CreateEntry(reader, readingBook, ReadingStatus.Reading),
            CreateEntry(reader, nextBook, ReadingStatus.WantToRead),
            CreateEntry(reader, finishedBook, ReadingStatus.Finished, new DateTime(year, 1, 1)),
            CreateEntry(reader, februaryBook, ReadingStatus.Finished, new DateTime(year, 2, 1)),
            CreateEntry(reader, undatedBook, ReadingStatus.Finished),
            CreateEntry(reader, oldBook, ReadingStatus.Finished, new DateTime(year - 1, 12, 31)),
            CreateEntry(reader, futureBook, ReadingStatus.Finished, new DateTime(year + 1, 1, 1)),
            CreateEntry(otherReader, otherReadingBook, ReadingStatus.Reading),
            CreateEntry(otherReader, otherNextBook, ReadingStatus.WantToRead),
            CreateEntry(otherReader, otherBook, ReadingStatus.Finished, new DateTime(year, 2, 1)));
        dbContext.ReadingGoals.Add(new ReadingGoal { UserId = reader.Id, Year = year, TargetBooks = 4 });
        await dbContext.SaveChangesAsync();

        var model = CreateModel(dbContext, new FakeGoogleBooksService());
        SetAuthenticatedUser(model, reader.Id);

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(year, model.CurrentYear);
        Assert.Equal("Reading now", Assert.Single(model.CurrentlyReading).Book.Title);
        Assert.Equal("Read next", Assert.Single(model.ReadNext).Book.Title);
        Assert.Equal(2, model.RecentlyFinished.Count);
        Assert.Equal([2, 1], model.RecentlyFinishedByMonth.Select(group => group.Key).ToArray());
        Assert.Equal("Finished in February", Assert.Single(model.RecentlyFinishedByMonth[0]).Book.Title);
        Assert.Equal("Finished this year", Assert.Single(model.RecentlyFinishedByMonth[1]).Book.Title);
        Assert.Equal(2, model.FinishedBookCount);
        Assert.Equal(4, model.CurrentGoal!.TargetBooks);
        Assert.Equal(50, model.GoalProgressPercentage);
    }

    [Fact]
    public async Task OnGetAsync_ReaderWithoutGoalShowsNoTargetOrProgress()
    {
        await using SqliteConnection connection = await CreateConnectionAsync();
        await using var dbContext = CreateContext(connection);
        User reader = CreateUser(1);
        dbContext.Users.Add(reader);
        await dbContext.SaveChangesAsync();
        var model = CreateModel(dbContext, new FakeGoogleBooksService());
        SetAuthenticatedUser(model, reader.Id);

        await model.OnGetAsync(CancellationToken.None);

        Assert.Null(model.CurrentGoal);
        Assert.Equal(DateTime.Today.Year, model.CurrentYear);
        Assert.Equal(0, model.FinishedBookCount);
        Assert.Equal(0, model.GoalProgressPercentage);
        Assert.Empty(model.CurrentlyReading);
        Assert.Empty(model.ReadNext);
        Assert.Empty(model.RecentlyFinished);
        Assert.Empty(model.RecentlyFinishedByMonth);
    }

    [Fact]
    public async Task OnGetAsync_SignedOutReaderDoesNotLoadPersonalData()
    {
        await using SqliteConnection connection = await CreateConnectionAsync();
        await using var dbContext = CreateContext(connection);
        var model = CreateModel(dbContext, new FakeGoogleBooksService());

        await model.OnGetAsync(CancellationToken.None);

        Assert.Empty(model.CurrentlyReading);
        Assert.Empty(model.ReadNext);
        Assert.Empty(model.RecentlyFinished);
        Assert.Null(model.CurrentGoal);
        Assert.Equal(0, model.FinishedBookCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a url")]
    [InlineData("/relative/cover.jpg")]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:image/png;base64,abc")]
    public void CreateCoverImage_InvalidUrlRendersPlaceholder(string? coverUrl)
    {
        string renderedImage = Render(IndexModel.CreateCoverImage(coverUrl, "A title", "book-cover"));

        Assert.Contains("src=\"/images/no-cover.svg\"", renderedImage);
        Assert.Contains("alt=\"Cover of A title\"", renderedImage);
        Assert.Contains("onerror=\"this.onerror=null;this.src='/images/no-cover.svg';\"", renderedImage);
    }

    [Fact]
    public void CreateCoverImage_ValidUrlRendersCoverAndNonRetryingFallback()
    {
        string renderedImage = Render(IndexModel.CreateCoverImage("https://covers.example.org/book.jpg", "A title", "book-cover"));

        Assert.Contains("src=\"https://covers.example.org/book.jpg\"", renderedImage);
        Assert.Contains("onerror=\"this.onerror=null;this.src='/images/no-cover.svg';\"", renderedImage);
    }

    [Fact]
    public async Task OnGetAsync_GoalProgressRoundsHalfUpAndCapsAtOneHundred()
    {
        await using SqliteConnection connection = await CreateConnectionAsync();
        await using var dbContext = CreateContext(connection);
        int userId = 1;
        int year = DateTime.Today.Year;
        User reader = CreateUser(userId);
        Book firstBook = CreateBook("First finished", "first-finished");
        dbContext.Users.Add(reader);
        dbContext.Books.Add(firstBook);
        dbContext.ReadingLists.Add(CreateEntry(reader, firstBook, ReadingStatus.Finished, new DateTime(year, 1, 1)));
        dbContext.ReadingGoals.Add(new ReadingGoal { UserId = userId, Year = year, TargetBooks = 8 });
        await dbContext.SaveChangesAsync();
        var halfwayModel = CreateModel(dbContext, new FakeGoogleBooksService());
        SetAuthenticatedUser(halfwayModel, userId);

        await halfwayModel.OnGetAsync(CancellationToken.None);

        Assert.Equal(13, halfwayModel.GoalProgressPercentage);

        for (int index = 2; index <= 9; index++)
        {
            Book book = CreateBook($"Finished {index}", $"finished-{index}");
            dbContext.Books.Add(book);
            dbContext.ReadingLists.Add(CreateEntry(reader, book, ReadingStatus.Finished, new DateTime(year, 1, index)));
        }
        dbContext.ReadingGoals.Single().TargetBooks = 4;
        await dbContext.SaveChangesAsync();
        var cappedModel = CreateModel(dbContext, new FakeGoogleBooksService());
        SetAuthenticatedUser(cappedModel, userId);

        await cappedModel.OnGetAsync(CancellationToken.None);

        Assert.Equal(9, cappedModel.FinishedBookCount);
        Assert.Equal(100, cappedModel.GoalProgressPercentage);
    }

    [Fact]
    public async Task OnPostSaveGoalAsync_InvalidTargetPreservesExistingGoal()
    {
        await using SqliteConnection connection = await CreateConnectionAsync();
        await using var dbContext = CreateContext(connection);
        int userId = 1;
        int year = DateTime.Today.Year;
        dbContext.Users.Add(CreateUser(userId));
        dbContext.ReadingGoals.Add(new ReadingGoal { UserId = userId, Year = year, TargetBooks = 12 });
        await dbContext.SaveChangesAsync();
        var model = CreateModel(dbContext, new FakeGoogleBooksService());
        model.GoalTarget = 0;
        SetAuthenticatedUser(model, userId);

        var result = await model.OnPostSaveGoalAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal(IndexModel.InvalidGoalMessage, Assert.Single(model.ModelState[nameof(model.GoalTarget)]!.Errors).ErrorMessage);
        Assert.Equal(12, (await dbContext.ReadingGoals.SingleAsync()).TargetBooks);
    }

    [Fact]
    public async Task OnPostSaveGoalAsync_ValidTargetCreatesGoalForCurrentUser()
    {
        await using SqliteConnection connection = await CreateConnectionAsync();
        await using var dbContext = CreateContext(connection);
        int userId = 1;
        dbContext.Users.Add(CreateUser(userId));
        await dbContext.SaveChangesAsync();
        var model = CreateModel(dbContext, new FakeGoogleBooksService());
        model.GoalTarget = 8;
        SetAuthenticatedUser(model, userId);

        var result = await model.OnPostSaveGoalAsync(CancellationToken.None);

        Assert.IsType<RedirectToPageResult>(result);
        ReadingGoal goal = await dbContext.ReadingGoals.SingleAsync();
        Assert.Equal(userId, goal.UserId);
        Assert.Equal(DateTime.Today.Year, goal.Year);
        Assert.Equal(8, goal.TargetBooks);
    }

    [Fact]
    public async Task OnPostSaveGoalAsync_ValidTargetUpdatesOnlyCurrentYearGoal()
    {
        await using SqliteConnection connection = await CreateConnectionAsync();
        await using var dbContext = CreateContext(connection);
        int userId = 1;
        int year = DateTime.Today.Year;
        dbContext.Users.Add(CreateUser(userId));
        dbContext.ReadingGoals.Add(new ReadingGoal { UserId = userId, Year = year, TargetBooks = 4 });
        await dbContext.SaveChangesAsync();
        var model = CreateModel(dbContext, new FakeGoogleBooksService());
        model.GoalTarget = 9;
        SetAuthenticatedUser(model, userId);

        var result = await model.OnPostSaveGoalAsync(CancellationToken.None);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal(9, (await dbContext.ReadingGoals.SingleAsync()).TargetBooks);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("https://")]
    [InlineData("ftp://covers.example.org/book.jpg")]
    [InlineData("file:///tmp/cover.jpg")]
    public void GetUsableCoverUrl_InvalidAbsoluteUrlUsesPlaceholder(string? coverUrl)
    {
        Assert.Equal(IndexModel.NoCoverUrl, IndexModel.GetUsableCoverUrl(coverUrl));
    }

    [Fact]
    public void GetUsableCoverUrl_ValidHttpAndHttpsUrlsArePreserved()
    {
        Assert.Equal("https://covers.example.org/book.jpg", IndexModel.GetUsableCoverUrl("https://covers.example.org/book.jpg"));
        Assert.Equal("http://covers.example.org/book.jpg", IndexModel.GetUsableCoverUrl("http://covers.example.org/book.jpg"));
    }

    private static async Task<SqliteConnection> CreateConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        return connection;
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static IndexModel CreateModel(ApplicationDbContext dbContext, IGoogleBooksService service)
    {
        var model = new IndexModel(dbContext, service)
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
        };
        return model;
    }

    private static void SetAuthenticatedUser(IndexModel model, int userId)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test"))
        };
        model.PageContext = new PageContext { HttpContext = httpContext };
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

    private static Book CreateBook(string title, string googleBooksId) => new()
    {
        Title = title,
        Author = "Test Author",
        GoogleBooksId = googleBooksId,
        CoverUrl = null
    };

    private static ReadingList CreateEntry(User user, Book book, ReadingStatus status, DateTime? finishedDate = null) => new()
    {
        User = user,
        Book = book,
        Status = status,
        FinishedDate = finishedDate
    };

    private static string Render(TagBuilder tag)
    {
        using var writer = new StringWriter();
        tag.WriteTo(writer, HtmlEncoder.Default);
        return WebUtility.HtmlDecode(writer.ToString());
    }

    [Fact]
    public async Task OnPostAddToListAsync_WhenSignedOutChallengesAndDoesNotCreateEntries()
    {
        await using SqliteConnection connection = await CreateConnectionAsync();
        await using var dbContext = CreateContext(connection);
        var model = CreateModelWithUser(dbContext, new ClaimsPrincipal(new ClaimsIdentity()));

        var result = await model.OnPostAddToListAsync(CancellationToken.None);

        Assert.IsType<ChallengeResult>(result);
        Assert.Empty(await dbContext.Books.ToListAsync());
        Assert.Empty(await dbContext.ReadingLists.ToListAsync());
    }

    [Fact]
    public async Task OnPostAddToListAsync_WhenInputIsInvalidReturnsBadRequest()
    {
        await using SqliteConnection connection = await CreateConnectionAsync();
        await using var dbContext = CreateContext(connection);
        dbContext.Users.Add(CreateUser(1));
        await dbContext.SaveChangesAsync();

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "1")],
            authenticationType: "Test"));
        var model = CreateModelWithUser(dbContext, user);
        model.AddToListInput = new IndexModel.BookInput();

        var result = await model.OnPostAddToListAsync(CancellationToken.None);

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(await dbContext.Books.ToListAsync());
        Assert.Empty(await dbContext.ReadingLists.ToListAsync());
    }

    private static IndexModel CreateModelWithUser(ApplicationDbContext dbContext, ClaimsPrincipal user)
    {
        var model = new IndexModel(dbContext, new FakeGoogleBooksService());
        var services = new ServiceCollection()
            .AddLogging()
            .AddMvcCore()
            .AddDataAnnotations()
            .Services
            .BuildServiceProvider();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
            User = user
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

    private sealed class FakeGoogleBooksService : IGoogleBooksService
    {
        public string? LastQuery { get; private set; }

        public BookSearchResult Result { get; init; } = BookSearchResult.NoResults();

        public Task<BookSearchResult> SearchAsync(string? query, CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(Result);
        }
    }
}
