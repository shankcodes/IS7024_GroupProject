namespace IS7024_GroupProject.Services.GoogleBooks;

// Settings read from the "GoogleBooks" section of configuration
// (appsettings.json + user secrets). Never put the real API key in appsettings.json.
public class GoogleBooksOptions
{
    public const string SectionName = "GoogleBooks";

    // Your Google Cloud API key. Comes from user secrets in development.
    public string? ApiKey { get; set; }

    // How many results to ask for. Google Books allows 0 to 40 (default 10).
    public int MaxResults { get; set; } = 20;
}
