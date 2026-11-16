# Quickstart: BN-12 Open Library Cover Fallback

## Prerequisites

- .NET 10 SDK
- Visual Studio 2026 or the .NET CLI
- A configured Google Books API key in user secrets under `GoogleBooks:ApiKey`
- Network access to Google Books and the Open Library covers host

## Run the application

From the repository root:

```powershell
dotnet run
```

Open the HTTPS URL printed by the application.

## Manual scenario: Dune ISBN-13 fallback

1. Open the Home page search form.
2. Search for `Dune`.
3. Find the Dune result by Frank Herbert with ISBN `9780441172719`.
4. Confirm the result's image uses the Open Library ISBN cover URL when Google Books does not provide a thumbnail:
   `https://covers.openlibrary.org/b/isbn/9780441172719-M.jpg?default=false`
5. Confirm the image is shown without a server-side Open Library lookup.
6. To verify the browser fallback, simulate or observe a failed cover request and confirm the image source changes to `/images/no-cover.svg`.

## Additional manual checks

- Search for a result with an existing Google Books thumbnail and confirm that Google Books remains the displayed cover source.
- Search for a result with only an ISBN-10 and confirm the generated Open Library cover URL contains that ISBN-10.
- Search for a result without either ISBN and confirm the placeholder is used immediately.
- Block or make the Open Library cover request fail and confirm the Google Books result remains visible while the image falls back to `/images/no-cover.svg`.

## Automated validation after implementation

```powershell
dotnet test tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj
dotnet build
```

Expected results:

- Existing Google Books search tests continue to pass.
- Tests cover Google thumbnail priority, ISBN-13 fallback, ISBN-10 fallback, missing ISBN placeholder behavior, and preservation of the existing result data.
- The solution builds successfully.
