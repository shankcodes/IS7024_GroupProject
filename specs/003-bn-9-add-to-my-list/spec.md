# Feature Specification: Add a Book to My List

**Feature Branch**: `BN-9-Add-book-to-list-tummalga`

**Created**: 2026-11-24

**Status**: Implemented; pending review

**Input**: Jira BN-9: Add a book to My List from search results so readers can save books they want to read.

## User Scenarios & Testing

### User Story 1 - Save a Search Result (Priority: P1)

As a reader, I want to add a book to My List from search results so that I can save books I want to read.

**Why this priority**: Saving a discovery is the main outcome of the story and turns a search result into a reader's next action.

**Independent Test**: Sign in, search for a book that is not already on the reader's list, add it, and verify it appears in My List as Want to Read.

**Acceptance Scenarios**:

1. **Given** a signed-in reader searches for "The Martian" (ISBN 9780553418026) and it is not on their list, **When** they select Add to List, **Then** it appears on My List with Want to Read status.
2. **Given** "The Martian" is already on the reader's list, **When** they select Add to List again, **Then** they see "This book is already on your list." and only one list entry exists.
3. **Given** a visitor is not signed in, **When** they choose the sign-in action for a search result, **Then** they are sent to the sign-in page and no list entry is created.
4. **Given** a reader adds "Dune" by Frank Herbert, **When** they open My List, **Then** its cover, title, and author are shown.

### Edge Cases

- The book already exists in the shared catalog but is not yet on this reader's list.
- The book has no usable cover image.
- A reader submits the same add action more than once.
- An add request is submitted after the reader's account is no longer available.
- Search results are unavailable or contain no books.

## Requirements

### Functional Requirements

- **FR-001**: The application MUST allow only signed-in readers to add a search result to a personal list.
- **FR-002**: Adding a book not already on the reader's list MUST create one entry with Want to Read status.
- **FR-003**: Repeating an add for the same reader and book MUST leave one list entry and display the exact message "This book is already on your list."
- **FR-004**: A visitor who is not signed in MUST be directed to sign in before a search result can be added.
- **FR-005**: My List MUST show each saved book's title, author, and cover; a missing or unusable cover MUST have a fallback.
- **FR-006**: Adding an already-catalogued book MUST NOT change its shared title, author, or cover based on values submitted by a reader.
- **FR-007**: After a successful add, the application MUST provide confirmation on My List.
- **FR-008**: Search and list operations MUST leave existing reading status, ratings, and reviews unchanged.

### Key Entities

- **Reader**: An authenticated person whose saved list entries are private to their account.
- **Book**: A catalog item with a stable identity, title, author, and optional cover.
- **Reading-list entry**: The association between one reader and one book, including its reading status.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Every successful add produces exactly one Want to Read entry for that reader and book.
- **SC-002**: Repeated add attempts display the specified duplicate message and leave the entry count at one.
- **SC-003**: A signed-out visitor cannot create a list entry and is offered the sign-in flow.
- **SC-004**: A reader can identify an added book in My List from its title and author, with its cover or a fallback image visible.
- **SC-005**: Adding an existing catalog item does not change its metadata for other readers.

## Assumptions

- Search results provide a stable identity and descriptive metadata for each book.
- The shared catalog may contain books added by other readers, while list entries remain reader-specific.
- A search result without a cover can still be added and displayed using a fallback image.
- Changing reading status, ratings, reviews, and social interactions are outside this story.
