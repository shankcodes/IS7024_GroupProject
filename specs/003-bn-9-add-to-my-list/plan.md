# Implementation Plan: Add a Book to My List

**Branch**: `BN-9-Add-book-to-list-tummalga` | **Date**: 2026-11-24 | **Spec**: [spec.md](spec.md)

**Input**: Completed BN-9 implementation and its acceptance scenarios.

## Summary

BN-9 lets an authenticated reader save a Google Books search result to My List as Want to Read, prevents a duplicate entry, preserves existing shared book metadata, and shows saved book details. The implementation uses the existing Razor Pages, EF Core, Identity, `Book`, `ReadingList`, and `ReadingListStore` patterns.

**Approved scope deviation**: The handout's example plan assumes SQLite for the application database. The project owner separately requested moving application persistence to Azure SQL, so this branch also switches the application provider and adds a SQL Server initial migration. This is documented as an explicit branch-level infrastructure change, not as a requirement of the BN-9 reader workflow. Existing SQLite database contents are not transferred by the schema migration.

## Technical Context

- **Language/Version**: C# on .NET 10 (`net10.0`), nullable enabled
- **Primary Dependencies**: ASP.NET Core Razor Pages, ASP.NET Core Identity, EF Core SQL Server 10.0.14; Google Books service already used for search
- **Storage**: Azure SQL for the application; SQLite and EF Core InMemory for automated tests
- **Testing**: xUnit; `dotnet test tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj`
- **Target Platform**: ASP.NET Core web application; local development and Azure-hosted database
- **Project Type**: Server-rendered web application
- **Performance Goals**: No new latency target; use asynchronous database and search operations and bounded database retry behavior
- **Constraints**: Configure the SQL connection outside source control; validate posted book fields; require authentication server-side; use a SQL Server-compatible migration and relationship behavior
- **Scale/Scope**: One search-result add flow, its list display, regression tests, and the separately approved application database provider change

## Constitution Check

*GATE: evaluated before research and re-checked after design.*

| Principle / constraint | Gate | Evidence and notes |
|---|---|---|
| Code quality and maintainability | Pass | Reuses the existing Razor Page, EF Core context, book/list entities, and reading-list store. |
| Testing standards | Pass | Covers normal and duplicate persistence, preserved metadata/status/review, signed-out access, invalid form input, and displayed book details with isolated SQLite/InMemory-backed tests. |
| Consistent user experience | Pass | Provides signed-out sign-in affordance, success/duplicate feedback, and a cover fallback. |
| Performance requirements | Pass | Keeps database and search I/O asynchronous; no broader query or performance behavior is introduced. |
| Technology and security constraints | Pass | Uses .NET 10 and Razor Pages; server validates submitted fields and checks authenticated identity; credentials stay in user secrets or deployment configuration. |
| Approved Azure SQL scope deviation | Accepted | Azure SQL migration was explicitly requested by the project owner. It changes persistence infrastructure beyond the handout's SQLite example and is called out here; it does not change BN-9's user-facing scope. |

## Project Structure

### Documentation (this feature)

```text
specs/003-bn-9-add-to-my-list/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── search-results.md
├── checklists/
│   └── requirements.md
└── tasks.md
```

### Source Code (repository root)

```text
Pages/
├── Index.cshtml(.cs)          # Search results and add action
└── ReadingList.cshtml(.cs)    # Saved book list
Services/
└── ReadingListStore.cs        # Find/create shared book and reader-list entry
Models/
├── Book.cs
└── ReadingList.cs
Data/
└── ApplicationDbContext.cs
Migrations/
└── 20261121170830_InitialSqlServerCreate.cs
tests/IS7024_GroupProject.Tests/
├── PersistenceTests.cs
└── Pages/IndexModelTests.cs
```

**Structure Decision**: Extend the existing single ASP.NET Core Razor Pages application and its service/model/data layers. Keep tests in the existing xUnit project. The SQL Server migration is in the repository-root `Migrations/` directory, matching the current EF Core configuration.

## Complexity Tracking

No constitution violations remain. The Azure SQL provider change is an approved scope deviation, not an exception to a constitution principle.
