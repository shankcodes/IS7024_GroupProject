# Implementation Plan: Open Library Cover Fallback

**Branch**: `BN-12-Combine-Open-Library-with-Google-Books-by-ISBN-13-sureshs8` | **Date**: 2026-10-27 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/012-bn-12-open-library-covers/spec.md`

## Summary

Update the existing Google Books cover-selection logic so Google Books remains
the primary source, Open Library's ISBN cover URL is used only when Google
Books has no cover, and the existing placeholder remains the final fallback.
The browser handles failed Open Library image requests; the server makes no
Open Library call.

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: Existing ASP.NET Core Razor Pages and Google Books service

**Storage**: N/A; no persistence changes

**Testing**: Existing xUnit test project at `tests/IS7024_GroupProject.Tests`

**Target Platform**: ASP.NET Core Razor Pages running locally and in Azure

**Project Type**: Web application

**Performance Goals**: Do not add a server-side Open Library request or serial dependency to search responses

**Constraints**: No new NuGet packages, service, `HttpClient`, API key, database change, migration, or `Program.cs` change. Implementation is limited to `Services/GoogleBooks/GoogleBooksService.cs`, with one approved markup exception for the existing search-result image in `Pages/Index.cshtml`.

**Scale/Scope**: Cover selection for existing search results only; Open Library search fallback is out of scope.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Code quality: PASS — the change is limited to one existing method and one constant.
- Testing: PASS — add or update focused xUnit coverage for cover priority and ISBN fallback without adding packages.
- User experience: PASS — retain the existing placeholder and add only the specified `onerror` fallback to the existing search-result image.
- Performance: PASS — no server-side Open Library call; browser image requests remain independent.
- Security and technology constraints: PASS — no API key, persistence, migration, or new dependency.

## Project Structure

### Documentation (this feature)

```text
specs/012-bn-12-open-library-covers/
├── spec.md              # Feature specification
├── plan.md              # This implementation plan
├── research.md          # Cover URL and fallback decisions
├── quickstart.md        # Manual and automated validation guide
└── checklists/          # Specification quality checklist
```

### Source Code (repository root)
```text
Services/GoogleBooks/GoogleBooksService.cs
tests/IS7024_GroupProject.Tests/GoogleBooks/GoogleBooksServiceTests.cs
Pages/Index.cshtml                         # BN-7 follow-up only, if search markup is present
```

**Structure Decision**: Keep the existing single ASP.NET Core Razor Pages
project. Implementation changes are limited to `Services/GoogleBooks/GoogleBooksService.cs`
plus one approved exception: add the specified `onerror` attribute to the
existing search-result `<img>` in `Pages/Index.cshtml`, with no other markup
changes. Do not create a new page. If the BN-7 search-result markup is not
present, record that image change as a BN-7 follow-up instead.

## Complexity Tracking

No violations. The plan intentionally avoids new projects, services, clients,
packages, endpoints, persistence, and startup changes.

## Implementation Sequence

1. Update `PickCoverUrl` to accept the selected ISBN in addition to
   `imageLinks`.
2. Add the single `OpenLibraryCoverUrlFormat` constant and construct the
   `?default=false` ISBN cover URL only after Google Books cover candidates are
   exhausted.
3. Preserve `/images/no-cover.svg` when no ISBN is available.
4. Update the existing BN-6 Dune `SearchAsync` test to assert the Open Library
   URL; this is an intentional behavior change from the old placeholder result.
5. Add the no-server-call AC4 test and confirm only the Google Books handler
   records a request.
6. If BN-7 search-result markup is available, add the specified browser
   `onerror` fallback to its existing `<img>` element; otherwise leave a
   follow-up note and do not create or modify a page.
7. Check the browser fallback manually using `quickstart.md`; a browser or
   `WebApplicationFactory` test is out of scope for this one-point story.
8. Run the existing xUnit tests and build the solution.

## Explicitly Excluded

- Server-side Open Library calls or new Open Library service code.
- Open Library search fallback when Google Books is unavailable.
- New NuGet packages, `HttpClient` registrations, API keys, database tables,
  migrations, or `Program.cs` changes.
- New pages or data models.
