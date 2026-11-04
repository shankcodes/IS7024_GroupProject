using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IS7024_GroupProject.Services.GoogleBooks;

// Talks to the Google Books API v1 and turns its JSON into BookSearchItem objects.
// Registered as a "typed HttpClient" in Program.cs, so the HttpClient we get here
// already has its BaseAddress (https://www.googleapis.com/books/v1/) and Timeout set.
public class GoogleBooksService : IGoogleBooksService
{
    // Shown when Google Books has no cover. BN-12 replaces it with an Open Library cover.
    public const string PlaceholderCoverUrl = "/images/no-cover.svg";

    private readonly HttpClient _httpClient;
    private readonly GoogleBooksOptions _options;
    private readonly ILogger<GoogleBooksService> _logger;

    public GoogleBooksService(
        HttpClient httpClient,
        IOptions<GoogleBooksOptions> options,
        ILogger<GoogleBooksService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<BookSearchResult> SearchAsync(string? query, CancellationToken cancellationToken = default)
    {
        // Rule: empty or whitespace search -> show a message and do NOT call the API.
        if (string.IsNullOrWhiteSpace(query))
        {
            return BookSearchResult.EmptyQuery();
        }

        string requestUrl = BuildRequestUrl(query.Trim());

        GoogleBooksResponse? response;
        try
        {
            using HttpResponseMessage httpResponse = await _httpClient.GetAsync(requestUrl, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                // e.g. 400 (bad key), 403 (quota used up), 429, 500, 503
                _logger.LogWarning("Google Books returned {StatusCode} for query '{Query}'.",
                    (int)httpResponse.StatusCode, query);
                return BookSearchResult.ServiceUnavailable();
            }

            response = await httpResponse.Content.ReadFromJsonAsync<GoogleBooksResponse>(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            // No internet, DNS failure, connection refused, ...
            _logger.LogWarning(ex, "Could not reach Google Books.");
            return BookSearchResult.ServiceUnavailable();
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient.Timeout was reached (the caller did not cancel).
            _logger.LogWarning(ex, "Google Books timed out.");
            return BookSearchResult.ServiceUnavailable();
        }
        catch (JsonException ex)
        {
            // Google Books sent something that is not the JSON we expect.
            _logger.LogWarning(ex, "Google Books returned JSON we could not read.");
            return BookSearchResult.ServiceUnavailable();
        }

        // "items" is missing when nothing matches.
        if (response?.Items == null || response.Items.Count == 0)
        {
            return BookSearchResult.NoResults();
        }

        var books = new List<BookSearchItem>();
        foreach (GoogleBooksVolume volume in response.Items)
        {
            BookSearchItem? book = MapToSearchItem(volume);
            if (book != null)
            {
                books.Add(book);
            }
        }

        return books.Count > 0 ? BookSearchResult.Success(books) : BookSearchResult.NoResults();
    }

    // Example result: volumes?q=Andy%20Weir&printType=books&maxResults=20&key=YOUR_KEY
    private string BuildRequestUrl(string query)
    {
        // A plain q (no intitle:/inauthor:) searches titles AND authors,
        // so one search box works for "The Hobbit" and for "Andy Weir".
        string url = $"volumes?q={Uri.EscapeDataString(query)}"
                   + "&printType=books"
                   + $"&maxResults={Math.Clamp(_options.MaxResults, 1, 40)}";

        // The key comes from user secrets / configuration, never from the code.
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            url += $"&key={Uri.EscapeDataString(_options.ApiKey)}";
        }

        return url;
    }

    // Turns one Google Books volume into a BookSearchItem. Returns null if the volume
    // has no ID, title, or ISBN: Book.GoogleBooksId is required, and BN-12 needs an ISBN to match Open Library.
    public static BookSearchItem? MapToSearchItem(GoogleBooksVolume volume)
    {
        GoogleBooksVolumeInfo? info = volume.VolumeInfo;
        if (string.IsNullOrWhiteSpace(volume.Id) || info == null || string.IsNullOrWhiteSpace(info.Title))
        {
            return null;
        }

        string? isbn = PickIsbn(info.IndustryIdentifiers);
        if (isbn == null)
        {
            return null;
        }

        return new BookSearchItem
        {
            Isbn13 = isbn,
            Title = info.Title.Trim(),
            Author = info.Authors is { Count: > 0 } ? string.Join(", ", info.Authors) : "Unknown author",
            Description = info.Description,
            CoverUrl = PickCoverUrl(info.ImageLinks),
            GoogleBooksId = volume.Id.Trim()
        };
    }

    // ISBN-13 first; if there is none, use the ISBN-10 (team rule).
    public static string? PickIsbn(List<GoogleBooksIndustryIdentifier>? identifiers)
    {
        if (identifiers == null)
        {
            return null;
        }

        string? isbn13 = identifiers.FirstOrDefault(i => i.Type == "ISBN_13")?.Identifier;
        if (!string.IsNullOrWhiteSpace(isbn13))
        {
            return isbn13.Trim();
        }

        string? isbn10 = identifiers.FirstOrDefault(i => i.Type == "ISBN_10")?.Identifier;
        if (!string.IsNullOrWhiteSpace(isbn10))
        {
            return isbn10.Trim();
        }

        return null;
    }

    // Prefer the bigger "thumbnail", fall back to "smallThumbnail", then to our placeholder.
    public static string PickCoverUrl(GoogleBooksImageLinks? imageLinks)
    {
        string? url = imageLinks?.Thumbnail ?? imageLinks?.SmallThumbnail;
        if (string.IsNullOrWhiteSpace(url))
        {
            return PlaceholderCoverUrl;
        }

        // Google Books often sends "http://books.google.com/..." links.
        // Browsers block or warn about http images on an https site, so switch to https.
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url.Substring("http://".Length);
        }

        return url;
    }
}
