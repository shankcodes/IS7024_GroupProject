# Tasks: Open Library Cover Fallback

**Feature**: BN-12 — Open Library covers by ISBN
**Spec**: [spec.md](spec.md)
**Plan**: [plan.md](plan.md)

## Execution Rules

- Complete tasks in order.
- Tests must be written and confirmed failing before T005 changes production code.
- Reuse `FakeHttpMessageHandler` and the existing `Fixtures` folder.
- Do not add setup or infrastructure tasks.
- Do not add services, `HttpClient` registrations, API keys, database changes, migrations, or new pages.

## Ordered Tasks

- [ ] T001 [AC1] Add a failing Google Books service test in `tests/IS7024_GroupProject.Tests/GoogleBooks/GoogleBooksServiceTests.cs` using a no-thumbnail Dune fixture/result with ISBN-13 `9780441172719`; assert that `CoverUrl` is `https://covers.openlibrary.org/b/isbn/9780441172719-M.jpg?default=false`.
- [ ] T002 [AC2] Add a failing Google Books service test in `tests/IS7024_GroupProject.Tests/GoogleBooks/GoogleBooksServiceTests.cs` for a no-thumbnail result with no ISBN; assert that `CoverUrl` is `GoogleBooksService.PlaceholderCoverUrl` (`/images/no-cover.svg`).
- [ ] T003 [AC3] Add a failing Google Books service test in `tests/IS7024_GroupProject.Tests/GoogleBooks/GoogleBooksServiceTests.cs` using the existing ISBN-10-only fixture; assert that the generated Open Library cover URL contains the ISBN-10 and uses the `-M.jpg?default=false` format.
- [ ] T004 [AC1] Add a failing regression test in `tests/IS7024_GroupProject.Tests/GoogleBooks/GoogleBooksServiceTests.cs` proving an existing Google Books thumbnail remains selected and add or update the Dune fixture test so a result with no `imageLinks` maps to the Open Library ISBN-13 URL.
- [ ] T005 [AC1][AC2][AC3] Update `Services/GoogleBooks/GoogleBooksService.cs` so `PickCoverUrl` accepts the selected ISBN, preserves Google Books thumbnail/small-thumbnail HTTPS priority, uses one `OpenLibraryCoverUrlFormat` constant for ISBN-13 or ISBN-10 fallback, and returns `PlaceholderCoverUrl` when no ISBN exists; update the `MapToSearchItem` call path and make all tests from T001–T004 pass.
- [ ] T006 [AC2][AC4] Update the existing search-result `<img>` in `Pages/Index.cshtml` with `onerror="this.onerror=null;this.src='/images/no-cover.svg'"`; if the BN-7 search-results page is not present in the branch, record this as a BN-7 follow-up in the task notes and do not create a page.
- [ ] T007 [AC1][AC2][AC3][AC4] Run `dotnet test tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj`, run `dotnet build`, and perform the Dune manual check from `quickstart.md`, confirming Google cover priority, ISBN-13 and ISBN-10 fallback URLs, placeholder behavior, browser `onerror` fallback, and continued Google Books result display.

## Dependencies and Execution Order

- T001, T002, T003, and T004 are test-first tasks and must complete before T005.
- T005 depends on T001–T004 and enables the cover-selection behavior.
- T006 depends on T005 for the final placeholder URL behavior in rendered results.
- T007 depends on all prior tasks.
- No setup or infrastructure phase is required.

## Acceptance-Criterion Coverage

| Acceptance criterion | Tasks |
|---|---|
| AC1 — Dune ISBN-13 uses the Open Library cover | T001, T004, T005, T007 |
| AC2 — No cover uses `/images/no-cover.svg` | T002, T005, T006, T007 |
| AC3 — ISBN-10 is used when ISBN-13 is absent | T003, T005, T007 |
| AC4 — Google Books results remain visible when Open Library is unavailable | T006, T007 |

## MVP Scope

T001–T005 deliver the core cover-selection behavior. T006 completes the browser fallback when the BN-7 results markup is available. T007 is required for final validation.
