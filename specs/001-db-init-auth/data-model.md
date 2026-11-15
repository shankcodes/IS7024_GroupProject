# Data Model: Multi-User Reading Data and Authentication

## User

Identity-backed authenticated account.

- Key: `Id` (`int`, inherited Identity key)
- Identity data: email, normalized email, password hash, login and role data
- Relationships: many `ReadingList`, `ReadingGoal`, `BookReview`, `ReviewComment`, and `ReviewLike` records

## Book

Shared catalog record.

- Key: `BookId` (`int`)
- `Title`: required text
- `Author`: required text
- `CoverUrl`: nullable text
- `GoogleBooksId`: required text, unique
- Relationships: many `ReadingList` and `BookReview` records

## ReadingList

A user's reading progress for a book.

- Key: `ReadingListId` (`int`)
- `UserId`: required foreign key to `User`
- `BookId`: required foreign key to `Book`
- `Status`: `WantToRead`, `Reading`, or `Finished`, stored as text
- `FinishedDate`: nullable date/time
- Constraint: unique `(UserId, BookId)`

## ReadingGoal

A user's annual reading target.

- Composite key: `(UserId, Year)`
- `UserId`: required foreign key to `User`
- `Year`: required integer
- `TargetBooks`: integer at least 1

## BookReview

One user's rating and review of a book.

- Key: `BookReviewId` (`int`)
- `UserId`: required foreign key to `User`
- `BookId`: required foreign key to `Book`
- `Rating`: nullable integer from 1 through 5
- `Summary`: nullable text, maximum 200 characters
- `Review`: nullable text, maximum 4000 characters
- `CreatedAt`: required UTC timestamp
- `UpdatedAt`: nullable UTC timestamp
- `ModerationStatus`: `Pending`, `Approved`, `Rejected`, or `Hidden`, stored as text
- `ModerationReason`: nullable text, maximum 1000 characters
- `ModeratedAt`: nullable timestamp
- Constraint: unique `(UserId, BookId)`

## ReviewComment

A user comment on a review.

- Key: `ReviewCommentId` (`int`)
- `BookReviewId`: required foreign key to `BookReview`
- `UserId`: required foreign key to `User`
- `Content`: required text, maximum 1000 characters
- `CreatedAt`: required UTC timestamp
- `UpdatedAt`: nullable UTC timestamp
- Moderation status, reason, and timestamp follow `BookReview` rules

## ReviewLike

A user's like on a review.

- Key: `ReviewLikeId` (`int`)
- `BookReviewId`: required foreign key to `BookReview`
- `UserId`: required foreign key to `User`
- `CreatedAt`: required UTC timestamp
- Constraint: unique `(UserId, BookReviewId)`

## Relationships and Delete Behavior

```text
User 1 --- many ReadingList
User 1 --- many ReadingGoal
User 1 --- many BookReview
User 1 --- many ReviewComment
User 1 --- many ReviewLike
Book 1 --- many ReadingList
Book 1 --- many BookReview
BookReview 1 --- many ReviewComment
BookReview 1 --- many ReviewLike
```

User- and book-owned dependent records use cascading relationships in the current EF configuration. Page handlers must still authorize access by the authenticated user's ID before modifying records.

## Aggregate Rules

- Average rating and review count are calculated from approved `BookReview` records.
- A review with no rating may contain review text, but it does not contribute to the average.
- A user can update their own review but cannot create a second review for the same book.
- A user can like a review once and can remove their own like.
