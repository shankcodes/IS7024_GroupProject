# Data Model: BN-11 Rate and Review Finished Books

## Existing Entities

### BookReview

One record represents one reader's optional rating and review text for one book.

| Field | Type | Constraints / behavior |
|---|---|---|
| `BookReviewId` | `int` | Primary key |
| `UserId` | `int` | Required foreign key to `User`; identifies the review owner |
| `BookId` | `int` | Required foreign key to `Book` |
| `Rating` | `int?` | Nullable model field constrained to 1–5; BN-11 save requires a rating |
| `Summary` | `string?` | Maximum 200 characters |
| `Review` | `string?` | Maximum 4,000 characters; this is the reader's comment field in the BN-11 form |
| `CreatedAt` | `DateTime` | Defaults to UTC now when the entity is created |
| `UpdatedAt` | `DateTime?` | Set to UTC now when BN-11 saves the review |
| `ModerationStatus` | `ReviewModerationStatus` | Existing moderation field; defaults to `Pending`, not changed by BN-11 |
| `ModerationReason` | `string?` | Existing moderation metadata, maximum 1,000 characters; outside BN-11 |
| `ModeratedAt` | `DateTime?` | Existing moderation metadata; outside BN-11 |

**Uniqueness**: A unique index on `(UserId, BookId)` ensures at most one `BookReview` per reader and book. The save handler queries by that pair, creates a record when absent, and updates the same record when present.

**Relationships**: `BookReview` belongs to one `User` and one `Book`. Both foreign-key relationships cascade on deletion. It may also have separate `ReviewComment` and `ReviewLike` child records; those existing features are not the BN-11 summary/comment fields.

### ReadingList

The reader's entry for a book gates rating eligibility.

| Field | Type | Constraints / behavior |
|---|---|---|
| `ReadingListId` | `int` | Primary key |
| `UserId` | `int` | Required reader foreign key |
| `BookId` | `int` | Required book foreign key |
| `Status` | `ReadingStatus` | Stored as a string; eligible only when `Finished` |
| `FinishedDate` | `DateTime?` | Set when status changes to `Finished`; not needed to validate a review |

**Uniqueness**: A unique index on `(UserId, BookId)` ensures one list entry per reader and book.

### Book and User

- `Book` owns a collection of `BookReview` records through `BookId`.
- `User` owns a collection of `BookReview` records through `UserId`.
- The existing `Book` and `User` entities are reused without BN-11 schema changes.

## Derived Values

For a selected book, the database groups `BookReview` records with a non-null `Rating` by `BookId` and returns only the count and sum.

- `RatingCount` is the number of those rated records.
- `AverageRating` is the arithmetic mean rounded to one decimal place using midpoint-away-from-zero rounding.
- With no ratings, the displayed average is `0.0` and the count is zero.
- The label uses `rating` for a count of one and `ratings` otherwise.
- Individual rating rows are not materialized in application memory to calculate the aggregate.
- Summary and comment text from other readers are not loaded for display by this feature.

## Validation and State Transitions

1. A save request must identify an authenticated current user.
2. The current user's `ReadingList` entry for the submitted `BookId` must have status `Finished`.
3. The submitted rating must be an integer from 1 through 5 and is required by the save handler.
4. Summary and comment are optional and limited to 200 and 4,000 characters respectively.
5. If no `BookReview` exists for the `(UserId, BookId)` pair, create it; otherwise update its rating, summary, comment, and `UpdatedAt`.
6. The unique index is the persisted invariant if concurrent requests attempt duplicate inserts.

## Persistence Impact

BN-11 reuses the current `BookReview`, `ReadingList`, `Book`, and `User` tables. No migration, table, column, index, or package change is required.
