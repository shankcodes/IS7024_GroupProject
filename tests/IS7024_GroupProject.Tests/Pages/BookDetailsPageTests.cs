using System.Security.Claims;
using IS7024_GroupProject.Data;
using IS7024_GroupProject.Models;
using IS7024_GroupProject.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IS7024_GroupProject.Tests.Pages;

public sealed class BookDetailsPageTests
{
    [Fact]
    public async Task SaveReview_CreatesAndUpdatesOnlyOneFinishedReadersReview()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var reader = CreateUser(1);
        var book = CreateBook("The Hobbit");
        context.Users.Add(reader);
        context.Books.Add(book);
        context.ReadingLists.Add(new ReadingList
        {
            User = reader,
            Book = book,
            Status = ReadingStatus.Finished,
            FinishedDate = DateTime.Today
        });
        await context.SaveChangesAsync();

        var model = CreateModel(context, reader);
        model.ReviewInput = new BookDetailsModel.ReviewFormInput
        {
            BookId = book.BookId,
            Rating = 4,
            Summary = "Fun adventure",
            Review = "Bilbo's journey kept me hooked"
        };

        var firstResult = await model.OnPostSaveReviewAsync(CancellationToken.None);

        Assert.IsType<RedirectToPageResult>(firstResult);
        Assert.Single(await context.BookReviews.ToListAsync());

        model.ReviewInput.Rating = 5;
        var secondResult = await model.OnPostSaveReviewAsync(CancellationToken.None);

        Assert.IsType<RedirectToPageResult>(secondResult);
        var saved = Assert.Single(await context.BookReviews.ToListAsync());
        Assert.Equal(5, saved.Rating);
        Assert.Equal("Fun adventure", saved.Summary);
        Assert.Equal("Bilbo's journey kept me hooked", saved.Review);
    }

    [Fact]
    public async Task OnGetAsync_CalculatesAverageAndDisablesWantToReadReview()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var alex = CreateUser(1);
        var sam = CreateUser(2);
        var currentReader = CreateUser(3);
        var martian = CreateBook("The Martian");
        context.Users.AddRange(alex, sam, currentReader);
        context.Books.Add(martian);
        context.ReadingLists.Add(new ReadingList
        {
            User = currentReader,
            Book = martian,
            Status = ReadingStatus.WantToRead
        });
        context.BookReviews.AddRange(
            new BookReview { User = alex, Book = martian, Rating = 4 },
            new BookReview { User = sam, Book = martian, Rating = 5 });
        await context.SaveChangesAsync();

        var model = CreateModel(context, currentReader);
        await model.OnGetAsync(martian.BookId, CancellationToken.None);

        Assert.False(model.CanRate);
        Assert.Equal(4.5m, model.AverageRating);
        Assert.Equal(2, model.RatingCount);
    }

    [Fact]
    public async Task OnGetAsync_RoundsAverageToOneDecimalPlace()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var readers = Enumerable.Range(1, 4).Select(CreateUser).ToArray();
        var book = CreateBook("A Wizard of Earthsea");
        context.Users.AddRange(readers);
        context.Books.Add(book);
        context.BookReviews.AddRange(
            new BookReview { User = readers[0], Book = book, Rating = 4 },
            new BookReview { User = readers[1], Book = book, Rating = 4 },
            new BookReview { User = readers[2], Book = book, Rating = 4 },
            new BookReview { User = readers[3], Book = book, Rating = 5 });
        await context.SaveChangesAsync();

        var model = CreateModel(context, readers[0]);
        await model.OnGetAsync(book.BookId, CancellationToken.None);

        Assert.Equal(4.3m, model.AverageRating);
        Assert.Equal(4, model.RatingCount);
    }

    [Fact]
    public async Task OnGetAsync_WithNoRatings_ReturnsZeroAggregate()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var reader = CreateUser(1);
        var book = CreateBook("The Left Hand of Darkness");
        context.Users.Add(reader);
        context.Books.Add(book);
        await context.SaveChangesAsync();

        var model = CreateModel(context, reader);
        await model.OnGetAsync(book.BookId, CancellationToken.None);

        Assert.Equal(0m, model.AverageRating);
        Assert.Equal(0, model.RatingCount);
    }

    [Fact]
    public async Task OnPostSaveReviewAsync_RejectsReaderWhoHasNotFinishedBook()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var reader = CreateUser(1);
        var book = CreateBook("The Martian");
        context.Users.Add(reader);
        context.Books.Add(book);
        context.ReadingLists.Add(new ReadingList
        {
            User = reader,
            Book = book,
            Status = ReadingStatus.WantToRead
        });
        await context.SaveChangesAsync();

        var model = CreateModel(context, reader);
        model.ReviewInput = new BookDetailsModel.ReviewFormInput
        {
            BookId = book.BookId,
            Rating = 5,
            Summary = "Should not save",
            Review = "Not finished"
        };

        var result = await model.OnPostSaveReviewAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.False(await context.BookReviews.AnyAsync());
        Assert.Contains(model.ModelState.Values.SelectMany(value => value.Errors),
            error => error.ErrorMessage == "Finish this book to rate it.");
    }

    [Theory]
    [InlineData(null, 0, 0)]
    [InlineData(0, 0, 0)]
    [InlineData(6, 0, 0)]
    [InlineData(4, 201, 0)]
    [InlineData(4, 0, 4001)]
    public async Task OnPostSaveReviewAsync_RejectsInvalidInputWithoutChangingExistingReview(
        int? rating,
        int summaryLength,
        int reviewLength)
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var (reader, book) = await CreateFinishedBookAsync(context, 1, "The Hobbit");
        context.BookReviews.Add(new BookReview
        {
            User = reader,
            Book = book,
            Rating = 3,
            Summary = "Original summary",
            Review = "Original comment"
        });
        await context.SaveChangesAsync();

        var model = CreateModel(context, reader);
        model.ReviewInput = new BookDetailsModel.ReviewFormInput
        {
            BookId = book.BookId,
            Rating = rating,
            Summary = summaryLength == 0 ? "Changed summary" : new string('s', summaryLength),
            Review = reviewLength == 0 ? "Changed comment" : new string('c', reviewLength)
        };

        var result = await model.OnPostSaveReviewAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.NotEmpty(model.ModelState.Values.SelectMany(value => value.Errors));
        var savedReview = Assert.Single(await context.BookReviews.ToListAsync());
        Assert.Equal(3, savedReview.Rating);
        Assert.Equal("Original summary", savedReview.Summary);
        Assert.Equal("Original comment", savedReview.Review);
    }

    [Fact]
    public async Task OnPostSaveReviewAsync_AcceptsMaximumSummaryAndCommentLengths()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var (reader, book) = await CreateFinishedBookAsync(context, 1, "The Hobbit");
        var model = CreateModel(context, reader);
        model.ReviewInput = new BookDetailsModel.ReviewFormInput
        {
            BookId = book.BookId,
            Rating = 4,
            Summary = new string('s', 200),
            Review = new string('c', 4000)
        };

        var result = await model.OnPostSaveReviewAsync(CancellationToken.None);

        Assert.IsType<RedirectToPageResult>(result);
        var savedReview = Assert.Single(await context.BookReviews.ToListAsync());
        Assert.Equal(200, savedReview.Summary!.Length);
        Assert.Equal(4000, savedReview.Review!.Length);
    }

    [Fact]
    public async Task OnPostSaveReviewAsync_ChallengesUnauthenticatedReader()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var (reader, book) = await CreateFinishedBookAsync(context, 1, "The Hobbit");
        var model = CreateModel(context, reader);
        model.PageContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        model.ReviewInput = new BookDetailsModel.ReviewFormInput
        {
            BookId = book.BookId,
            Rating = 4
        };

        var result = await model.OnPostSaveReviewAsync(CancellationToken.None);

        Assert.IsType<ChallengeResult>(result);
        Assert.False(await context.BookReviews.AnyAsync());
    }

    [Fact]
    public async Task OnGetAsync_LoadsOnlyCurrentReadersReview()
    {
        await using var connection = await CreateConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var alex = CreateUser(1);
        var sam = CreateUser(2);
        var currentReader = CreateUser(3);
        var book = CreateBook("The Hobbit");
        context.Users.AddRange(alex, sam, currentReader);
        context.Books.Add(book);
        context.BookReviews.AddRange(
            new BookReview { User = alex, Book = book, Rating = 4, Summary = "Alex private summary", Review = "Alex private comment" },
            new BookReview { User = sam, Book = book, Rating = 5, Summary = "Sam private summary", Review = "Sam private comment" },
            new BookReview { User = currentReader, Book = book, Rating = 3, Summary = "My summary", Review = "My comment" });
        await context.SaveChangesAsync();

        var model = CreateModel(context, currentReader);
        await model.OnGetAsync(book.BookId, CancellationToken.None);

        Assert.Equal("My summary", model.ReviewInput.Summary);
        Assert.Equal("My comment", model.ReviewInput.Review);
    }

    [Fact]
    public void BookDetailsPageContract_ExposesAccessibleStarsAndRatingStates()
    {
        var page = File.ReadAllText(FindBookDetailsPagePath());

        Assert.Contains("role=\"radiogroup\" aria-label=\"Choose a rating\"", page);
        Assert.Contains("class=\"star-rating-input\"", page);
        Assert.Contains("Model.RatingCount == 1 ? \"rating\" : \"ratings\"", page);
        Assert.Contains("Finish this book to rate it.", page);
        Assert.Contains("disabled=\"@(!Model.CanRate)\"", page);
    }

    private static BookDetailsModel CreateModel(ApplicationDbContext context, User reader)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, reader.Id.ToString())],
            authenticationType: "Test"));
        var model = new BookDetailsModel(context);
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
        var pageContext = new PageContext { HttpContext = httpContext };
        pageContext.ViewData = new ViewDataDictionary(
            new EmptyModelMetadataProvider(),
            pageContext.ModelState);
        model.PageContext = pageContext;
        return model;
    }

    private static async Task<(User Reader, Book Book)> CreateFinishedBookAsync(
        ApplicationDbContext context,
        int userId,
        string title)
    {
        var reader = CreateUser(userId);
        var book = CreateBook(title);
        context.Users.Add(reader);
        context.Books.Add(book);
        context.ReadingLists.Add(new ReadingList
        {
            User = reader,
            Book = book,
            Status = ReadingStatus.Finished,
            FinishedDate = DateTime.Today
        });
        await context.SaveChangesAsync();
        return (reader, book);
    }

    private static Book CreateBook(string title) => new()
    {
        Title = title,
        Author = "Test Author",
        GoogleBooksId = title.ToLowerInvariant().Replace(" ", "-")
    };

    private static User CreateUser(int id) => new()
    {
        Id = id,
        UserName = $"reader{id}@example.com",
        NormalizedUserName = $"READER{id}@EXAMPLE.COM",
        Email = $"reader{id}@example.com",
        NormalizedEmail = $"READER{id}@EXAMPLE.COM",
        SecurityStamp = Guid.NewGuid().ToString()
    };

    private static string FindBookDetailsPagePath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Pages", "BookDetails.cshtml")))
        {
            directory = directory.Parent;
        }

        return directory is null
            ? throw new FileNotFoundException("Could not locate the BookDetails Razor page.")
            : Path.Combine(directory.FullName, "Pages", "BookDetails.cshtml");
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
}
