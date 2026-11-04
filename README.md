<p align="left">
  <img src="./images/BookNest_logo.png" alt="BookNest logo" width="250">
</p>

<h1 align="left">BookNest</h1>
<p align="left"><em>Discover books, organize your reading list, and track your reading goals.</em></p>

---

## Table of Contents

1. [Introduction](#introduction)
2. [Icon / Logo](#icon--logo)
3. [Storyboard](#storyboard)
4. [Projects](#projects)
5. [Requirements](#requirements)
6. [Data Sources](#data-sources)
7. [Authentication and Authorization](#authentication-and-authorization)
8. [Database](#database)
9. [Model Design](#model-design)
10. [Team Composition](#team-composition)

---

## Introduction

**BookNest** is a simple personal reading tracker. Search for a book, add it to your list, mark where you are with it, and leave a quick rating and review when you finish.

**Features**

- **Search** – Find books by title or author.
- **Reading List** – Mark books as *Want to Read*, *Reading*, or *Finished*.
- **Rate & Review** – Give finished books 1–5 stars and a short review.
- **Reading Goal** – Set how many books you want to read this year and see your progress.

Built with **C# / ASP.NET Core** in **Visual Studio 2026**.

---

## Icon / Logo

<img src="images/BookNest_logo.png" alt="BookNest logo" width="150">

An open book resting in a nest: a cozy home for your reading list.

---

## Storyboard

| Screen | Widgets |
|---|---|
| **Home** | Reading goal progress bar, *Reading* list, link to Search |
| **Search** | Search box, Search button, results (cover, title, author), *Add to List* button |
| **My List** | Books grouped by status, status dropdown, *Remove* button |
| **Book Details** | Cover, title, author, description, star rating, review box, *Save* button |

---

## Projects

Work is tracked on our [Jira Project board](https://mail-team-g68n68ck.atlassian.net/jira/software/projects/BN/boards/1/backlog) with the columns **To Do → In Progress → Peer Review → Ready to Deploy → Done**.

| Sprint | Epic | Goal |
|---|---|---|
| Week 1 | Design, Setup & Book Search | Storyboard and design document first, then repo setup, database, Google Books search, Search page |
| Week 2 | Reading List & Reviews | My List, status, ratings and reviews, Open Library backup |
| Week 3 | Reading Goal, Partner Service & Demo | Reading goal, partner team's service, tests, final demo |

---

## Requirements

### 1. Search for Books

**As a reader, I want to search for books by title or author so that I can find books to add to my list.**

1. **Given** Google Books is available and contains "The Hobbit" by J.R.R. Tolkien (ISBN 9780547928227),
   **When** I search for "The Hobbit" on the Home page,
   **Then** I should see "The Hobbit" by J.R.R. Tolkien in the results.

2. **Given** Google Books is available and contains "The Martian" and "Project Hail Mary" by Andy Weir,
   **When** I search for "Andy Weir",
   **Then** I should see both "The Martian" and "Project Hail Mary" in the results.
   
3. **Given** I am on the Home page and the search box is empty,
   **When** I click Search,
   **Then** I should see "Please enter a title or author." and no API call should be made.

4. **Given** Google Books is available and has no book matching "xqzvplmnt",
   **When** I search for "xqzvplmnt",
   **Then** I should see "No books found."

5. **Given** Google Books returns "Dune" by Frank Herbert (ISBN 9780441172719) without a cover image, and Open Library has a cover for ISBN 9780441172719,
   **When** I search for "Dune",
   **Then** I should see one "Dune" result that shows the Open Library cover.

### 2. Manage My Reading List

**As a reader, I want to mark books as Want to Read, Reading, or Finished so that I can keep track of what I am reading.**

1. **Given** I am signed in and "The Martian" (ISBN 9780553418026) is not on my list,
   **When** I click *Add to List* on its search result,
   **Then** "The Martian" should appear on my list as *Want to Read*.
   
2. **Given** I am signed in and "The Martian" is on my list as *Want to Read*,
   **When** I change its status to *Reading*,
   **Then** "The Martian" should show as *Reading* and appear in *Currently Reading* on the Home page.
   
3. **Given** I am signed in and "The Martian" is already on my list as *Want to Read*,
   **When** I click *Add to List* on "The Martian" again,
   **Then** I should see "This book is already on your list." and only one copy should exist.

### 3. Rate and Review Books

**As a reader, I want to rate and review books I finish so that I can remember what I thought of them.**

1. **Given** I am signed in, "The Hobbit" is on my list as *Finished*, and I have not reviewed it,
   **When** I select 4 stars, type the summary "Fun adventure" and the comment "Bilbo's journey kept me hooked," and click Save,
   **Then** "The Hobbit" should show my 4-star rating and my review.
   
2. **Given** I am signed in and I gave "The Hobbit" 4 stars,
   **When** I change my rating to 5 stars and click Save,
   **Then** "The Hobbit" should show my 5-star rating, and I should still have only one review for it.
   
3. **Given** reader Alex rated "The Hobbit" 4 stars and reader Sam rated it 5 stars,
   **When** I open the Book Details page for "The Hobbit",
   **Then** I should see "Average rating: 4.5 (2 ratings)."
   
4. **Given** I am signed in and "The Martian" is on my list as *Want to Read*,
   **When** I open its Book Details page,
   **Then** my rating and review boxes should be disabled with the message "Finish this book to rate it."

### 4. Set a Reading Goal

**As a reader, I want to set a yearly reading goal so that I can stay motivated to read.**

1. **Given** I am signed in and have no reading goal for 2026,
   **When** I set a goal of 12 books,
   **Then** the Home page should show "0 of 12 books read in 2026."
   
2. **Given** I am signed in, my 2026 goal is 12 books, and I have finished 3 books in 2026,
   **When** I mark "The Hobbit" as *Finished* on November 1, 2026,
   **Then** the Home page should show "4 of 12 books read in 2026."
   
3. **Given** I am signed in and on the reading goal form,
   **When** I enter 0 and click Save,
   **Then** I should see "Goal must be at least 1 book." and my goal should not change.

<!--### 5. Use Another Team's Service

**As a reader, I want to see [information from the other team's service] on a book's page so that I can [benefit to the reader].**

*Placeholder: fill this in once our class partner team and their service are confirmed.*

1. **Given** [the other team's service] is available,
   **When** I open the Book Details page for "The Hobbit",
   **Then** I should see [specific data from their service].

2. **Given** [the other team's service] is unavailable,
   **When** I open the Book Details page for "The Hobbit",
   **Then** the page should still load, without that section.

--->

## Data Sources

| Source | Example | Used For | Match Key |
|---|---|---|---|
| [Google Books API](https://developers.google.com/books/docs/v1/using) | `https://www.googleapis.com/books/v1/volumes?q=the+hobbit` | Main book search (RESTful JSON) | ISBN-13 from `volumeInfo.industryIdentifiers` |
| [Open Library API](https://openlibrary.org/developers/api) | `https://openlibrary.org/isbn/9780547928227.json` | Backup search and missing covers (RESTful JSON) | ISBN-13 in the URL, or the `isbn` field in `search.json` results |
 
**How we combine them:** Both APIs identify a book by its **ISBN-13**. When Google Books returns a book, we use its ISBN-13 to look up the same book in Open Library and fill in anything missing, such as the cover. Each book is saved once in our `Books` table, keyed by its unique ISBN-13. If a result has no ISBN-13, we use its ISBN-10.
<!--| [Other team's service] | [Their JSON endpoint] | [What we use it for] |-->

---

## Authentication and Authorization

BookNest uses ASP.NET Core Identity with integer user IDs. Identity provides password hashing, login, logout, account management, and role support. Email addresses must be unique.

Reading lists, reading goals, reviews, comments, and likes are associated with the authenticated user's ID. User-owned queries and page handlers must filter by that ID so one user cannot access or change another user's data.

The application supports an `Admin` role for future moderation features. Admin access should be limited to explicit moderation and management pages using role-based authorization such as `[Authorize(Roles = "Admin")]`. The Admin role does not provide unrestricted access to the database.

Database migrations create the Identity tables and application tables. Pending migrations are applied during application startup. Admin role and account provisioning will be handled separately during deployment; no administrator account or password is stored in source code.

---

## Database
 
A small SQLite database, built with Entity Framework Core, stores books, each reader's reading list, reviews, and goals. Ratings and reviews are stored per book, so BookNest can show the average rating from all readers. Over time, this builds BookNest's own collection of reader reviews.
 
**Naming conventions** (standard C# and Entity Framework Core):
 
- Entity classes are singular (`Book`); tables are plural (`Books`).
- Columns are PascalCase. Acronyms longer than two letters are written as words (`Isbn13`, not `ISBN13`).
- Every table has a primary key named `<Entity>Id`, and foreign keys use the same name as the key they point to (`BookId` → `Books.BookId`).
- Date-only columns end in `Date`; date-and-time columns end in `At`.
**Users**
 
| Column | Type | Notes |
|---|---|---|
| UserId | int | Primary key |
| UserName | string | Unique |
| Email | string | Unique |
| CreatedAt | DateTime | |
 
**Books**
 
| Column | Type | Notes |
|---|---|---|
| BookId | int | Primary key |
| Isbn13 | string | Unique; the key shared by Google Books and Open Library |
| Title | string | |
| Author | string | |
| Description | string? | |
| CoverUrl | string? | From Google Books, or from Open Library if Google Books has none |
| GoogleBooksId | string? | |
| OpenLibraryKey | string? | |
 
**ReadingListItems**
 
| Column | Type | Notes |
|---|---|---|
| ReadingListItemId | int | Primary key |
| UserId | int | Foreign key → Users |
| BookId | int | Foreign key → Books |
| Status | ReadingStatus | Enum: WantToRead, Reading, Finished |
| AddedAt | DateTime | |
| FinishedDate | DateOnly? | Set when Status changes to Finished |
 
*(UserId, BookId) is unique, so a reader can't add the same book twice.*
 
**Reviews**
 
| Column | Type | Notes |
|---|---|---|
| ReviewId | int | Primary key |
| BookId | int | Foreign key → Books |
| UserId | int | Foreign key → Users |
| Rating | int | 1–5, required |
| Summary | string? | Short headline, up to 100 characters |
| Comment | string? | Up to 2,000 characters |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime? | Set when the reader edits the review |
 
*(UserId, BookId) is unique, so each reader has one review per book.*
 
**ReadingGoals**
 
| Column | Type | Notes |
|---|---|---|
| ReadingGoalId | int | Primary key |
| UserId | int | Foreign key → Users |
| Year | int | |
| TargetBookCount | int | At least 1 |
 
*(UserId, Year) is unique, so each reader has one goal per year.*
 
**Relationships**
 
- One **User** has many **ReadingListItems**, many **Reviews**, and one **ReadingGoal** per year.
- One **Book** has many **ReadingListItems** and many **Reviews** from different readers.
- A book's average rating is the average of `Reviews.Rating` for that `BookId`.
- A reader's goal progress counts their **ReadingListItems** with `Status = Finished` and a `FinishedDate` in that year.
  
---

## Model Design

The domain model is designed for a multi-user reading application. The detailed model reference is available in [Models/README.md](Models/README.md).

### Entity Responsibilities

- **User**: Identity account, authentication, and role membership through ASP.NET Core Identity.
- **Book**: Shared catalog record identified by a unique `GoogleBooksId`.
- **ReadingList**: One user's reading status and completion date for a book.
- **ReadingGoal**: One user's annual reading target.
- **BookReview**: One user's rating, review, summary, timestamps, and moderation state for a book.
- **ReviewComment**: A user-authored comment on a review with moderation metadata.
- **ReviewLike**: A user's like on a review.

### Relationships and Constraints

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

- `GoogleBooksId` is unique so the shared catalog does not save the same book twice.
- `(UserId, BookId)` is unique in `ReadingList` so a user cannot add a book twice.
- `(UserId, Year)` is the composite key for `ReadingGoal`.
- `(UserId, BookId)` is unique in `BookReview` so each user has one review per book.
- `(UserId, BookReviewId)` is unique in `ReviewLike` so a user can like a review once.
- Ratings are stored per user and aggregated from approved reviews; a single aggregate rating is not stored on `Book`.
- Moderation states are tracked for reviews and comments using `Pending`, `Approved`, `Rejected`, and `Hidden`.

Reviews, comments, and likes are separate from reading progress so community content can be moderated and extended without changing the `ReadingList` record.

---

## Team Composition

### Team Members

| Name | Role |
|---|---|
| Sashaank Suresh | Scrum Master, API integration |
| Pranava Koyyada | UI/UX design, storyboard, testing |
| Greeshma Saroja Tummala | Database, backend |

### Weekly Meeting

**Mondays, 4:00–4:30 PM ET, on Microsoft Teams.** Each member shares what they finished, what's next, and anything blocking them, and then we update the GitHub Project board.
