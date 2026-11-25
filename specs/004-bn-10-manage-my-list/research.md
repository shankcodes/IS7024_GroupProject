# Research: Manage My List

## Decision: Extend the existing ReadingListStore for status and removal operations

- **Rationale**: `ReadingListStore` already owns My List persistence, loads entries by `UserId`, and creates the existing Add to List records. Adding ownership-scoped status and removal operations keeps list behavior in one service and avoids duplicate services or direct persistence logic in the page.
- **Alternatives considered**: Adding a second service was rejected because it would duplicate the existing list boundary. Performing updates directly in `ReadingListModel` was rejected because it would bypass the current store abstraction and make ownership rules harder to reuse and test.

## Decision: Scope every mutation by both entry identity and authenticated user identity

- **Rationale**: A posted entry identifier is user-controlled. Update and remove queries must require the current user's identifier as well as the reading-list entry key, so a reader cannot affect another reader's entry. The existing `[Authorize]` page protection remains the first access boundary; the store remains the data-level boundary.
- **Alternatives considered**: Loading by entry ID and checking ownership afterward was rejected because an ownership predicate is safer and avoids exposing another reader's record to the page model.

## Decision: Apply finished-date transitions in the store mutation operation

- **Rationale**: The status transition rule is domain behavior: Finished sets the current date, while Want to Read and Reading clear the date. Applying both values together before saving prevents stale FinishedDate values and gives persistence tests a single behavior boundary.
- **Alternatives considered**: Letting the Razor Page set `FinishedDate` was rejected because it would duplicate business rules and make non-page callers easier to get wrong. A database default or trigger is unnecessary because the existing schema already has the required nullable column.

## Decision: Use existing server-rendered Razor Page forms and validation

- **Rationale**: The feature is an internal Razor Pages workflow with no separate public API. Named POST handlers for status and remove actions fit the existing page and preserve antiforgery protection, validation summaries, redirects, and TempData feedback.
- **Alternatives considered**: Client-only updates were rejected because persistence and authorization must be server-side. A new endpoint layer was rejected because no external interface is required.

## Decision: Reuse existing BookNest layout and CSS patterns

- **Rationale**: The page already uses Bootstrap and the shared BookNest layout; existing `.book-grid`, `.book-card`, `.empty-state`, button, and focus styles provide responsive and accessible presentation without introducing a feature-specific visual system.
- **Alternatives considered**: A new page-specific stylesheet was rejected unless implementation reveals a styling gap that cannot be handled by the existing classes.

## Decision: Use isolated SQLite-backed tests plus PageModel tests

- **Rationale**: The current test project already uses xUnit and isolated SQLite connections against `ApplicationDbContext`, which exercises EF Core relationships and ownership predicates without requiring a configured SQL Server instance. PageModel tests can verify authentication results, validation, redirects, feedback, and per-user behavior.
- **Alternatives considered**: Production database tests were rejected because they would be environment-dependent and non-deterministic. New test packages are unnecessary.

## Decision: Separate automated behavior tests from rendered-markup verification

- **Finding**: The existing test project contains xUnit, EF Core SQLite/InMemory, and the Microsoft test SDK, but no `WebApplicationFactory`, `TestServer`, `IRazorViewEngine`, or other Razor rendering infrastructure. No existing test file renders a Razor Page.
- **Rationale**: Use `ReadingListModelTests.cs` and `PersistenceTests.cs` for automated PageModel, authentication, ownership, status, FinishedDate, removal, and Add to List behavior. Use `ReadingListPageTests.cs` only for page-contract assertions that can be performed with existing dependencies and do not claim to render markup. Verify the actual rendered dropdown, Remove form, exact empty-state text, and FinishedDate display through the browser checks in `quickstart.md`.
- **Alternatives considered**: Adding `Microsoft.AspNetCore.Mvc.Testing` or another rendering package was not selected because the request prohibits introducing NuGet packages without reporting the need first; no package is required for the planned automated persistence and PageModel coverage.

## Decision: Do not add a migration or persistent entity

- **Rationale**: `ReadingList.Status` is already mapped as a string and `ReadingList.FinishedDate` already exists in the current model and migration. The feature only changes values and deletes existing reading-list rows.
- **Alternatives considered**: Adding a separate status-history table was rejected because history is outside the approved scope and would violate the no-new-table requirement.

## Verification checkpoint: Existing persistence artifacts are reusable

- `ReadingList` already contains `Status` and nullable `FinishedDate`.
- `ReadingStatus` already contains `WantToRead`, `Reading`, and `Finished`.
- `Book` remains the shared catalog entity and must survive list-entry removal.
- `ReadingListStore` already owns user-scoped list reads and Add to List behavior.
- `ApplicationDbContext` already exposes and maps `Books` and `ReadingLists`; no new table or migration is needed.
