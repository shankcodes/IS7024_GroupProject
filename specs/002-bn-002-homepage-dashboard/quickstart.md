# Quickstart: Homepage Dashboard Validation

## Prerequisites

- .NET 10 SDK.
- Repository dependencies restored.
- Local database configuration and migrations available as required by the existing application.
- For live search verification, a valid Google Books configuration; automated search tests use the repository's existing fake service and do not require an API key.

## Automated validation

From the repository root, run:

```powershell
dotnet test tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj
```

Verified: **43 tests passed, 0 failed, 0 skipped**. This includes search-query forwarding and feedback, user-scoped dashboard data, goal calculations and saves, signed-out data isolation, and generated cover-image fallback markup.

Build the web application:

```powershell
dotnet build IS7024_GroupProject.csproj
```

Verified: the web application builds successfully.

## Remaining browser verification

The automated test suite and build are complete. Real-browser checks for the rendered goal card and layout at 320px, tablet, and desktop widths have not been performed; complete tasks T034-T035 before considering visual browser validation finished.

## End-to-end scenarios

1. Start the existing web application using the repository's normal local configuration and open `/` while signed out. Confirm search works and no personal reading data is shown.
2. Search `The Hobbit`, `Andy Weir`, whitespace, and `xqzvplmnt`. Confirm existing results and messages, and verify whitespace does not invoke Google Books.
3. Sign in as a test reader with known Reading and WantToRead entries, one current-year goal, and dated Finished entries. Confirm only that reader's real entries are shown; the count and month grouping match stored data.
4. Use a reader with no goal, no Reading items, no WantToRead items, or no dated current-year completions. Confirm each section independently displays its empty state and no invented data.
5. Open Home at 320px, tablet, and desktop widths. Confirm no horizontal page scroll, clipped primary controls, or overlapping sections and verify keyboard focus and navigation.

See [data-model.md](data-model.md) for entity/count rules and [contracts/homepage.md](contracts/homepage.md) for the UI contract.
