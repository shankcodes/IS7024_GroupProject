# Tasks: SQLite Database Initialization and Multi-User Reading Data

**Input**: Design documents from `specs/001-db-init-auth/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, and `quickstart.md`

**Tests**: Included because the project constitution requires automated coverage for normal, validation, authorization, and boundary behavior.

## Phase 1: Setup

**Purpose**: Establish the project dependencies and test structure needed by the feature.

- [X] T001 [P] Add EF Core SQLite, Identity EF Core, Identity UI, and EF design dependencies in `IS7024_GroupProject.csproj`
- [X] T002 [P] Create the focused automated test project under `tests/IS7024_GroupProject.Tests/`
- [X] T003 [P] Configure the `BookNest.db` connection string in `appsettings.json`

## Phase 2: Foundational

**Purpose**: Build the persistence, identity, migration, and role foundation required by all user stories.

- [X] T004 Configure `ApplicationDbContext` in `Data/ApplicationDbContext.cs` as the Identity and application EF context
- [X] T005 [P] Implement the catalog, reading, goal, user, review, comment, like, and moderation models under `Models/`
- [X] T006 Configure unique `Book.GoogleBooksId`, unique `(ReadingList.UserId, ReadingList.BookId)`, composite `(ReadingGoal.UserId, ReadingGoal.Year)`, unique `(BookReview.UserId, BookReview.BookId)`, and unique `(ReviewLike.UserId, ReviewLike.BookReviewId)` constraints in `Data/ApplicationDbContext.cs`
- [X] T007 [P] Configure ASP.NET Core Identity, unique email validation, role support, and authentication middleware in `Program.cs`
- [X] T008 [P] Seed only the `Admin` role in `Services/IdentitySeed.cs` without storing an administrator password in source control
- [X] T009 Generate and apply EF migrations under `Data/Migrations/`, including the Identity, reading, review, comment, and like tables

**Checkpoint**: A fresh clone can create `BookNest.db` and the shared persistence/identity foundation is available.

## Phase 3: User Story 1 - Persist Reading Data (Priority: P1) - MVP

**Goal**: Create the Books, ReadingList, and ReadingGoal persistence required by Jira BN-8.

**Independent Test**: Start from a fresh clone, confirm `BookNest.db` and the three core tables are created, save "The Martian", restart the application, and confirm it remains without a duplicate book row.

- [ ] T010 [P] [US1] Add model-validation tests for required book fields and `ReadingGoal.TargetBooks >= 1` in `tests/IS7024_GroupProject.Tests/`
- [X] T011 [P] [US1] Add integration tests for first-start migration creation of `BookNest.db` and the `Books`, `ReadingList`, and `ReadingGoal` tables in `tests/IS7024_GroupProject.Tests/`
- [X] T012 [US1] Implement book persistence and duplicate `GoogleBooksId` handling in the owning Razor Page or service under `Pages/` or `Services/`
- [X] T013 [US1] Implement reading-list persistence with the default `WantToRead` status in `Services/ReadingListStore.cs` and `Pages/ReadingList.cshtml.cs`
- [ ] T014 [US1] Implement yearly reading-goal persistence and finished-book counting in the owning Razor Page or service under `Pages/` or `Services/`
- [X] T015 [US1] Run the BN-8 restart-persistence and duplicate-book scenarios from `specs/001-db-init-auth/quickstart.md`

**Checkpoint**: BN-8 is independently demonstrable and testable.

## Phase 4: User Story 2 - Create and Use an Account (Priority: P1)

**Goal**: Let readers register and authenticate with secure account handling.

**Independent Test**: Register two accounts, sign in and sign out with each, and confirm duplicate email registration is rejected.

- [ ] T016 [P] [US2] Add authentication integration tests for registration, login, logout, duplicate email, and invalid credentials in `tests/IS7024_GroupProject.Tests/`
- [X] T017 [US2] Configure standard Identity account routes and shared login state in `Pages/Shared/` and `Pages/Account/`
- [ ] T018 [US2] Add server-side validation and user-facing feedback for account forms in `Pages/Account/`
- [X] T019 [US2] Verify password hashing, cookie authentication, and unique email behavior through the Identity service configuration in `Program.cs`

**Checkpoint**: Readers can create and use independent authenticated sessions.

## Phase 5: User Story 3 - Keep Personal Data Isolated (Priority: P1)

**Goal**: Ensure personal data is visible and mutable only by its owner.

**Independent Test**: Create data under two accounts and confirm each account can access only its own list, goals, reviews, comments, and likes.

- [ ] T020 [P] [US3] Add authorization integration tests for cross-user read, update, delete, and unauthenticated access in `tests/IS7024_GroupProject.Tests/`
- [X] T021 [US3] Require authentication on the personal reading-list page in `Pages/ReadingList.cshtml.cs`
- [ ] T022 [US3] Scope reading-list, goal, review, comment, and like queries to the authenticated user ID in `Pages/` and `Services/`
- [ ] T023 [US3] Return safe not-found or forbidden behavior when a user targets another user's record in `Pages/`
- [ ] T024 [US3] Add server-side ownership validation for create, update, and delete handlers in `Pages/`

**Checkpoint**: Cross-user access attempts are denied and personal records remain isolated.

## Phase 6: User Story 4 - Review and Moderate Books (Priority: P2)

**Goal**: Support user reviews, aggregate ratings, comments, likes, and Admin moderation.

**Independent Test**: Submit reviews from multiple users, calculate an approved aggregate, add a comment and like once, and change moderation state as an Admin.

- [ ] T025 [P] [US4] Add integration tests for rating validation, review uniqueness, approved aggregates, comments, likes, and moderation authorization in `tests/IS7024_GroupProject.Tests/`
- [ ] T026 [P] [US4] Add review and comment validation for nullable ratings, rating range `1..5`, and content length limits in `Models/BookReview.cs` and `Models/ReviewComment.cs`
- [ ] T027 [US4] Implement review create/update behavior for finished books in the owning Razor Page or service under `Pages/` or `Services/`
- [ ] T028 [US4] Implement approved-review aggregate rating and review count queries in the owning Razor Page or service under `Pages/` or `Services/`
- [ ] T029 [US4] Implement comment create/update/delete behavior with ownership checks in the owning Razor Page or service under `Pages/` or `Services/`
- [ ] T030 [US4] Implement idempotent review-like add/remove behavior using the unique like constraint in the owning Razor Page or service under `Pages/` or `Services/`
- [ ] T031 [US4] Add Admin-only moderation pages and handlers protected by `[Authorize(Roles = "Admin")]` in `Pages/`
- [ ] T032 [US4] Apply moderation visibility rules so pending, rejected, and hidden content is not shown as approved content in `Pages/` and `Services/`

**Checkpoint**: Review community and moderation flows are independently demonstrable.

## Phase 7: Polish and Cross-Cutting Concerns

**Purpose**: Complete documentation, security, performance, and release validation.

- [X] T033 [P] Update `README.md` and `Models/README.md` with the final BN-8 schema, authentication behavior, and moderation boundaries
- [ ] T034 [P] Add quickstart validation coverage from `specs/001-db-init-auth/quickstart.md` to `tests/IS7024_GroupProject.Tests/`
- [ ] T035 Review all database access for asynchronous calls, indexed ownership filters, and avoidance of repeated queries in `Pages/` and `Services/`
- [ ] T036 Run `dotnet build IS7024_GroupProject.csproj` and the complete automated test suite with no new warnings
- [X] T037 Verify no secrets, default administrator passwords, or local database files are committed to source control

## Dependencies and Execution Order

### Phase Dependencies

- **Phase 1** has no dependencies.
- **Phase 2** depends on Phase 1 and blocks all user stories.
- **User Story 1** depends on Phase 2 and is the BN-8 MVP.
- **User Story 2** depends on Phase 2 and can proceed alongside User Story 1.
- **User Story 3** depends on User Stories 1 and 2 because it protects their persisted data and requires authenticated identities.
- **User Story 4** depends on User Stories 1 and 2; ownership checks from User Story 3 are required before public review workflows are exposed.
- **Phase 7** depends on all selected user stories.

### Parallel Opportunities

- T001, T002, and T003 can run in parallel.
- T005, T007, and T008 can run in parallel after the project dependency setup.
- T010 and T011 can run in parallel within User Story 1.
- T016 can run in parallel with T017 after the Identity foundation exists.
- T020 can be prepared in parallel with page authorization work.
- T025 and T026 can run in parallel within User Story 4.
- T033 and T034 can run in parallel during polish.

## Implementation Strategy

1. Complete Phase 1 and Phase 2.
2. Deliver User Story 1 as the BN-8 MVP and validate the three Jira acceptance criteria.
3. Add authentication, then ownership authorization before exposing personal multi-user workflows.
4. Add reviews, comments, likes, and Admin moderation incrementally.
5. Run the full quickstart, automated tests, build, and security checks before review.
