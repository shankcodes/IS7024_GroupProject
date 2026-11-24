# Implementation Plan: BN-002 Homepage Dashboard

**Branch**: `002-bn-002-homepage-dashboard` | **Date**: 2026-11-16 | **Spec**: [spec.md](spec.md)

**Input**: Approved feature specification in `specs/002-bn-002-homepage-dashboard/spec.md`.

## Summary

Redesign the existing Razor Pages home/search surface as a warm cream and sage dashboard with responsive search, signed-in reader sections (Currently Reading, current-year Reading Goal, recent completions, and Read Next), signed-out prompts, and feature navigation. Preserve the existing Google Books search contract and use the existing `ReadingList`, `ReadingGoal`, `Book`, and `ApplicationDbContext` data. Filter all personal data to the authenticated user; do not add services, models, tables, Add to List functionality, or fabricated data. Preserve and link to Add to List only if it is already implemented; otherwise show navigation only to existing features and no inert Add to List button. All homepage book cards use `/images/no-cover.svg` when their cover URL is absent, invalid, or fails to load in the browser; the load-error fallback runs once without retrying a failed placeholder.

## Technical Context

**Language/Version**: C# / ASP.NET Core on .NET 10 (`net10.0`)

**Primary Dependencies**: Razor Pages, ASP.NET Core Identity, Entity Framework Core SQLite, Bootstrap already loaded by the shared layout, existing `IGoogleBooksService`.

**Storage**: Existing SQLite database through `ApplicationDbContext`; no schema changes.

**Testing**: Existing xUnit 2.9 test project (`tests/IS7024_GroupProject.Tests`) with Microsoft.NET.Test.Sdk and SQLite test coverage. Verify actual Google Books service outcomes without assuming genre or review-keyword matching; cover query forwarding, empty query, no results, service unavailable, and rendered `/images/no-cover.svg` fallback for null, empty, whitespace-only, malformed, or non-HTTP(S) cover URLs in search results and every homepage book-card section. Verify the browser load-error fallback runs once and does not loop when the placeholder itself fails; use existing test infrastructure without adding dependencies.

**Target Platform**: ASP.NET Core web application, responsive browser experience (minimum supported design width 320 CSS pixels).

**Project Type**: Single-project Razor Pages web application with a separate xUnit test project.

**Performance Goals**: Load the homepage without blocking I/O; issue bounded asynchronous reads for the current user's display data and avoid loading other users' entries or unnecessary related records.

**Constraints**: Preserve GET search semantics and all current status/messages/results; forward genre- and review-related keywords through existing search without promising specialized filtering or review-text search; use current identity ID for user-scoped data; maintain signed-out search; no new dependencies, models, services, tables, migrations, or Add to List functionality; retain existing navigation targets; preserve/link Add to List only if already implemented, otherwise show navigation only to existing features and no inert Add to List button; no fabricated goals or progress; use `/images/no-cover.svg` for missing or invalid covers on every homepage book card.

**Scale/Scope**: One homepage and shared navigation/theme styling; existing home page model tests extended for dashboard scenarios. No changes to unrelated features.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Code quality and maintainability**: PASS — extend the existing page and shared CSS, reuse current entity/service boundaries, and avoid unnecessary abstractions.
- **Testing standards**: PASS — extend deterministic xUnit tests for search preservation and dashboard user/data/empty-state behavior.
- **Consistent user experience**: PASS — retain shared navigation and feedback, use accessible labels and focus behavior, and support mobile and desktop.
- **Performance requirements**: PASS — use asynchronous, read-only, user-scoped data queries and avoid cross-user or unnecessary data loads.
- **Technology/security constraints**: PASS — remain on .NET 10 Razor Pages and keep personal information scoped to the authenticated user.
- **Development workflow**: PASS — implementation validation requires a clean build and passing relevant tests.

**Post-design re-check**: PASS — `research.md`, `data-model.md`, `contracts/homepage.md`, and `quickstart.md` follow the same boundaries; no new dependencies, models, tables, migrations, or constitution exceptions are required.

## Project Structure

### Documentation (this feature)

```text
specs/002-bn-002-homepage-dashboard/
├── plan.md
├── research.md
├── data-model.md
├── contracts/
│   └── homepage.md
├── quickstart.md
└── tasks.md              # Executable implementation checklist
```

### Source Code (repository root)

```text
Pages/
├── Index.cshtml          # Homepage/dashboard and existing search presentation
├── Index.cshtml.cs       # Existing search orchestration and user-scoped dashboard reads
└── Shared/
    └── _Layout.cshtml    # Existing shared navigation and site shell

wwwroot/css/
└── site.css              # Existing global styles and responsive cream/sage dashboard theme

Models/                   # Existing Book, ReadingList, ReadingGoal, ReadingStatus, User
Data/
└── ApplicationDbContext.cs
Services/
├── ReadingListStore.cs   # Existing reading-list behavior to preserve/reuse where suitable
└── GoogleBooks/          # Existing search contract and implementation; unchanged

tests/IS7024_GroupProject.Tests/
├── GoogleBooks/
│   └── GoogleBooksServiceTests.cs # Existing cover fallback and service behavior tests
└── Pages/
    └── IndexModelTests.cs # Existing search tests extended with dashboard behavior

**Structure Decision**: Keep the single ASP.NET Core web project and its existing Razor Pages, shared layout, CSS, EF Core context, and xUnit project. Implement only within the named home/layout/style surface and directly related page tests; leave the Google Books service and database schema unchanged.

## Implementation Boundaries

- Resolve the current user with the existing Identity pattern. Skip personal queries when no authenticated user is available.
- Query `ReadingList` and `ReadingGoal` through `ApplicationDbContext` (or its existing reading-list store where appropriate) using `AsNoTracking`, cancellation-aware async calls, and `UserId` filters.
- Treat the existing entity names as authoritative: `ReadingGoal.TargetBooks` and nullable `ReadingList.FinishedDate` (`DateTime?`). Count only Finished entries with recorded dates in the current calendar year; group those by actual month.
- For a configured goal, derive the displayed whole-number progress percentage from actual finished count divided by saved target, rounded to the nearest integer with halfway values rounded up and capped at 100%; render an accessible progress bar and keep the actual count visible. When no goal exists, show the current year and goal-setting path without a fabricated target or progress.
- Render empty and signed-out states independently for each personal section. Search remains the existing GET flow and continues to work for signed-out visitors; keyword searches use the existing service and do not imply specialized genre or review-text filtering. Preserve/link Add to List only when that functionality already exists; otherwise expose only existing-feature navigation and no inert Add to List button.
- Treat a cover URL as valid only when it is a well-formed absolute URL using HTTP or HTTPS. For null, empty, whitespace-only, malformed, or unsupported-scheme URLs, render `/images/no-cover.svg`. Apply this rule to search results, Currently Reading, recent finishes, Read Next, and every other homepage book card.
- For a browser image-load error, replace the source with `/images/no-cover.svg` after disabling/removing that image's error handler; do not retry either the failed image or a failed placeholder.
- Use the existing Bootstrap bundle and shared `site.css`; do not add a CSS framework, recommendation service, data entity, or migration.

## Complexity Tracking

No constitution violations or additional architectural complexity are proposed.
