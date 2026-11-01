# Quickstart Validation: BN-8 SQLite Database and Authentication Foundation

## Prerequisites

- .NET 10 SDK
- PowerShell on Windows
- Repository root as the current directory

## Build and Start

```powershell
dotnet restore IS7024_GroupProject.csproj
dotnet build IS7024_GroupProject.csproj
dotnet run --project IS7024_GroupProject.csproj
```

Expected result: a fresh clone starts successfully and creates `BookNest.db` through the EF Core migrations. The database contains `Books`, `ReadingList`, and `ReadingGoal`, along with Identity and review-support tables.

## BN-8 Acceptance Checks

1. Start the application from a fresh clone and confirm `BookNest.db` is created.
2. Save "The Martian" to a reading list.
3. Stop and restart the application.
4. Confirm "The Martian" remains in `ReadingList`.
5. Attempt to save the same `GoogleBooksId` again.
6. Confirm no duplicate `Books` row is created.

The unique `GoogleBooksId` index and migration-backed schema enforce the duplicate-prevention requirement.

## Migration Command

To apply migrations directly instead of through application startup:

```powershell
dotnet ef database update --project IS7024_GroupProject.csproj
```

## Authentication and Ownership Checks

1. Register two accounts with different email addresses.
2. Sign in as the first account and create a reading-list entry and yearly goal.
3. Sign out and sign in as the second account.
4. Confirm the second account cannot see or change the first account's reading list or goal.
5. Confirm duplicate email registration is rejected.
6. Confirm unauthenticated access to protected personal-data actions redirects to sign-in once page authorization is enabled.

## Review Checks

1. Add the same catalog book to two users' reading lists.
2. Submit one rating and review per user for that book.
3. Confirm a second review by the same user/book is rejected or updates the existing review.
4. Confirm aggregate ratings use approved ratings only.
5. Add a comment and a like to a review.
6. Confirm repeating the same like does not create a duplicate record.
7. Confirm a non-admin cannot change moderation state.
8. Confirm an Admin-role user can approve or hide review and comment content once moderation pages are implemented.

## Database Constraint Checks

Verify these constraints through integration tests or direct database inspection:

- Unique `Books.GoogleBooksId`
- Unique `(ReadingList.UserId, ReadingList.BookId)`
- Composite primary key `(ReadingGoal.UserId, ReadingGoal.Year)`
- Unique `(BookReview.UserId, BookReview.BookId)`
- Unique `(ReviewLike.UserId, ReviewLike.BookReviewId)`
- Rating values are null or between 1 and 5
- Goal targets are at least 1
