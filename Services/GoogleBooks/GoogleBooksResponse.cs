using System.Text.Json.Serialization;

namespace IS7024_GroupProject.Services.GoogleBooks;

// These classes match the JSON that Google Books returns from
// GET https://www.googleapis.com/books/v1/volumes?q=...
// We only list the fields BookNest uses; System.Text.Json ignores the rest.
// Every field is nullable because Google Books often leaves fields out.
// Docs: https://developers.google.com/books/docs/v1/reference/volumes

public class GoogleBooksResponse
{
    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    // Google Books leaves "items" out completely when nothing matches, so this can be null.
    [JsonPropertyName("items")]
    public List<GoogleBooksVolume>? Items { get; set; }
}

public class GoogleBooksVolume
{
    // The Google Books volume ID, e.g. "zyTCAlFPjgYC". Copied to BookSearchItem.GoogleBooksId (and later Book.GoogleBooksId).
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("volumeInfo")]
    public GoogleBooksVolumeInfo? VolumeInfo { get; set; }
}

public class GoogleBooksVolumeInfo
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("authors")]
    public List<string>? Authors { get; set; }

    // Can contain simple HTML tags such as <b>, <i> and <br>.
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    // Format varies: "2012", "2012-09", or "2012-09-18". Kept as a string.
    [JsonPropertyName("publishedDate")]
    public string? PublishedDate { get; set; }

    [JsonPropertyName("pageCount")]
    public int? PageCount { get; set; }

    [JsonPropertyName("industryIdentifiers")]
    public List<GoogleBooksIndustryIdentifier>? IndustryIdentifiers { get; set; }

    [JsonPropertyName("imageLinks")]
    public GoogleBooksImageLinks? ImageLinks { get; set; }
}

public class GoogleBooksIndustryIdentifier
{
    // "ISBN_13", "ISBN_10", "ISSN" or "OTHER".
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("identifier")]
    public string? Identifier { get; set; }
}

public class GoogleBooksImageLinks
{
    // About 80 px wide.
    [JsonPropertyName("smallThumbnail")]
    public string? SmallThumbnail { get; set; }

    // About 128 px wide.
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }
}
