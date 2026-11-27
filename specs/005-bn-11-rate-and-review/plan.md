# Implementation Plan: Rate and Review Finished Books

**Branch**: `BN-11-book-reviews-ui-enhancements` | **Date**: 2026-11-27 | **Spec**: [spec.md](spec.md)

**Input**: BN-11 story in [spec.md](spec.md) and its existing implementation, including the review fixes validated during documentation follow-up. This plan records the implemented design rather than proposing a broad rewrite.

## Summary

BN-11 lets a signed-in reader save one 1–5 star rating and optional summary/comment for each Finished book. A later save updates that reader's existing review. Book Details displays the average rating and count from all readers, while the review form shows only the signed-in reader's content and is disabled until that reader finishes the book. Selectable star icons are the BN-11 rating control. The Reading List/navbar/login restyling on this branch is a separate UI spike, explicitly outside the BN-11 card.

The implementation reuses the existing review entity and unique reader/book index. No new migration, table, package, or external service is required.

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`)

**Primary Dependencies**: ASP.NET Core Razor Pages, ASP.NET Core Identity, EF Core 10, Bootstrap, and existing BookNest CSS

**Storage**: Existing `ApplicationDbContext` and SQL Server database; isolated SQLite databases are used in automated tests

**Testing**: xUnit and EF Core SQLite; `dotnet test tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj`; browser checks documented in [quickstart.md](quickstart.md)

**Target Platform**: Server-rendered ASP.NET Core web application

**Project Type**: Web application

**Performance Goals**: Keep persistence asynchronous and scope review aggregation to one book. Calculate the rating count and sum in the database, returning only aggregate values to the application; do not materialize every rating.

**Constraints**: Require a signed-in reader and Finished status for saves; validate ratings and text lengths server-side; preserve one review per reader/book; do not expose another reader's summary/comment; reuse existing schema and dependencies.

**Scale/Scope**: One Book Details view, the signed-in reader's review form, and the all-reader aggregate rating/count. Reading List/navbar/login restyling is a separate same-branch UI spike and is not part of BN-11.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / constraint | Gate | Evidence and notes |
|---|---|---|
| Code quality and maintainability | Pass | Extends the existing Razor Pages app and existing `BookReview` model; no duplicate persistence abstraction is introduced. |
| Testing standards | Pass | Automated tests cover create/update, uniqueness, invalid ratings and text lengths, maximum accepted lengths, aggregate rounding/empty results, review ownership, unauthenticated saves, and unfinished-book rejection. |
| Consistent user experience | Pass | Book Details uses the shared BookNest styling; star controls retain accessible radio semantics, and unfinished-book controls are disabled with the required explanation. |
| Performance requirements | Pass | Persistence is asynchronous and the rating count/sum are computed by the database for one book without materializing every rating. |
| Technology and security constraints | Pass | Uses .NET 10 Razor Pages and existing Identity, validates inputs on the server, authorizes saves against the current reader's Finished entry, and requires no new secret or package. |

## Project Structure

### Documentation (this feature)

```text
specs/005-bn-11-rate-and-review/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── book-details.md
├── checklists/
│   └── requirements.md
├── tasks.md
└── copilot-chat-transcript.md
```

### Source Code (repository root)

```text
Areas/Identity/Pages/
└── _ViewStart.cshtml              # Separate same-branch UI spike; not BN-11 scope
Pages/
├── BookDetails.cshtml
├── BookDetails.cshtml.cs          # Review validation, upsert, gating, and aggregate
├── ReadingList.cshtml             # Separate same-branch UI spike; not BN-11 scope
└── Shared/
    ├── _Layout.cshtml             # Separate same-branch UI spike; not BN-11 scope
    └── _LoginPartial.cshtml       # Separate same-branch UI spike; not BN-11 scope
Models/
├── BookReview.cs                  # Existing review entity and unique reader/book index
├── ReadingList.cs
└── ReadingStatus.cs
Data/
└── ApplicationDbContext.cs        # Existing review relationships
wwwroot/
└── css/site.css                   # Rating styles; other UI-spike styles are out of scope
tests/IS7024_GroupProject.Tests/
└── Pages/
    ├── BookDetailsPageTests.cs
    └── ReadingListPageTests.cs    # Separate same-branch UI spike; not BN-11 scope
```

**Structure Decision**: The existing single ASP.NET Core Razor Pages project, Identity system, EF Core context, and xUnit project are sufficient. This branch reuses the `BookReview` table and unique index on `(UserId, BookId)`; no database migration or new NuGet dependency is planned. The user-facing review contract is documented in [contracts/book-details.md](contracts/book-details.md).

## Existing Implementation Sequence

1. Reuse the existing `BookReview` relationship to a reader and book, including its unique reader/book constraint.
2. Load a book, its database-computed rating count/sum, the current reader's Finished status, and that reader's own review for Book Details.
3. Validate signed-in identity, Finished status, rating range, and text lengths before inserting or updating a review.
4. Present the average/count and accessible star form; prevent edits until the reader finishes the book.
5. Apply accessible selectable-star styling to the Book Details rating control.
6. Validate with focused automated tests, the complete test project, and the browser scenarios in [quickstart.md](quickstart.md).

The Reading List/navbar/login style refresh is recorded only as a separate UI spike on this branch, not as BN-11 work.

## Constitution Check — Post-Design

All gates remain **Pass** after design. The feature reuses current persistence and page boundaries, enforces ownership and finished status in the server handler, computes aggregates in the database, documents user-visible behavior and browser validation, and introduces no dependency or schema change. The separate Reading List/navbar/login UI spike is excluded from BN-11 scope.
