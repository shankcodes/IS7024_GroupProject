# Tasks: Add a Book to My List

**Input**: Design documents in `specs/003-bn-9-add-to-my-list/`
**Status**: Retrospective task record; implementation tasks are marked complete where present in the reviewed branch. Do not run `/speckit-implement` against this already completed feature.

## Phase 1: Setup

**Purpose**: Record the application and test-project setup used by the feature.

- [X] T001 [P] Configure the application for .NET 10 and the SQL Server EF Core provider in `IS7024_GroupProject.csproj` and `Program.cs`.
- [X] T002 [P] Keep SQLite and EF Core InMemory providers available for isolated automated tests in `tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj`.

## Phase 2: Foundational

**Purpose**: Prepare persistence and database configuration before the reader flow.

- [X] T003 [P] Configure SQL Server-compatible user relationship delete behavior in `Data/ApplicationDbContext.cs`.
- [X] T004 Create the SQL Server initial schema migration in `Migrations/20261121170830_InitialSqlServerCreate.cs`.
- [X] T005 [P] Document local Azure SQL connection setup, User Secrets, firewall caveat, and VS Code SQL Server extension steps in `README.md`.

## Phase 3: User Story 1 - Save a Search Result (Priority: P1) - MVP

**Goal**: Let a reader save a search result once, see its details on My List, and guide signed-out visitors to sign in.

**Independent Test**: Sign in, add a search result, verify one Want to Read entry with book details, repeat the add to verify duplicate feedback, then sign out and verify the sign-in flow.

### Implementation and focused tests

- [X] T006 Reuse or create the shared book by its stable identifier and add one Want to Read entry without overwriting existing shared metadata or changing an existing entry in `Services/ReadingListStore.cs`.
- [X] T007 Add authenticated and signed-out search-result actions, server-side input validation, redirects, and success/duplicate feedback in `Pages/Index.cshtml` and `Pages/Index.cshtml.cs`.
- [X] T008 [P] Show saved book title, author, status, and a cover fallback on My List in `Pages/ReadingList.cshtml`.
- [X] T009 [P] Verify handler authorization and invalid input in `tests/IS7024_GroupProject.Tests/Pages/IndexModelTests.cs`; verify saved details, duplicate behavior, and preservation of shared metadata, reading status, and reviews in `tests/IS7024_GroupProject.Tests/PersistenceTests.cs`.

**Checkpoint**: The complete reader flow is independently testable through the automated persistence tests and the manual scenarios in `quickstart.md`.

## Phase 4: Polish and Documentation

**Purpose**: Capture the completed implementation and provide repeatable validation steps.

- [X] T010 [P] Document the BN-9 requirements and acceptance scenarios in `specs/003-bn-9-add-to-my-list/spec.md`.
- [X] T011 [P] Record the plan, decisions, entities, UI contract, and run instructions in `specs/003-bn-9-add-to-my-list/plan.md`, `specs/003-bn-9-add-to-my-list/research.md`, `specs/003-bn-9-add-to-my-list/data-model.md`, `specs/003-bn-9-add-to-my-list/contracts/search-results.md`, and `specs/003-bn-9-add-to-my-list/quickstart.md`.
- [X] T012 Validate the feature documentation against the requirements checklist in `specs/003-bn-9-add-to-my-list/checklists/requirements.md`.

## Dependencies and Execution Order

### Phase dependencies

- **Setup**: Completed; the application and test-project providers are configured.
- **Foundational**: Completed; the SQL Server schema and local connection instructions are present.
- **User Story 1**: Completed; depends on the application's persistence configuration and uses the existing models/services.
- **Polish**: Completed; documents the implemented behavior.

### User story dependencies

- **US1 (P1)**: No dependency on another user story. This is the only story and forms the MVP.

### Parallel opportunities

- T001 and T002 touch separate project files and can proceed in parallel.
- T003 and T005 touch separate files and can proceed in parallel; T004 follows T003 because the migration reflects the context model.
- After T006, the handler in T007, list rendering in T008, and focused tests in T009 touch separate files and can proceed in parallel.
- T010 and T011 create separate documentation files and can proceed in parallel; T012 validates them afterward.

## Implementation Strategy

The completed MVP is US1: add a search result, prevent duplicates, preserve shared metadata, display the saved book, and require sign-in. Azure SQL was an explicitly requested infrastructure change recorded in the plan; it is not an additional user story. For any future changes, verify the focused test project and manual steps in `quickstart.md` before merging.

## Follow-up Outside This Branch's Completed Tasks

- Verify the Azure App Service connection string, database identity/permissions, and startup migration in the deployed environment; local connectivity alone does not verify deployment configuration.
- Recheck integration with PR #9's homepage work when that PR is merged.
- Restrict the currently broad database firewall rule to the required application and developer networks before production use.
