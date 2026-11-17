# Tasks: BN-002 Homepage Dashboard

**Input**: Design documents from `specs/002-bn-002-homepage-dashboard/` (`plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/homepage.md`, `quickstart.md`)

**Tests**: Requested. Use the existing xUnit project and SQLite test setup. Place each automated test task immediately after the implementation task it verifies.

**Organization**: Tasks are grouped by specification user story. Every task is scoped to a single focused change intended to take less than one hour.

## Phase 1: Setup

**Purpose**: Confirm the existing test baseline; no scaffolding or dependency changes are needed.

- [X] T001 Run `dotnet test tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj` and record the baseline result.

---

## Phase 2: Foundational

**Purpose**: No new foundation work is required. Reuse Razor Pages, Identity, `ApplicationDbContext`, existing entities/services, and xUnit. Do not add services, models, tables, migrations, packages, or constitution changes.

**Checkpoint**: T001 passes before feature work begins.

---

## Phase 3: User Story 1 - Find a book from Home (Priority: P1) 🎯 MVP

**Goal**: Redesign the homepage while preserving only the existing Google Books search behavior and feedback.

**Independent Test**: Existing search queries and feedback remain unchanged, and the redesigned root page works at desktop and mobile widths.

- [X] T002 [US1] Restyle the hero and GET search form in `Pages/Index.cshtml` while keeping the existing `Query` binding, result card data, and messages; use `/images/no-cover.svg` for invalid cover URLs and implement a one-time browser load-error fallback that disables its handler before changing the image source. Preserve/link Add to List only if already implemented; otherwise show no Add to List button.
- [X] T003 [US1] Extend `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` to verify title and author queries are passed to the existing search service and its actual returned results are exposed by the page model; do not require results for unsupported keyword types.
- [X] T004 [US1] Extend `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` to verify genre-related and review-related keywords are passed unchanged through the existing search flow and assertions depend only on the fake service's returned data.
- [X] T005 [US1] Add tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` for empty-query validation/no service call, no-results feedback, and service-unavailable feedback.
- [X] T006 [US1] Add cover tests in `tests/IS7024_GroupProject.Tests/GoogleBooks/GoogleBooksServiceTests.cs` and `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` for null, empty, whitespace-only, malformed, relative, and unsupported-scheme URLs; assert the rendered search-result `<img>` uses `/images/no-cover.svg` rather than only checking the underlying `CoverUrl` value.
- [X] T007 [P] [US1] Add warm cream and sage color variables and homepage surface/card styles in `wwwroot/css/site.css`.
- [X] T008 [US1] Add mobile breakpoints and 320px-safe search/card layouts in `wwwroot/css/site.css`.
- [X] T009 [P] [US1] Update branding and navigation in `Pages/Shared/_Layout.cshtml` to existing Home, Feed, My Reading List, and Privacy pages while preserving account links; link to Add to List only if already implemented and do not add a nonfunctional button.

**Checkpoint**: Search behavior and outcomes are unchanged; homepage styling is ready for responsive dashboard sections.

---

## Phase 4: User Story 2 - Review my reading dashboard (Priority: P1)

**Goal**: Show the signed-in reader's real Currently Reading and current-year finished books, grouped by recorded completion month.

**Independent Test**: Seed two readers with Reading and Finished entries, including undated and boundary-date entries; verify only the current reader's valid entries are exposed and rendered.

- [X] T010 [US2] Resolve the authenticated reader and asynchronously load only that reader's `Reading` entries with their associated books in `Pages/Index.cshtml.cs`.
- [X] T011 [US2] Add SQLite-backed tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` proving Currently Reading contains only the authenticated reader's entries.
- [X] T012 [US2] Load only the authenticated reader's `Finished` entries with an actual completion date in the current calendar year in `Pages/Index.cshtml.cs`.
- [X] T013 [US2] Add tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` proving undated, prior-year, next-year, and other-reader Finished entries are excluded from recent activity.
- [X] T014 [US2] Render the Currently Reading section and its no-reading-books empty state in `Pages/Index.cshtml` using real book data; render `/images/no-cover.svg` for invalid URLs and use the shared one-time browser load-error fallback.
- [X] T015 [US2] Add rendered-page tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` verifying empty and populated Currently Reading states, asserting each invalid-cover card's `<img src>` equals `/images/no-cover.svg` for null, empty, whitespace-only, malformed, relative, and unsupported-scheme URLs; verify the image error handler is disabled before fallback and a failed placeholder cannot recurse.
- [X] T016 [US2] Render current-year Finished books grouped by their recorded completion month, including the empty recent-activity state, in `Pages/Index.cshtml`; use `/images/no-cover.svg` for invalid cover URLs and the shared one-time browser load-error fallback.
- [X] T017 [US2] Add rendered-page tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` verifying recent-finished month grouping and empty state use actual completion dates, and cards with invalid covers render `<img src="/images/no-cover.svg">`.

**Checkpoint**: Activity sections show only real, user-owned entries and never assign dates to undated books.

---

## Phase 5: User Story 3 - Track my yearly goal (Priority: P2)

**Goal**: Show and update the signed-in reader's saved current-year goal using actual completed-book data.

**Independent Test**: A reader with a configured goal sees the current year, target, completion count, rounded percentage, and corresponding progress bar. No-goal readers see no fabricated target or progress; invalid goal values preserve saved data.

- [X] T018 [US3] Load only the authenticated reader's `ReadingGoal` for the current calendar year in `Pages/Index.cshtml.cs`.
- [X] T019 [US3] Add tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` for the matching current-year goal and the absence of a goal record.
- [X] T020 [US3] Calculate the authenticated reader's current-year Finished count from entries with actual `FinishedDate` values in `Pages/Index.cshtml.cs`.
- [X] T021 [US3] Add tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` verifying count inclusion at the start of the year and exclusion of undated, other-year, and other-reader entries.
- [X] T022 [US3] Calculate goal progress as finished count divided by target, rounded to a whole percentage with halfway values up and capped at 100% in `Pages/Index.cshtml.cs`.
- [X] T023 [US3] Add tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` for a halfway-round-up percentage and the 100% cap while preserving the actual finished count.
- [X] T024 [US3] Render a Reading Goal card in `Pages/Index.cshtml` showing current year, actual finished count, saved target, percentage, accessible progress bar, and a no-goal state without fabricated values.
- [X] T025 [US3] Add tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` verifying goal summary view data contains the current year, finished count, target, percentage, and no-goal state.
- [X] T026 [US3] Add an authenticated goal-save handler in `Pages/Index.cshtml.cs` that creates or updates only the current reader's current-year `ReadingGoal` and rejects targets below 1 without modifying the saved goal.
- [X] T027 [US3] Add tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` for valid goal saving, the exact "Goal must be at least 1 book." message, unchanged goal on invalid input, and user isolation.

**Checkpoint**: Goal summary and updates reflect only the current reader's saved annual goal and actual completions.

---

## Phase 6: User Story 4 - Choose a book to read next (Priority: P2)

**Goal**: Help the signed-in reader choose from their saved Want to Read books.

**Independent Test**: Seed WantToRead entries for two readers; verify only the current reader's books are shown, and an empty collection has a search/list link.

- [X] T028 [US4] Load only the authenticated reader's `WantToRead` entries and associated books in `Pages/Index.cshtml.cs`.
- [X] T029 [US4] Add tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` proving WantToRead entries are user-scoped and empty when none are saved.
- [X] T030 [US4] Render saved Want to Read book cards and the search/My Reading List empty state in `Pages/Index.cshtml`; use `/images/no-cover.svg` for invalid cover URLs and the shared one-time browser load-error fallback.
- [X] T031 [US4] Add rendered-page tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` verifying Read Next contains only saved books, no sample recommendations, and cards with null, empty, whitespace-only, malformed, relative, or unsupported-scheme covers render `<img src="/images/no-cover.svg">`.

**Checkpoint**: Read Next is based only on the signed-in reader's saved books.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Complete signed-out behavior, verify rendered responsive/goal UI, and run integrated validation.

- [X] T032 Add signed-out prompts for personal sections while keeping public search visible and usable in `Pages/Index.cshtml`.
- [X] T033 Add tests in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs` verifying signed-out requests expose no reader-specific goal, list, completion, or progress data.
- [ ] T034 Verify the rendered Reading Goal card shows current year, finished count, saved target, rounded percentage, progress bar, and no-goal empty state using real test-reader data; record results in `specs/002-bn-002-homepage-dashboard/quickstart.md`.
- [ ] T035 Verify Home at 320px, tablet, and desktop widths; confirm navigation reaches Home, Feed, My Reading List, and Privacy; and confirm any Add to List entry point is linked only when the feature exists, with no inert button otherwise. Record results in `specs/002-bn-002-homepage-dashboard/quickstart.md`.
- [X] T036 Run `dotnet test tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj` and report any homepage test failures.
- [X] T037 Run `dotnet build IS7024_GroupProject.csproj` and report any homepage build failures.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: T001 confirms the test baseline; no scaffolding is needed.
- **Foundational (Phase 2)**: No tasks; existing infrastructure is sufficient.
- **User stories (Phases 3-6)**: Start after T001. Complete by priority for incremental delivery. Tasks editing the same Razor page, page model, or test file run sequentially.
- **Polish (Phase 7)**: T034-T037 follow implementation and focused tests.

### User Story Dependencies

- **US1 (P1)**: Independent; preserves existing search and delivers the visual homepage MVP.
- **US2 (P1)**: Uses existing reading-list data and is independent of search behavior.
- **US3 (P2)**: Depends on the user-scoped Finished data from US2; the inline goal-setting path reuses the existing `ReadingGoal` entity because there is no separate goal page.
- **US4 (P2)**: Uses the user-scoped reading-list data pattern established in US2.

### Within Each User Story

- Each test task immediately follows the implementation task it verifies.
- Page-model data tasks precede view tasks that consume their properties.
- Keep edits to `Index.cshtml`, `Index.cshtml.cs`, and `IndexModelTests.cs` sequential; different files may proceed in parallel when dependencies are satisfied.
- Every task is scoped to one focused change intended to take under one hour.

### Parallel Opportunities

- **T009** navigation can proceed in parallel with page-model work because it edits `Pages/Shared/_Layout.cshtml` only.
- **T007** cream/sage base styles can proceed in parallel with independent `Pages/Index.cshtml` work after visual class names are agreed; **T008** responsive CSS follows T007 because both edit `site.css`.
- **T036-T037** test and build validation can run alongside T034-T035 browser verification after implementation is complete; run T036 and T037 sequentially because both use the same project outputs.
- Do not parallelize tasks that edit the same file.

---

## Parallel Example: User Story 1

```text
After agreeing homepage class names, these distinct-file tasks may proceed in parallel:
Task: T002 Update Pages/Index.cshtml search presentation
Task: T007 Add cream/sage base styles in wwwroot/css/site.css
Task: T009 Update navigation in Pages/Shared/_Layout.cshtml

Run T003-T006 search regression tests after T002 is complete; run T008 after T007.
```

## Implementation Strategy

### MVP First (User Story 1)

1. Run T001 to establish the test baseline.
2. Implement homepage search presentation (T002), then verify supported search behavior with T003-T006.
3. Complete theme, responsiveness, and navigation (T007-T009) and verify the visual shell.
4. Stop for an independently demonstrable search-and-homepage MVP before adding personal data sections.

### Incremental Delivery

1. Complete US1 and verify existing search behavior, feedback, and cover fallback.
2. Add US2 user-scoped Currently Reading and recent finishes with date-boundary and empty-state tests.
3. Add US3 current-year Reading Goal display, percentage/bar, and update/validation tests.
4. Add US4 Read Next from saved WantToRead entries and verify user scoping.
5. Complete signed-out, responsive/browser, and build/test validation in Phase 7.

## Notes

- Every task is estimated to fit within one focused hour; split any task further if repository-level test setup or UI complexity exceeds that bound.
- `[P]` indicates distinct files and no dependency on an incomplete task.
- Genre/review-keyword tests verify unchanged query forwarding and actual service results only; they do not assert specialized matching.
- A usable cover URL must be a well-formed absolute HTTP or HTTPS URL; null, empty, whitespace-only, malformed, relative, and unsupported-scheme URLs render `/images/no-cover.svg`. Tests assert the rendered `<img src>`, not just `CoverUrl` values.
- Browser image-load failures switch to `/images/no-cover.svg` once, with the error handler disabled or removed before changing the source so a placeholder failure cannot recurse. T006, T015, T017, and T031 cover the search, Currently Reading, recent-finished, and Read Next cards respectively.
- Preserve/link Add to List only if already implemented. Do not implement it in this story; if it is unavailable, show navigation only to existing features and no nonfunctional button.
- Reuse the existing model names and fields exactly: `ReadingList.Status`, `ReadingList.FinishedDate`, `ReadingGoal.Year`, and `ReadingGoal.TargetBooks`.
- Do not change unrelated functionality, existing migrations, or `.specify/memory/constitution.md`.
