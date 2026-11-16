# Tasks: Open Library Cover Fallback

**Feature**: BN-12 — Open Library covers by ISBN
**Spec**: [spec.md](spec.md)
**Plan**: [plan.md](plan.md)

## Execution Rules

- Complete tasks in order.
- Tests must be written and confirmed failing before T006 changes production code.
- Reuse `FakeHttpMessageHandler` and the existing `Fixtures` folder.
- Do not add setup or infrastructure tasks.
- Do not add services, `HttpClient` registrations, API keys, database changes, migrations, or new pages.

## Ordered Tasks

- [x] T001 [AC1] Add a failing Google Books service test in `tests/IS7024_GroupProject.Tests/GoogleBooks/GoogleBooksServiceTests.cs` using a no-thumbnail Dune fixture/result with ISBN-13 `9780441172719`; assert that `CoverUrl` is `https://covers.openlibrary.org/b/isbn/9780441172719-M.jpg?default=false`.
- [x] T002 [AC2] Add a failing direct Google Books service test in `tests/IS7024_GroupProject.Tests/GoogleBooks/GoogleBooksServiceTests.cs` that calls `GoogleBooksService.PickCoverUrl(imageLinks: null, isbn: null)` and asserts it returns `GoogleBooksService.PlaceholderCoverUrl` (`/images/no-cover.svg`) as the defensive fallback. Do not build a search result with no ISBN because `MapToSearchItem` still excludes those results as existing BN-6 behavior; AC2's normal case (a book with an ISBN but no cover in either source) is covered by the `onerror` fallback from T007 and the manual check in T008.
- [x] T003 [AC3] Add a failing Google Books service test in `tests/IS7024_GroupProject.Tests/GoogleBooks/GoogleBooksServiceTests.cs` using the existing ISBN-10-only fixture; assert that the generated Open Library cover URL contains the ISBN-10 and uses the `-M.jpg?default=false` format.
- [x] T004 [AC1] Update the existing test `SearchAsync_ResultWithoutCover_UsesPlaceholderCover` in `tests/IS7024_GroupProject.Tests/GoogleBooks/GoogleBooksServiceTests.cs` to `SearchAsync_ResultWithoutCover_UsesOpenLibraryCover` and assert `https://covers.openlibrary.org/b/isbn/9780441172719-M.jpg?default=false`; this is the end-to-end AC1 test for the Dune fixture with no `imageLinks`.
- [x] T005 [AC4] Add a failing `SearchAsync("Dune")` test in `tests/IS7024_GroupProject.Tests/GoogleBooks/GoogleBooksServiceTests.cs` that asserts the result is `Success`, contains Dune, and records only a `googleapis.com` request in `FakeHttpMessageHandler`, with no `openlibrary.org` request.
- [x] T006 [AC1][AC2][AC3] Update `Services/GoogleBooks/GoogleBooksService.cs` so `PickCoverUrl` accepts the selected ISBN, preserves Google Books thumbnail/small-thumbnail HTTPS priority, uses one `OpenLibraryCoverUrlFormat` constant for ISBN-13 or ISBN-10 fallback, and returns `PlaceholderCoverUrl` when called without an ISBN; preserve the existing no-ISBN `MapToSearchItem` exclusion and make tests T001–T005 pass.
- [x] T007 [AC2][AC4] Add `onerror="this.onerror=null;this.src='/images/no-cover.svg'"` to the existing search-result `<img>` in `Pages/Index.cshtml`, with no other markup changes; if BN-7 search-result markup is not present, record this as a BN-7 follow-up and do not create a page.
- [x] T008 [AC1][AC2][AC3][AC4] Run `dotnet test tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj`, run `dotnet build`, and perform the Dune manual check from `quickstart.md`, including the browser fallback; browser automation or `WebApplicationFactory` coverage is intentionally out of scope for this one-point story.

## Dependencies and Execution Order

- T001, T002, T003, T004, and T005 are test-first tasks and must complete before T006.
- T006 depends on T001–T005 and enables the cover-selection behavior.
- T007 depends on T006 for the final placeholder URL behavior in rendered results.
- T008 depends on all prior tasks.
- No setup or infrastructure phase is required.

## Acceptance-Criterion Coverage

| Acceptance criterion | Tasks |
|---|---|
| AC1 — Dune ISBN-13 uses the Open Library cover | T001, T004, T006, T008 |
| AC2 — An ISBN-bearing result with no cover uses `/images/no-cover.svg` | T002, T006, T007, T008 |
| AC3 — ISBN-10 is used when ISBN-13 is absent | T003, T006, T008 |
| AC4 — Google Books results remain visible when Open Library is unavailable | T005, T007, T008 |

## MVP Scope

T001–T006 deliver the core cover-selection behavior. T007 completes the browser fallback when the BN-7 results markup is available. T008 is required for final validation.
