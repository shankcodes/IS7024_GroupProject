# Data Model: Manage My List

## Reader

An authenticated Identity user who owns personal reading-list entries.

- **Identity**: Integer `User.Id`.
- **Relationship**: A reader may have many `ReadingList` entries.
- **Access rule**: The current authenticated user's ID is required for every read, status update, and removal operation.

## Book

A shared catalog record displayed in a reader's list.

- **Identity**: Integer `Book.BookId`.
- **Display data**: Required title and author; optional cover URL; stable Google Books identifier.
- **Relationship**: A book may be referenced by multiple readers' list entries.
- **Mutation rule**: Status and removal operations do not alter or delete the shared book.

## Reading-list entry

The existing `ReadingList` association between one reader and one book.

- **Identity**: Integer `ReadingList.ReadingListId`.
- **Relationships**: References one `User` and one `Book`.
- **Uniqueness**: At most one entry per reader/book pair, enforced by the existing unique index.
- **Status**: Existing `ReadingStatus` value: `WantToRead`, `Reading`, or `Finished`.
- **Finished date**: Nullable `FinishedDate`.
- **Update rule**:
  - `WantToRead` or `Reading` sets `FinishedDate` to `null`.
  - `Finished` sets `FinishedDate` to the current date.
- **Ownership rule**: A mutation succeeds only when both `ReadingListId` and authenticated `UserId` identify the same entry.
- **Missing/unauthorized mutation**: No unrelated entry is changed or removed; the page returns actionable feedback.

## State transitions

| Current status | Requested status | Resulting status | FinishedDate |
|---|---|---|---|
| WantToRead | WantToRead | WantToRead | `null` |
| WantToRead | Reading | Reading | `null` |
| WantToRead | Finished | Finished | Today's date |
| Reading | WantToRead | WantToRead | `null` |
| Reading | Reading | Reading | `null` |
| Reading | Finished | Finished | Today's date |
| Finished | WantToRead | WantToRead | Cleared |
| Finished | Reading | Reading | Cleared |
| Finished | Finished | Finished | Today's date |

## Persistence impact

No entity, relationship, index, database table, or migration is required. Existing `ApplicationDbContext` mapping already persists the status enum as a string and includes the nullable finished date.
