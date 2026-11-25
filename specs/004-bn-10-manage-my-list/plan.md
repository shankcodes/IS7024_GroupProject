# Implementation Plan: Manage My List

**Branch**: `BN-10-Build-My-List-page-with-status-dropdown-and-Remove-koyyadps` | **Date**: 2026-11-24 | **Spec**: [spec.md](spec.md)

**Input**: Approved BN-10 feature specification for status changes, removal, empty-state guidance, ownership protection, and preservation of Add to List.

## Summary

Extend the existing authenticated My Reading List Razor Page so a reader can update a personal entry to Want to Read, Reading, or Finished, remove the entry, and receive the required empty-state guidance. Reuse the existing `ReadingList`, `ReadingStatus`, `Book`, `ReadingListStore`, and `ApplicationDbContext`; enforce ownership in store queries and preserve the existing Add to List form and feedback. No new models, services, tables, or migrations are required.

## Technical Context

- **Language/Version**: C# on .NET 10 (`net10.0`), nullable enabled
- **Primary Dependencies**: ASP.NET Core Razor Pages, ASP.NET Core Identity, EF Core 10.0.14, Bootstrap and existing BookNest CSS
- **Storage**: Existing EF Core `ApplicationDbContext` with SQL Server application persistence; isolated SQLite contexts for tests
- **Testing**: xUnit, EF Core SQLite, existing PageModel test patterns; `dotnet test tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj`
- **Target Platform**: Server-rendered ASP.NET Core Razor Pages web application
- **Project Type**: Web application
- **Performance Goals**: Keep list reads and mutations asynchronous; retain the existing single list query and title ordering without introducing extra per-entry database queries
- **Constraints**: Require authentication, scope every mutation by authenticated user ID, validate status values and entry IDs server-side, preserve antiforgery and existing Add to List behavior, and avoid schema changes
- **Scale/Scope**: One existing My List page, one existing store, three supported statuses, remove action, empty state, and focused regression tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / constraint | Gate | Evidence and notes |
|---|---|---|
| Code quality and maintainability | Pass | Extends the existing Razor Page and `ReadingListStore`; no duplicate model, service, or persistence abstraction. |
| Testing standards | Pass | Plan includes deterministic SQLite persistence tests and PageModel tests for all acceptance scenarios, invalid input, ownership, authentication, and Add to List regression. |
| Consistent user experience | Pass | Reuses the existing BookNest layout, Bootstrap controls, cover fallback, status feedback, validation patterns, and responsive card styling. |
| Performance requirements | Pass | Store operations remain asynchronous and ownership predicates execute in the database; no unbounded or per-entry I/O is planned. |
| Technology and security constraints | Pass | Targets .NET 10 Razor Pages, keeps `[Authorize]`, validates posted values server-side, and scopes all mutations to the authenticated user. |

## Project Structure

### Documentation (this feature)

```text
specs/004-bn-10-manage-my-list/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── reading-list.md
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by this plan)
```

### Source Code (repository root)

```text
Pages/
├── ReadingList.cshtml          # Add status dropdown, finished date, remove action, empty state
└── ReadingList.cshtml.cs       # Add authenticated POST handlers and validation/feedback
Services/
└── ReadingListStore.cs         # Add ownership-scoped status and removal operations
Models/
├── ReadingList.cs              # Reuse existing status and finished-date fields
├── ReadingStatus.cs            # Reuse existing three-state enum
└── Book.cs                      # Reuse existing shared catalog display data
Data/
└── ApplicationDbContext.cs      # Reuse existing mapping; no schema change planned
tests/IS7024_GroupProject.Tests/
├── PersistenceTests.cs          # Add store transition, ownership, removal, and Add regression tests
├── Pages/ReadingListModelTests.cs # Add PageModel, auth, validation, feedback, and data-scope tests
└── Pages/ReadingListPageTests.cs  # Add page-contract tests that do not require a Razor rendering host
```

**Structure Decision**: Extend the existing single ASP.NET Core Razor Pages project, store, and test project. The existing `ReadingList` model and EF Core mapping are sufficient; no migration or new persistence artifact is planned. The test project has no existing Razor rendering host (`WebApplicationFactory`, `TestServer`, or `IRazorViewEngine`), so automated tests cover persistence and PageModel behavior while `quickstart.md` provides the rendered-markup browser checks. `ReadingListPageTests.cs` is reserved for page-contract assertions that use existing dependencies and do not claim to render Razor output. The user-facing contract is documented in [contracts/reading-list.md](contracts/reading-list.md).

## Constitution Check — Post-Design

All gates remain **Pass** after design. The design keeps the existing Razor Pages and EF Core boundaries, uses asynchronous ownership-scoped persistence operations, validates all posted status and entry identifiers on the server, preserves authentication and antiforgery behavior, and adds deterministic automated tests without introducing dependencies, models, tables, or migrations.

## Complexity Tracking

No constitution violations or complexity exceptions are required.
