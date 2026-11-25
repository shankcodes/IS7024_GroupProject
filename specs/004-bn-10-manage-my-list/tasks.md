# Tasks: Manage My List

**Input**: Design documents from `specs/004-bn-10-manage-my-list/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/reading-list.md`

**Tests**: Included for every acceptance criterion, validation failure, authentication boundary, ownership boundary, state transition, and preservation of Add to List behavior.

**Task sizing**: Each task is scoped to be completed in under one hour.

## Phase 1: Setup

**Purpose**: Confirm the existing project and test structure require no new dependencies or persistence artifacts.

- [X] T001 [P] Verify that the existing `ReadingList`, `ReadingStatus`, `Book`, `ReadingListStore`, and `ApplicationDbContext` support BN-10 without new models, services, tables, or migrations; record the verification in `specs/004-bn-10-manage-my-list/research.md`.
- [X] T002 [P] Prepare the isolated SQLite test helpers and authenticated PageModel test context for BN-10 in `tests/IS7024_GroupProject.Tests/Pages/ReadingListModelTests.cs`; confirm that the existing test project has no Razor rendering host and do not add a NuGet package.

## Phase 2: Foundational

**Purpose**: Establish the ownership-scoped persistence operations used by all My List actions.

- [X] T003 Add an ownership-scoped status update operation to `Services/ReadingListStore.cs` that accepts the reading-list entry ID, authenticated user ID, and supported `ReadingStatus` value, saves asynchronously, and returns a safe success/not-found result.
- [X] T004 Add persistence tests immediately for the status operation in `tests/IS7024_GroupProject.Tests/PersistenceTests.cs`, covering Want to Read to Reading, Reading to Finished, FinishedDate set to `DateTime.Today`, unfinished transitions clearing FinishedDate, and no mutation of another user's entry.
- [X] T005 Add an ownership-scoped remove operation to `Services/ReadingListStore.cs` that deletes only the matching user's `ReadingList` entry, leaves the shared `Book` intact, saves asynchronously, and returns a safe success/not-found result.
- [X] T006 Add persistence tests immediately for the remove operation in `tests/IS7024_GroupProject.Tests/PersistenceTests.cs`, covering successful deletion, shared book preservation, missing entries, and attempts against another user's entry.

**Checkpoint**: Store-level status transitions and removals are deterministic, ownership-scoped, and covered before page handlers are added.

## Phase 3: User Story 1 - View and Update Reading Status (Priority: P1)

**Goal**: Let an authenticated reader view their saved books and change each entry between Want to Read, Reading, and Finished.

**Independent Test**: Seed The Martian as Want to Read, open My List, change it to Reading, then Finished, and verify status and finished-date behavior; change it back to an unfinished status and verify the date is cleared.

- [X] T007 [US1] Add authenticated status-update input binding, server-side entry/status validation, current-user lookup, store invocation, safe failure feedback, and redirect behavior to `Pages/ReadingList.cshtml.cs` while preserving the existing Add to List handler.
- [X] T008 [US1] Add PageModel tests immediately for `Pages/ReadingList.cshtml.cs` in `tests/IS7024_GroupProject.Tests/Pages/ReadingListModelTests.cs`, covering authenticated status changes, invalid status values, missing entries, unauthenticated challenge behavior, and ownership protection.
- [X] T009 [US1] Update `Pages/ReadingList.cshtml` to display each user's saved book title, author, cover/fallback, current status, and applicable FinishedDate using the existing responsive BookNest card structure.
- [X] T010 [US1] Add page-level display tests immediately in `tests/IS7024_GroupProject.Tests/Pages/ReadingListModelTests.cs` for user-specific entry loading, current status values, FinishedDate visibility data, title ordering, and exclusion of another user's books.
- [X] T011 [US1] Add an accessible status dropdown to each entry in `Pages/ReadingList.cshtml` with exactly Want to Read, Reading, and Finished choices, associated labels, and a named POST form that targets the status handler.
- [X] T012 [US1] Add page-contract coverage immediately in `tests/IS7024_GroupProject.Tests/Pages/ReadingListPageTests.cs` for these named status scenarios: Want to Read to Reading, and Reading to Finished with today's date. Keep the tests explicit about whether they verify PageModel data/handler contracts or source-level page contracts; rendered HTML is verified by the manual browser checks in `quickstart.md` because no Razor rendering host exists.

**Checkpoint**: User Story 1 independently demonstrates list display, all supported status changes, FinishedDate set/clear behavior, authentication, and ownership protection.

## Phase 4: User Story 2 - Remove a Saved Book (Priority: P1)

**Goal**: Let an authenticated reader remove a book from their own list without deleting shared catalog data.

**Independent Test**: Seed The Martian for Reader A, submit Remove, and verify it is absent from Reader A's list while the shared book and any other reader's entry remain.

- [X] T013 [US2] Add an authenticated remove handler with entry-ID validation, current-user lookup, ownership-scoped store invocation, safe failure feedback, and redirect behavior to `Pages/ReadingList.cshtml.cs`.
- [X] T014 [US2] Add PageModel tests immediately in `tests/IS7024_GroupProject.Tests/Pages/ReadingListModelTests.cs` for successful removal, invalid/missing entry IDs, unauthenticated access, and cross-reader removal attempts.
- [X] T015 [US2] Add a clearly labeled Remove form action for each saved book to `Pages/ReadingList.cshtml`, preserving antiforgery protection and the existing BookNest responsive layout.
- [X] T016 [US2] Add page-contract coverage immediately in `tests/IS7024_GroupProject.Tests/Pages/ReadingListPageTests.cs` for the removal scenario without deleting the shared `Book` record: verify the Remove action's handler and entry-ID contract, verify shared-book preservation through `PersistenceTests.cs`, and verify the rendered Remove form through the manual browser checks in `quickstart.md`.

**Checkpoint**: User Story 2 independently demonstrates safe removal of the current reader's entry and protection of other readers' data.

## Phase 5: User Story 3 - Empty List and Existing Add Flow (Priority: P2)

**Goal**: Provide the exact empty-state guidance and ensure existing Add to List behavior continues to work.

**Independent Test**: Open My List with no entries, verify the exact empty-state message, add a book through the existing flow, and verify the book appears as Want to Read without the empty-state message.

- [X] T017 [US3] Replace the existing empty-list text in `Pages/ReadingList.cshtml` with the exact message `Your list is empty. Search for a book to get started.` and keep it conditional on zero entries.
- [X] T018 [US3] Add PageModel/page-contract tests immediately in `tests/IS7024_GroupProject.Tests/Pages/ReadingListPageTests.cs` for the exact empty-list message `Your list is empty. Search for a book to get started.` and the empty-versus-nonempty state; verify the rendered message after opening an empty list and after removing the final entry using the manual browser checks in `quickstart.md`, because the existing test project has no Razor rendering host.
- [X] T019 [US3] Verify the existing Add to List form and handler remain unchanged in `Pages/ReadingList.cshtml` and `Pages/ReadingList.cshtml.cs`, including success feedback, duplicate protection, and Want to Read initialization.
- [X] T020 [US3] Add regression tests immediately in `tests/IS7024_GroupProject.Tests/PersistenceTests.cs` and `tests/IS7024_GroupProject.Tests/Pages/ReadingListModelTests.cs` proving Add to List still creates one Want to Read entry, preserves duplicate behavior, and does not change existing status or shared book metadata.

**Checkpoint**: User Story 3 independently demonstrates the exact empty state and the preserved Add to List workflow.

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Verify shared styling, accessibility, responsive behavior, and the complete acceptance suite.

- [X] T021 [P] Apply existing BookNest layout, Bootstrap utility classes, `.book-grid`, `.book-card`, `.empty-state`, button, focus, validation, and cover-fallback patterns in `Pages/ReadingList.cshtml`; avoid introducing a new stylesheet unless an existing styling gap is demonstrated.
- [X] T022 [P] Document and execute the rendered-markup browser checks in `specs/004-bn-10-manage-my-list/quickstart.md` for accessible status labels, the Remove form, validation feedback, responsive card layout, exact empty-state text, and FinishedDate display; do not describe these checks as automated Razor rendering tests.
- [X] T023 Run the complete automated test project and fix only BN-10 regressions using `dotnet test tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj`.
- [X] T024 Run every manual scenario in `specs/004-bn-10-manage-my-list/quickstart.md`, including authentication, both status transitions, FinishedDate clearing, removal, empty state, Add to List preservation, and a cross-reader request attempt.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No production-code dependencies; T002 provides shared test setup for later tests.
- **Foundational (Phase 2)**: Depends on T001; T003/T004 and T005/T006 can proceed in parallel because they touch separate store operations and related test cases.
- **User Story 1 (Phase 3)**: Depends on Phase 2; T007/T008 precede the display and dropdown work, while T009/T010 and T011/T012 can proceed in parallel after the status handler contract is settled.
- **User Story 2 (Phase 4)**: Depends on Phase 2 and can proceed in parallel with User Story 1 if the team coordinates changes to `ReadingList.cshtml.cs` and `ReadingList.cshtml`.
- **User Story 3 (Phase 5)**: Depends on the list display from User Story 1 and can proceed after the existing Add to List regression tests are identified.
- **Polish (Phase 6)**: Depends on all desired user stories and their tests being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Depends only on Foundational; delivers the MVP status-management workflow.
- **User Story 2 (P1)**: Depends only on Foundational; independently delivers safe removal, but shares the existing page files with User Story 1.
- **User Story 3 (P2)**: Depends on User Story 1's list display and existing Add to List behavior; it is independently testable after those contracts exist.

### Parallel Opportunities

- T001 and T002 can run in parallel.
- T003/T004 and T005/T006 can run in parallel.
- After Phase 2, User Stories 1 and 2 can be assigned to separate developers if page-file conflicts are coordinated.
- T009/T010 and T011/T012 can run in parallel after T007/T008.
- T017/T018 and T019/T020 can run in parallel after the shared page display is stable.
- T021 and T022 can run in parallel with each other after the final markup shape is agreed.

## Implementation Strategy

### MVP First

1. Complete Phase 1 and Phase 2.
2. Complete User Story 1 and run its checkpoint tests.
3. Complete User Story 2 and run its checkpoint tests.
4. Stop for review: status changes and removal are independently demonstrable.

### Incremental Delivery

1. Add store operations and persistence tests.
2. Deliver status display and transitions with authentication and ownership checks.
3. Deliver removal and verify shared book preservation.
4. Deliver the exact empty state and verify Add to List regression behavior.
5. Apply styling/accessibility polish and run the complete quickstart validation.

### Implementation Constraints

- Use the existing `ReadingList`, `ReadingStatus`, `Book`, `ReadingListStore`, and `ApplicationDbContext`.
- Do not create models, services, tables, migrations, or duplicate persistence paths.
- Keep FinishedDate transition logic in the existing store boundary.
- Keep all status and remove mutations scoped by authenticated user ID.
- Do not modify `.specify/memory/constitution.md`.
