# Feature Specification: SQLite Database Initialization and Multi-User Reading Data

**Feature Branch**: `db-init-and-auth-tummalga`

**Created**: 2026-11-01

**Status**: Draft

**Input**: Jira BN-8: "Set up SQLite database (Books, ReadingList, ReadingGoal)" plus the approved multi-user authentication and review foundation.

## Jira Work Item BN-8

The initial delivery must satisfy these database acceptance criteria:

1. On a fresh clone, first application startup creates `BookNest.db` with `Books`, `ReadingList`, and `ReadingGoal` tables through the EF Core migration.
2. A saved book such as "The Martian" remains in `ReadingList` after the application restarts.
3. Saving an existing `GoogleBooksId` does not create a duplicate `Books` row.

## User Scenarios & Testing

### User Story 1 - Persist Reading Data (Priority: P1)

As a reader, I want my books, reading progress, and yearly goals to persist so that my reading history is available when I return.

**Why this priority**: Persistent data is the foundation for every other reading workflow.

**Independent Test**: Create a book, add it to a reading list, set a yearly goal, restart the application, and confirm the records remain available.

**Acceptance Scenarios**:

1. **Given** a book is not in the catalog, **When** it is added with a valid external book ID, **Then** the book is stored once.
2. **Given** a reader has a book in their list, **When** they change its status, **Then** the new status is persisted.
3. **Given** a reader sets a yearly target of at least one book, **When** the goal is saved, **Then** it is associated with that reader and year.

---

### User Story 2 - Create and Use an Account (Priority: P1)

As a reader, I want to create an account and sign in so that my reading data is private to me.

**Why this priority**: Multi-user ownership and privacy require a reliable identity before personal data is exposed.

**Independent Test**: Register two accounts, sign in as each account, and verify that each account receives an independent authenticated session.

**Acceptance Scenarios**:

1. **Given** an unused valid email address, **When** a reader registers with a valid password, **Then** an account is created and the reader can sign in.
2. **Given** an existing account, **When** the reader signs out, **Then** protected actions require signing in again.
3. **Given** an email already belongs to an account, **When** another reader tries to register it, **Then** registration is rejected with a useful validation message.

---

### User Story 3 - Keep Personal Data Isolated (Priority: P1)

As a reader, I want my reading list, goals, reviews, comments, and likes separated from other readers so that no user can modify another user's personal data.

**Why this priority**: Authorization is a security requirement for a public multi-user application.

**Independent Test**: Create data under two accounts and verify that each account can read and modify only its own records, while shared book catalog records remain visible.

**Acceptance Scenarios**:

1. **Given** two authenticated readers, **When** each opens their reading list, **Then** each sees only their own list entries.
2. **Given** a reader attempts to edit or delete another reader's record, **When** the request is processed, **Then** the operation is denied without exposing the record.
3. **Given** an unauthenticated visitor, **When** they access a protected personal-data action, **Then** they are required to sign in.

---

### User Story 4 - Review and Moderate Books (Priority: P2)

As a reader, I want to rate and review books, comment on reviews, and like useful reviews so that the community can share opinions. As a moderator, I want to review reported content so that inappropriate content can be hidden.

**Why this priority**: Reviews are the main proprietary community data, while moderation provides the minimum control needed for public contributions.

**Independent Test**: Submit a review, add a comment, like the review once, calculate the aggregate rating, and approve or hide content as an administrator.

**Acceptance Scenarios**:

1. **Given** a reader has finished a book, **When** they submit a rating from 1 to 5 and optional review text, **Then** one review is stored for that reader and book.
2. **Given** multiple readers have approved ratings for a book, **When** the book rating is displayed, **Then** the aggregate is calculated from those ratings.
3. **Given** a reader has already liked a review, **When** they like it again, **Then** the review receives no duplicate like from that reader.
4. **Given** content is pending moderation, **When** an administrator approves or hides it, **Then** its moderation state is updated and visibility follows that state.

### Edge Cases

- A duplicate external book ID must not create a second catalog book.
- A reader must not add the same book to their list twice.
- A reader must not create two yearly goals for the same year.
- Ratings outside 1 through 5 must be rejected.
- A review, comment, or like referencing a missing user or book must be rejected.
- Deleting a user must not leave orphaned personal records.
- A non-administrator must not access moderation actions.
- Empty, overlong, or invalid email and password inputs must produce validation errors without revealing sensitive details.

## Requirements

### Functional Requirements

- **FR-001**: System MUST create `BookNest.db` and its `Books`, `ReadingList`, and `ReadingGoal` tables on first startup of a fresh clone.
- **FR-002**: System MUST persist books, reading-list entries, reading goals, reviews, comments, and likes across application restarts.
- **FR-003**: System MUST prevent duplicate catalog books by enforcing a unique external book identifier.
- **FR-004**: System MUST allow a user to have at most one reading-list entry per book.
- **FR-005**: System MUST allow a user to have at most one reading goal per year.
- **FR-006**: System MUST allow users to register, sign in, sign out, and maintain authenticated sessions.
- **FR-007**: System MUST require unique email addresses for accounts and must never store plaintext passwords.
- **FR-008**: System MUST associate personal reading data with its owning user.
- **FR-009**: System MUST deny unauthenticated access to protected personal-data actions.
- **FR-010**: System MUST prevent users from reading or changing another user's reading lists, goals, reviews, comments, and likes.
- **FR-011**: System MUST validate ratings as nullable or an integer from 1 through 5.
- **FR-012**: System MUST allow at most one like per user and review.
- **FR-013**: System MUST calculate aggregate book ratings from individual user ratings rather than storing only one rating on the book.
- **FR-014**: System MUST track moderation state and moderation metadata for reviews and comments.
- **FR-015**: System MUST restrict moderation actions to users with the administrator role.
- **FR-016**: System MUST validate user input server-side and must not expose secrets in source control or user-facing errors.

### Key Entities

- **User**: An authenticated reader with identity and role membership.
- **Book**: A shared catalog record identified by a unique external book ID.
- **ReadingList**: A user's status and completion date for a book.
- **ReadingGoal**: A user's target for a specific year.
- **BookReview**: One user's rating, review, summary, timestamps, and moderation state for a book.
- **ReviewComment**: A user's comment on a review, including timestamps and moderation state.
- **ReviewLike**: A user's like on a review, unique per user and review.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Two users can use the application concurrently with no cross-user reading-list or goal data visible between their accounts.
- **SC-002**: Duplicate attempts for the same user/book list entry, user/year goal, user/book review, or user/review like are rejected or treated idempotently.
- **SC-003**: Valid account registration and sign-in complete successfully, while invalid credentials and duplicate emails are rejected with actionable validation.
- **SC-004**: A reviewer can submit a valid rating and review, and the resulting aggregate rating includes all approved ratings for that book.
- **SC-005**: Only administrator accounts can change review or comment moderation state.
- **SC-006**: The application builds successfully with no new compiler warnings, and automated tests cover normal, validation, authorization, and boundary behavior.

## Assumptions

- Email/password authentication is the initial account method; external identity providers are out of scope for this feature.
- The application is a personal reading tracker extended for multiple users, not a fully public social network with direct messaging.
- A user may submit one review per book and may update that review later.
- The `Admin` role is an application-level moderation role, not unrestricted database access.
- SQLite is sufficient for the current project scope and local deployment.
- Existing book-search integrations provide the external book identifier used for catalog deduplication.
- Account creation and password-reset UI can use the authentication framework's standard flows before custom branding is added.
