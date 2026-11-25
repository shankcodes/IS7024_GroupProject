# Feature Specification: Manage My List

**Feature Branch**: `004-bn-10-manage-my-list`
**Created**: 2026-11-24
**Status**: Draft
**Input**: Jira BN-10: Build My List page with status dropdown and Remove so readers can keep their reading list up to date.

## User Scenarios & Testing

### User Story 1 - View and update a saved book (Priority: P1)

As a reader, I want to change a saved book's reading status so that My List reflects what I am currently reading.

**Why this priority**: Keeping status current is the primary purpose of managing My List and supports the reader's ongoing reading workflow.

**Independent Test**: Sign in with The Martian already on My List as Want to Read, change it to Reading, and verify that My List shows Reading.

**Acceptance Scenarios**:

1. **Given** The Martian is on My List as Want to Read, **When** the reader changes its status to Reading, **Then** My List shows The Martian as Reading.
2. **Given** The Martian is on My List as Reading, **When** the reader changes its status to Finished, **Then** My List shows The Martian as Finished with today's date.
3. **Given** the reader changes a finished book to Want to Read or Reading, **When** the updated list is displayed, **Then** the finished date is no longer shown for that entry.

---

### User Story 2 - Remove a saved book (Priority: P1)

As a reader, I want to remove a book from My List so that books I no longer want to track do not remain in my list.

**Why this priority**: Removing outdated entries is necessary for the list to remain useful and accurate.

**Independent Test**: Sign in with The Martian on My List, select Remove, and verify that The Martian no longer appears on My List.

**Acceptance Scenarios**:

1. **Given** The Martian is on My List, **When** the reader selects Remove, **Then** The Martian no longer appears on My List.
2. **Given** another reader owns a reading-list entry, **When** the current reader attempts to change or remove it, **Then** the other reader's entry remains unchanged and inaccessible to the current reader.

---

### User Story 3 - See an empty list message (Priority: P2)

As a reader, I want clear guidance when My List has no books so that I know how to get started.

**Why this priority**: Empty-state guidance prevents confusion and directs the reader back to the existing book-discovery flow.

**Independent Test**: Sign in with no saved entries, open My List, and verify the exact empty-state message.

**Acceptance Scenarios**:

1. **Given** My List is empty, **When** the reader opens My List, **Then** they see "Your list is empty. Search for a book to get started."
2. **Given** the reader adds a book using the existing Add to List flow, **When** they open My List, **Then** the book appears and the empty-state message is not shown.

### Edge Cases

- A status update is submitted for an entry that no longer exists; the reader's list remains consistent and no unrelated entry is changed.
- An unsupported status value is submitted; the update is rejected without changing the saved entry.
- A remove request is submitted for an entry owned by another reader; no entry outside the current reader's list is removed.
- A finished date is set only when the status becomes Finished.
- A finished date is cleared when a Finished entry changes to an unfinished status.
- Removing the last entry displays the empty-state message.
- Existing Add to List behavior continues to create and display a Want to Read entry.

## Requirements

### Functional Requirements

- **FR-001**: My List MUST display each saved book's title, author, cover or fallback, current reading status, and finished date when applicable.
- **FR-002**: A signed-in reader MUST be able to change the status of an entry belonging to that reader to Want to Read, Reading, or Finished.
- **FR-003**: When an entry changes to Finished, the system MUST record today's date as its finished date.
- **FR-004**: When an entry changes from Finished to an unfinished status, the system MUST clear its finished date.
- **FR-005**: A signed-in reader MUST be able to remove an entry belonging to that reader, after which the entry MUST no longer appear on My List.
- **FR-006**: The system MUST prevent a reader from modifying or removing another reader's reading-list entries.
- **FR-007**: When a reader has no saved entries, My List MUST display the exact message: "Your list is empty. Search for a book to get started."
- **FR-008**: The existing Add to List behavior MUST remain available and MUST continue to display newly added books on My List.
- **FR-009**: Status changes and removals MUST preserve the shared book information and any unrelated reader data.
- **FR-010**: The feature MUST use the existing book and reading-list data and behavior; it MUST NOT introduce duplicate models, tables, or services.
- **FR-011**: Status and remove actions MUST provide server-side validation and actionable feedback when an operation is invalid or cannot be completed.
- **FR-012**: My List controls and status feedback MUST follow the existing BookNest styling and remain usable on responsive layouts.

### Key Entities

- **Reader**: An authenticated person whose reading-list entries are private to that account.
- **Book**: A shared catalog item displayed in the reader's list.
- **Reading-list entry**: A reader-specific association with a book, current reading status, and optional finished date.
- **Reading status**: One of Want to Read, Reading, or Finished.

## Success Criteria

### Measurable Outcomes

- **SC-001**: 100% of the defined status-change scenarios pass, including Want to Read to Reading and Reading to Finished with today's date.
- **SC-002**: 100% of status changes from Finished to an unfinished status remove the finished date.
- **SC-003**: 100% of successful remove actions make the selected book absent from the reader's My List.
- **SC-004**: 100% of attempted cross-reader modifications and removals leave the other reader's entries unchanged.
- **SC-005**: 100% of empty My List views display the specified empty-state message, and the message disappears after a successful add.
- **SC-006**: Automated tests cover every acceptance scenario, validation failure, ownership boundary, finished-date transition, and preservation of existing Add to List behavior.
- **SC-007**: The feature introduces no duplicate persistent entities and does not require readers to learn a new navigation path to find My List.

## Assumptions

- Readers must be signed in to view and manage their personal My List.
- The existing Book, reading-list, and reading-status information is the source of truth for this feature.
- Today's date means the date in the application's current calendar context at the time the status becomes Finished.
- A status update replaces the current status for the selected reader's entry; it does not create a second entry.
- Removing an entry does not delete the shared book from the catalog or affect other readers' entries.
- The existing Add to List flow remains the way readers discover and add books.
- Existing site navigation, layout, validation, feedback, and fallback-cover behavior remain in scope and are reused.
