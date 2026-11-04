using System.Net;
using System.Runtime.CompilerServices;
using IS7024_GroupProject.Services.GoogleBooks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace IS7024_GroupProject.Tests;

// Unit tests for BN-6. No real network calls: FakeHttpMessageHandler returns
// canned JSON from the Fixtures folder.
public class GoogleBooksServiceTests
{
    // ---------- helpers ----------

    // Reads tests/IS7024_GroupProject.Tests/Fixtures/<fileName>. The path is worked out from
    // this source file's location, so the test project needs no extra "copy to output" setting.
    private static string ReadFixture(string fileName, [CallerFilePath] string thisFile = "") =>
        File.ReadAllText(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", "Fixtures", fileName));

    private static GoogleBooksService CreateService(FakeHttpMessageHandler handler, string? apiKey = null)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://www.googleapis.com/books/v1/")
        };
        var options = Options.Create(new GoogleBooksOptions { ApiKey = apiKey });
        return new GoogleBooksService(httpClient, options, NullLogger<GoogleBooksService>.Instance);
    }

    // ---------- acceptance criteria ----------

    // AC1: Search "The Hobbit" -> results include "The Hobbit" by J.R.R. Tolkien.
    [Fact]
    public async Task SearchAsync_TheHobbit_ResultsIncludeTheHobbitByTolkien()
    {
        var handler = new FakeHttpMessageHandler(ReadFixture("the-hobbit.json"));
        var service = CreateService(handler);

        BookSearchResult result = await service.SearchAsync("The Hobbit");

        Assert.Equal(BookSearchStatus.Success, result.Status);
        Assert.Contains(result.Books, b => b.Title == "The Hobbit" && b.Author == "J.R.R. Tolkien");
    }

    // AC2: Search "Andy Weir" -> results include The Martian and Project Hail Mary.
    [Fact]
    public async Task SearchAsync_AndyWeir_ResultsIncludeBothBooks()
    {
        var handler = new FakeHttpMessageHandler(ReadFixture("andy-weir.json"));
        var service = CreateService(handler);

        BookSearchResult result = await service.SearchAsync("Andy Weir");

        Assert.Equal(BookSearchStatus.Success, result.Status);
        Assert.Contains(result.Books, b => b.Title == "The Martian" && b.Author == "Andy Weir");
        Assert.Contains(result.Books, b => b.Title == "Project Hail Mary" && b.Author == "Andy Weir");
    }

    // AC3: ISBN-13 in industryIdentifiers -> Book.Isbn13 is 9780547928227.
    [Fact]
    public async Task SearchAsync_HobbitWithIsbn13_SetsIsbn13()
    {
        var handler = new FakeHttpMessageHandler(ReadFixture("the-hobbit.json"));
        var service = CreateService(handler);

        BookSearchResult result = await service.SearchAsync("The Hobbit");

        BookSearchItem hobbit = Assert.Single(result.Books);
        Assert.Equal("9780547928227", hobbit.Isbn13);
        Assert.Equal("TEST_HOBBIT_ID", hobbit.GoogleBooksId);
    }

    // AC4: No cover image -> placeholder cover (until Open Library provides one in BN-12).
    [Fact]
    public async Task SearchAsync_ResultWithoutCover_UsesPlaceholderCover()
    {
        var handler = new FakeHttpMessageHandler(ReadFixture("no-cover.json"));
        var service = CreateService(handler);

        BookSearchResult result = await service.SearchAsync("Dune");

        BookSearchItem dune = Assert.Single(result.Books);
        Assert.Equal(GoogleBooksService.PlaceholderCoverUrl, dune.CoverUrl);
        Assert.Equal("9780441172719", dune.Isbn13);
    }

    // ---------- related rules ----------

    // Empty or whitespace search -> message and NO API call.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchAsync_EmptyQuery_ReturnsMessageAndMakesNoApiCall(string query)
    {
        var handler = new FakeHttpMessageHandler(ReadFixture("the-hobbit.json"));
        var service = CreateService(handler);

        BookSearchResult result = await service.SearchAsync(query);

        Assert.Equal(BookSearchStatus.EmptyQuery, result.Status);
        Assert.Equal("Please enter a title or author.", result.Message);
        Assert.Empty(result.Books);
        Assert.Empty(handler.Requests); // proves no HTTP call was made
    }

    // Razor Pages turns an empty text box into null, so null must behave the same way.
    [Fact]
    public async Task SearchAsync_NullQuery_ReturnsMessageAndMakesNoApiCall()
    {
        var handler = new FakeHttpMessageHandler(ReadFixture("the-hobbit.json"));
        var service = CreateService(handler);

        BookSearchResult result = await service.SearchAsync(null);

        Assert.Equal(BookSearchStatus.EmptyQuery, result.Status);
        Assert.Equal("Please enter a title or author.", result.Message);
        Assert.Empty(handler.Requests);
    }

    // No matches (Google Books leaves out "items") -> "No books found."
    [Fact]
    public async Task SearchAsync_NoMatches_ReturnsNoBooksFound()
    {
        var handler = new FakeHttpMessageHandler(ReadFixture("no-results.json"));
        var service = CreateService(handler);

        BookSearchResult result = await service.SearchAsync("xqzvplmnt");

        Assert.Equal(BookSearchStatus.NoResults, result.Status);
        Assert.Equal("No books found.", result.Message);
        Assert.Empty(result.Books);
    }

    // Google Books error status -> ServiceUnavailable, no crash.
    [Fact]
    public async Task SearchAsync_GoogleBooksReturnsServerError_ReturnsServiceUnavailable()
    {
        var handler = new FakeHttpMessageHandler("{}", HttpStatusCode.InternalServerError);
        var service = CreateService(handler);

        BookSearchResult result = await service.SearchAsync("The Hobbit");

        Assert.Equal(BookSearchStatus.ServiceUnavailable, result.Status);
        Assert.Empty(result.Books);
    }

    // Google Books timeout -> ServiceUnavailable, no crash.
    [Fact]
    public async Task SearchAsync_GoogleBooksTimesOut_ReturnsServiceUnavailable()
    {
        // HttpClient throws TaskCanceledException when its Timeout is reached.
        var handler = new FakeHttpMessageHandler(new TaskCanceledException("Timed out"));
        var service = CreateService(handler);

        BookSearchResult result = await service.SearchAsync("The Hobbit");

        Assert.Equal(BookSearchStatus.ServiceUnavailable, result.Status);
    }

    // No ISBN-13 -> use the ISBN-10.
    [Fact]
    public async Task SearchAsync_OnlyIsbn10_UsesIsbn10()
    {
        var handler = new FakeHttpMessageHandler(ReadFixture("isbn10-only.json"));
        var service = CreateService(handler);

        BookSearchResult result = await service.SearchAsync("The Hobbit");

        BookSearchItem book = Assert.Single(result.Books);
        Assert.Equal("054792822X", book.Isbn13);
    }

    // Google Books sends http:// thumbnails -> we switch them to https://.
    [Fact]
    public async Task SearchAsync_HttpThumbnail_IsChangedToHttps()
    {
        var handler = new FakeHttpMessageHandler(ReadFixture("the-hobbit.json"));
        var service = CreateService(handler);

        BookSearchResult result = await service.SearchAsync("The Hobbit");

        BookSearchItem hobbit = Assert.Single(result.Books);
        Assert.StartsWith("https://books.google.com/", hobbit.CoverUrl);
    }

    // The request URL contains the encoded search text, and the API key when one is configured.
    [Fact]
    public async Task SearchAsync_BuildsRequestUrlWithQueryAndApiKey()
    {
        var handler = new FakeHttpMessageHandler(ReadFixture("andy-weir.json"));
        var service = CreateService(handler, apiKey: "test-key");

        await service.SearchAsync("Andy Weir");

        HttpRequestMessage request = Assert.Single(handler.Requests);
        string url = request.RequestUri!.AbsoluteUri; // AbsoluteUri keeps %20 (ToString() would not)
        Assert.StartsWith("https://www.googleapis.com/books/v1/volumes?q=Andy%20Weir", url);
        Assert.Contains("printType=books", url);
        Assert.Contains("key=test-key", url);
    }
}
