using IS7024_GroupProject.Data;
using IS7024_GroupProject.Models;
using IS7024_GroupProject.Pages;
using IS7024_GroupProject.Services.GoogleBooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Xunit;

namespace IS7024_GroupProject.Tests;

public sealed class IndexModelTests
{
    [Fact]
    public async Task OnGetAsync_WithQuery_ForwardsQueryAndExposesResults()
    {
        var service = new FakeGoogleBooksService
        {
            Result = BookSearchResult.Success(
            [
                new BookSearchItem
                {
                    Title = "The Hobbit",
                    Author = "J.R.R. Tolkien",
                    Isbn13 = "9780547928227",
                    CoverUrl = GoogleBooksService.PlaceholderCoverUrl,
                    GoogleBooksId = "hobbit"
                }
            ])
        };
        await using var dbContext = CreateContext();
        var model = new IndexModel(dbContext, service);
        model.Query = "The Hobbit";

        await model.OnGetAsync(CancellationToken.None);

        Assert.True(model.HasSearched);
        Assert.Equal("The Hobbit", service.LastQuery);
        Assert.Equal("The Hobbit", Assert.Single(model.SearchResult!.Books).Title);
    }

    [Fact]
    public async Task OnGetAsync_WithEmptyQuery_ShowsMessageWithoutCallingService()
    {
        var service = new FakeGoogleBooksService();
        await using var dbContext = CreateContext();
        var model = new IndexModel(dbContext, service);
        model.Query = "   ";

        await model.OnGetAsync(CancellationToken.None);

        Assert.True(model.HasSearched);
        Assert.Equal(BookSearchStatus.EmptyQuery, model.SearchResult!.Status);
        Assert.Equal(BookSearchResult.EmptyQueryMessage, model.SearchResult.Message);
        Assert.Null(service.LastQuery);
    }

    [Fact]
    public async Task OnPostAddToListAsync_WhenSignedOutChallengesAndDoesNotCreateEntries()
    {
        await using var dbContext = CreateContext();
        var model = CreateModel(dbContext, new ClaimsPrincipal(new ClaimsIdentity()));

        var result = await model.OnPostAddToListAsync(CancellationToken.None);

        Assert.IsType<ChallengeResult>(result);
        Assert.Empty(await dbContext.Books.ToListAsync());
        Assert.Empty(await dbContext.ReadingLists.ToListAsync());
    }

    [Fact]
    public async Task OnPostAddToListAsync_WhenInputIsInvalidReturnsBadRequest()
    {
        await using var dbContext = CreateContext();
        dbContext.Users.Add(CreateUser());
        await dbContext.SaveChangesAsync();

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "1")],
            authenticationType: "Test"));
        var model = CreateModel(dbContext, user);
        model.AddToListInput = new IndexModel.BookInput();

        var result = await model.OnPostAddToListAsync(CancellationToken.None);

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(await dbContext.Books.ToListAsync());
        Assert.Empty(await dbContext.ReadingLists.ToListAsync());
    }

    private static IndexModel CreateModel(ApplicationDbContext dbContext, ClaimsPrincipal user)
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

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"IndexModelTests-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static User CreateUser()
    {
        return new User
        {
            Id = 1,
            UserName = "reader@example.com",
            NormalizedUserName = "READER@EXAMPLE.COM",
            Email = "reader@example.com",
            NormalizedEmail = "READER@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString()
        };
    }

    private sealed class FakeGoogleBooksService : IGoogleBooksService
    {
        public string? LastQuery { get; private set; }

        public BookSearchResult Result { get; init; } = BookSearchResult.NoResults();

        public Task<BookSearchResult> SearchAsync(
            string? query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(Result);
        }
    }
}
