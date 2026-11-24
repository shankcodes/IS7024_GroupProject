# BookNest Models

This folder contains the domain and persistence models for the multi-user BookNest application.

## Entities

### User

`User` extends ASP.NET Core Identity's `IdentityUser<int>`. Identity owns authentication data such as the user ID, email, password hash, login state, and roles.

Navigation properties:

- `ReadingLists`: the user's reading progress records
- `ReadingGoals`: the user's yearly goals
- `BookReviews`: the user's reviews
- `ReviewComments`: comments written by the user
- `ReviewLikes`: reviews liked by the user

### Book

`Book` is the shared book catalog record.

Fields:

- `BookId`: primary key
- `Title`: book title
- `Author`: author name
- `CoverUrl`: optional cover URL
- `GoogleBooksId`: required external ID and unique value

A book can have reading-list records and reviews from many users.

### ReadingList

`ReadingList` stores one user's reading progress for one book.

Fields:

- `ReadingListId`: primary key
- `UserId`: foreign key to `User`
- `BookId`: foreign key to `Book`
- `Status`: `WantToRead`, `Reading`, or `Finished`
- `FinishedDate`: optional completion date

The `(UserId, BookId)` pair is unique, so a user cannot add the same book twice.

### ReadingGoal

`ReadingGoal` stores one user's yearly reading target.

Fields:

- `UserId`: foreign key to `User`
- `Year`: goal year
- `TargetBooks`: target count, at least 1

`(UserId, Year)` is the composite primary key, allowing each user to have one goal per year.

### BookReview

`BookReview` stores a user's rating and review for a book.

Fields:

- `BookReviewId`: primary key
- `UserId`: foreign key to `User`
- `BookId`: foreign key to `Book`
- `Rating`: optional value from 1 to 5
- `Summary`: optional short review summary
- `Review`: optional full review text
- `CreatedAt`: creation timestamp
- `UpdatedAt`: optional last-update timestamp
- `ModerationStatus`: `Pending`, `Approved`, `Rejected`, or `Hidden`
- `ModerationReason`: optional moderation explanation
- `ModeratedAt`: optional moderation timestamp

The `(UserId, BookId)` pair is unique, so each user can have one review per book. Aggregate ratings should be calculated from individual reviews rather than stored as one value on `Book`.

### ReviewComment

`ReviewComment` stores a comment on a review.

Fields include the comment author, content, timestamps, and moderation metadata. Comments are separate records so they can grow, be moderated, and later support replies without changing `BookReview`.

### ReviewLike

`ReviewLike` records that a user liked a review.

The `(UserId, BookReviewId)` pair is unique, so a user can like a review at most once.

## Enums

- `ReadingStatus`: `WantToRead`, `Reading`, `Finished`
- `ReviewModerationStatus`: `Pending`, `Approved`, `Rejected`, `Hidden`

## Relationships

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

## Database Constraints

`ApplicationDbContext` configures:

- Unique `Book.GoogleBooksId`
- Unique `(ReadingList.UserId, ReadingList.BookId)`
- Composite primary key `(ReadingGoal.UserId, ReadingGoal.Year)`
- Unique `(BookReview.UserId, BookReview.BookId)`
- Unique `(ReviewLike.UserId, ReviewLike.BookReviewId)`
- Enum values stored as readable text in SQL Server
- Cascading relationships from users and books to their owned records

## Design Notes

- Ratings and reviews are user-owned data, so they are stored separately from the shared `Book` catalog.
- `BookReview.Summary` means a short summary of the user's review. A publisher or API-provided book synopsis would belong on `Book` or a separate book-metadata table instead.
- Review aggregates such as average rating and review count should be calculated from approved reviews.
- Authorization must filter reading lists, goals, reviews, comments, and likes by the authenticated user's ID where appropriate.
- The `Admin` role is available for application-level moderation. It should not be treated as unrestricted database access.
- Migrations in `Migrations` are the source of truth for creating and updating the SQL Server schema.
