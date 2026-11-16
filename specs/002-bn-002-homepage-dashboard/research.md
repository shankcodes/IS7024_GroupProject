# Research: Homepage Dashboard

## Search and homepage flow

- **Decision**: Keep `Pages/Index.cshtml` as the home/search page and preserve `IndexModel.OnGetAsync`'s current query binding, empty-query handling, and delegation to `IGoogleBooksService.SearchAsync`. Add dashboard reads without changing the search service contract or result behavior.
- **Rationale**: The current home page is the existing Google Books search surface and its xUnit tests already cover forwarding a query and not calling the service for whitespace input.
- **Alternatives considered**: Moving search to another page or replacing it with a new search service was rejected because it risks regressions and duplicates existing functionality.

## Reader-specific dashboard data

- **Decision**: Resolve the signed-in reader using the established Identity user lookup pattern; load dashboard data only for that user's ID using asynchronous, read-only `ApplicationDbContext` queries and existing `ReadingList`/`ReadingGoal`/`Book` entities. Signed-out requests skip personal data queries. Do not introduce entities, tables, or dashboard services.
- **Rationale**: `ApplicationDbContext` already exposes `ReadingLists` and `ReadingGoals`; existing reading-list access filters by `UserId` and includes the associated book. The entities use `ReadingList.Status`, nullable `FinishedDate`, and `ReadingGoal.Year` / `TargetBooks`.
- **Alternatives considered**: Creating a new dashboard model/service, schema change, or placeholder/sample records was rejected as unnecessary duplication and inconsistent with the specification's no-invented-data requirement.

## Goal and completion calculation

- **Decision**: For the current calendar year, display the signed-in reader's `ReadingGoal` only if a matching row exists. Count only that user's entries with `Finished` status and a recorded `FinishedDate` within the year. Group recent finishes by the recorded month. Display a missing-goal state without a target and treat a zero goal as invalid under existing validation.
- **Rationale**: This follows the README's annual goal acceptance criteria and uses existing stored completion dates. Date-range filtering by start-of-year inclusive and start-of-next-year exclusive handles year boundaries.
- **Alternatives considered**: Assuming missing completion dates, carrying prior-year progress forward, or inventing a default goal were rejected.

## Read Next and Currently Reading

- **Decision**: Populate Currently Reading only from the user's saved `Reading` entries and Read Next only from their saved `WantToRead` entries. Display the existing available book fields, and use the established no-cover placeholder when necessary.
- **Rationale**: These are already represented by `ReadingStatus`, `ReadingList`, and `Book`; the README doesn't define an external recommendation source.
- **Alternatives considered**: Recommending unsaved Google Books results or adding recommendation logic was rejected because there is no data source and the feature must not fabricate user list data.

## Responsive theme and site navigation

- **Decision**: Style the existing page and shared layout with site CSS and existing Bootstrap assets, using a warm cream/sage palette, responsive fluid cards and search controls, clear focus indicators, and accessible navigation. Retain all current navigation links and add only links to existing features.
- **Rationale**: The constitution requires Razor Pages, shared responsive styling, and consistent navigation. Bootstrap is already loaded by `_Layout.cshtml`.
- **Alternatives considered**: Adding a CSS/UI dependency or a separate frontend was rejected because current assets are adequate and project dependencies should remain minimal.

## Test and validation approach

- **Decision**: Extend existing xUnit page-model tests using in-memory/fake dependencies as appropriate for search and signed-in page behavior; exercise real SQLite persistence only where it is already the repository's established pattern. Validate responsive behavior with the documented minimum viewport acceptance checks and run the test project plus build.
- **Rationale**: The repository has an existing xUnit test project with `IndexModelTests` and SQLite persistence coverage, so new tooling is unnecessary.
- **Alternatives considered**: Adding a test framework or relying only on manual inspection was rejected.
