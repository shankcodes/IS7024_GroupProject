using IS7024_GroupProject.Data;
using IS7024_GroupProject.Pages;
using IS7024_GroupProject.Services.GoogleBooks;
using Microsoft.EntityFrameworkCore;
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

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"IndexModelTests-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
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
