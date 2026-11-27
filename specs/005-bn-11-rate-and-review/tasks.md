# Tasks: Rate and Review Finished Books

**Input**: Design documents in `specs/005-bn-11-rate-and-review/`

**Status**: As-built checklist. All tasks below are marked complete because BN-11 is already implemented on this branch; do not run the implementation workflow.

**Tests**: The feature specification requests normal, validation, and boundary coverage. Existing focused tests and the full test project are included below.

**Organization**: Tasks are grouped by user story. `[P]` marks work that can be performed in parallel without editing the same files.

## Phase 1: Setup

**Purpose**: Confirm the existing app and test project are the implementation baseline.

- [X] T001 [P] Confirm the .NET 10 application and xUnit/SQLite test project in `IS7024_GroupProject.csproj` and `tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj`.

## Phase 2: Foundational

**Purpose**: Reuse the existing identity, persistence, and reading-status foundations; no migration or package is required.

- [X] T002 [P] Reuse `Models/BookReview.cs` with `Rating` constrained to 1–5, `Summary` limited to 200 characters, `Review` limited to 4,000 characters, and the unique `(UserId, BookId)` index.
- [X] T003 [P] Reuse the reader/book relationships and Finished status in `Data/ApplicationDbContext.cs`, `Models/ReadingList.cs`, and `Models/ReadingStatus.cs`.
- [X] T004 Confirm the existing Identity and Book Details page structure in `Models/User.cs` and `Pages/BookDetails.cshtml.cs`; make no schema or dependency changes.

## Phase 3: User Story 1 - Rate and review a finished book (Priority: P1)

**Goal**: A signed-in reader can save and update one 1–5 star rating, optional summary, and optional comment for a Finished book.

**Independent Test**: Sign in, mark The Hobbit Finished, save the specified four-star rating, summary, and comment, reopen details to verify them, then save five stars and confirm the same review is updated.

### Tests for User Story 1

- [X] T005 [US1] Verify a first save persists the selected rating, summary, and comment in `tests/IS7024_GroupProject.Tests/Pages/BookDetailsPageTests.cs`.
- [X] T006 [US1] Verify a repeated save updates the rating, retains the summary/comment, and leaves one reader/book record in `tests/IS7024_GroupProject.Tests/Pages/BookDetailsPageTests.cs`.
- [X] T007 [US1] Verify missing/out-of-range ratings and summary/comment values over 200/4,000 characters are rejected without changing an existing review, while exact maximum lengths are accepted in `tests/IS7024_GroupProject.Tests/Pages/BookDetailsPageTests.cs`.

### Implementation for User Story 1

- [X] T008 [US1] Validate the signed-in reader and Finished status, require rating 1–5, enforce summary maximum 200 and comment maximum 4,000 characters, and create or update the reader/book record in `Pages/BookDetails.cshtml.cs`.
- [X] T009 [US1] Render the reader's saved rating, summary, and comment with accessible selectable star controls in `Pages/BookDetails.cshtml` and `wwwroot/css/site.css`.

**Checkpoint**: A reader's review persists and can be changed without creating a second review.

## Phase 4: User Story 2 - See the average rating (Priority: P1)

**Goal**: Book Details shows the one-decimal average and exact count from all submitted ratings without exposing other readers' summary/comment text.

**Independent Test**: Have Alex and Sam rate the same book 4 and 5 stars; verify `Average rating: 4.5 (2 ratings).` and confirm only the current reader's review text is shown.

### Tests for User Story 2

- [X] T010 [US2] Verify average/count, one-decimal rounding, zero-rating behavior, and loading only the current reader's review in `tests/IS7024_GroupProject.Tests/Pages/BookDetailsPageTests.cs`.

### Implementation for User Story 2

- [X] T011 [US2] Calculate rating count and sum in the database, then render the one-decimal average with correct singular/plural wording in `Pages/BookDetails.cshtml.cs` and `Pages/BookDetails.cshtml`.

**Checkpoint**: Readers see the shared aggregate while review text remains scoped to its author.

## Phase 5: User Story 3 - Rate only after finishing (Priority: P1)

**Goal**: Readers cannot submit a review until their own reading-list entry is Finished.

**Independent Test**: Set The Martian to Want to Read, open details, verify the explanation and disabled controls, then verify a direct save request is rejected.

### Tests for User Story 3

- [X] T012 [US3] Verify unauthenticated saves are challenged and unfinished-book saves are rejected without creating a review in `tests/IS7024_GroupProject.Tests/Pages/BookDetailsPageTests.cs`.

### Implementation for User Story 3

- [X] T013 [US3] Enforce Finished status in the save handler and render `Finish this book to rate it.` with disabled rating, summary, comment, and Save controls in `Pages/BookDetails.cshtml.cs` and `Pages/BookDetails.cshtml`.

**Checkpoint**: Unfinished books cannot be reviewed through either the UI or a direct POST.

## Phase 6: Polish and Cross-Cutting Concerns

**Purpose**: Verify the Book Details UI contract and validate the full BN-11 feature.

- [X] T014 Verify the Book Details markup exposes accessible star choices, singular/plural count wording, and disabled unfinished-book controls in `tests/IS7024_GroupProject.Tests/Pages/BookDetailsPageTests.cs`.
- [X] T015 Run the automated suite from `tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj` and complete the browser scenarios in `specs/005-bn-11-rate-and-review/quickstart.md`.

### Separate Same-Branch UI Spike (Not BN-11 Tasks)

The following requested changes are present on this branch but are not part of the BN-11 Jira card or task count:

- Reading List card proportions, cover sizing, left alignment, and typography in `Pages/ReadingList.cshtml` and `wwwroot/css/site.css`.
- Navbar active-page underline and shared login/Identity styling in `Pages/Shared/_Layout.cshtml`, `Pages/Shared/_LoginPartial.cshtml`, `Areas/Identity/Pages/_ViewStart.cshtml`, and `wwwroot/css/site.css`.

## Dependencies and Execution Order

### Phase Dependencies

- **Setup**: No dependencies.
- **Foundational**: Depends on Setup; complete before user-story work.
- **User Stories**: Each has an independent acceptance test after the shared foundation. Their page-model and Razor view edits overlap, so execute in the listed order when implementing.
- **Polish**: Depends on the three user stories; validate the page contract before the final test and browser run.

### User Story Dependencies

- **US1**: Independent after Foundational; delivers personal review create/update.
- **US2**: Independent after Foundational; delivers the shared aggregate and privacy behavior.
- **US3**: Independent after Foundational; delivers the Finished-only UI and server gate.
- No story depends on another story's user-visible behavior. The listed order avoids concurrent edits to shared Book Details files.

### Parallel Opportunities

- T001, T002, and T003 touch separate project/model files and can be assessed in parallel.
- US1, US2, and US3 have independent test criteria, but implementation edits share `Pages/BookDetails.cshtml` and `Pages/BookDetails.cshtml.cs`; avoid parallel edits to those files.
- The separate Reading List/navbar/login spike edits shared CSS and is not part of the BN-11 implementation task graph.

## Implementation Strategy

### MVP

The smallest useful increment is **US1**: a signed-in reader can save and update a review for a Finished book, with the uniqueness and input constraints verified. US3's Finished-status enforcement is required by US1's save contract and must be included in any deployable MVP. US2 adds the cross-reader aggregate view.

### Incremental Delivery

1. Confirm the shared model and Identity foundation.
2. Deliver US1 personal review create/update and input validation.
3. Deliver US2 aggregate rating/count and review-text privacy.
4. Deliver US3 disabled unfinished-book UI and independent server-side enforcement.
5. Apply the Book Details star-control polish and run automated and browser validation.

## Completion Summary

- **Total BN-11 tasks**: 15
- **By story**: US1: 5; US2: 2; US3: 2; Setup/Foundational: 4; Polish: 2.
- **Independent test criteria**: Defined separately for US1, US2, and US3 above.
- **Suggested MVP**: US1 with its Finished-status precondition; US2 is the additional aggregate story.
- **Separate spike**: Reading List/navbar/login visual changes are documented above but excluded from the BN-11 task count.
- **Format validation**: Every task has a checkbox, sequential task ID, applicable parallel/story labels, and an exact repository file path. `[X]` indicates the as-built work is complete.
