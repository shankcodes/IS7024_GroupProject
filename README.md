<p align="center">
  <img src="images/booknest-logo.svg" alt="BookNest logo" width="160">
</p>

<h1 align="center">BookNest</h1>
<p align="center"><em>Discover books, organize your reading list, and track your reading progress.</em></p>

---

## Table of Contents

1. [Introduction](#introduction)
2. [Icon / Logo](#icon--logo)
3. [Storyboard](#storyboard)
4. [Projects](#projects)
5. [Requirements](#requirements)
6. [Data Sources](#data-sources)
7. [Database](#database)
8. [Team Composition](#team-composition)

---

## Introduction

**BookNest** is a personal reading tracker for anyone who wants to read more and remember what they read. Search millions of titles, save them to your shelves, log your progress page by page, and set a yearly goal to stay motivated.

**Features**

- 🔍 **Book Search** – Find books by title, author, or ISBN using Google Books and Open Library.
- 📚 **Reading Status** – Sort books into *Want to Read*, *Currently Reading*, and *Finished* shelves.
- 📈 **Progress Tracking** – Log your current page and see your percent complete.
- ⭐ **Ratings** – Rate finished books from 1 to 5 stars.
- 📝 **Reviews** – Write and edit personal reviews of the books you read.
- 🎯 **Reading Goals** – Set a yearly book goal and watch your progress bar fill up.

BookNest is built with **C# / ASP.NET Core** in **Visual Studio 2026**.

---

## Icon / Logo

<img src="images/booknest-logo.svg" alt="BookNest logo" width="120">

An open book resting in a nest, representing a cozy home for your reading life.

---

## Storyboard

The storyboard shows the planned screens and their widgets. *(Storyboard image: `images/storyboard.png`, created in Figma.)*

<!-- Replace this line with the image once exported from Figma: -->
<!-- ![BookNest Storyboard](images/storyboard.png) -->

| Screen | Widgets |
|---|---|
| **Home / Dashboard** | Navigation bar, yearly goal progress bar, *Currently Reading* carousel, *Recently Finished* list |
| **Search** | Search box, search-by dropdown (Title / Author / ISBN), Search button, results list (cover, title, author, year), *Add to Shelf* button |
| **Book Details** | Cover image, title, author(s), description, page count, publish date, status dropdown, star rating, *Write Review* button |
| **My Shelves** | Tabs (*Want to Read* / *Currently Reading* / *Finished*), book cards, *Remove* button |
| **Update Progress** | Current page input, total pages label, percent-complete bar, *Save* button |
| **Review Editor** | Star rating selector, review text area (2,000-character limit with counter), *Save* and *Delete* buttons |
| **Reading Goals** | Goal year, target number input, books-finished count, progress bar, *Save Goal* button |

---

## Projects

Work is planned and tracked with **GitHub Projects** in this repository: [BookNest Project Board](../../projects)

Board columns: **Backlog → To Do → In Progress → In Review → Done**

| Sprint | Example Tasks |
|---|---|
| Sprint 1 – Setup | Create repo and VS 2026 solution, all members clone, write README design document, create logo and storyboard |
| Sprint 2 – Search | Google Books API client, Open Library API client, search page UI, unit tests for search |
| Sprint 3 – Shelves | Database schema and migrations, reading status shelves, progress tracking |
| Sprint 4 – Ratings & Reviews | Star ratings, review editor, validation |
| Sprint 5 – Goals & Polish | Reading goals, dashboard, bug fixes, final demo |

---

## Requirements

### 1. Book Search

**As a reader, I want to search for books by title, author, or ISBN so that I can find books to add to my reading list.**

#### Examples

1. **Given** the Google Books API is available
   **When** I search by title for "The Hobbit"
   **Then** I should see "The Hobbit" by J.R.R. Tolkien in the results.

2. **Given** the Google Books API is available
   **When** I search by ISBN for "9780441172719"
   **Then** I should see exactly one result: "Dune" by Frank Herbert.

3. **Given** the Google Books API is available
   **When** I search by author for "Andy Weir"
   **Then** I should see "Project Hail Mary" and "The Martian" in the results.

4. **Given** the Google Books API is available
   **When** I search for "xqzvplmnt"
   **Then** I should see the message "No books found. Try a different search."

5. **Given** I am on the Search page
   **When** I click Search with an empty search box
   **Then** I should see the message "Please enter a title, author, or ISBN." and no API call should be made.

6. **Given** the Google Books API is unavailable (returns HTTP 503)
   **When** I search by title for "The Hobbit"
   **Then** BookNest should query the Open Library API instead and show "The Hobbit" by J.R.R. Tolkien in the results.

### 2. Reading Status

**As a reader, I want to put books on Want to Read, Currently Reading, and Finished shelves so that I can organize what I plan to read, am reading, and have read.**

#### Examples

1. **Given** "Dune" by Frank Herbert is not on any of my shelves
   **When** I click *Add to Shelf* and choose "Want to Read"
   **Then** "Dune" should appear on my *Want to Read* shelf.

2. **Given** "Dune" is on my *Want to Read* shelf
   **When** I change its status to "Currently Reading"
   **Then** "Dune" should appear on my *Currently Reading* shelf, no longer appear on *Want to Read*, and its start date should be set to today.

3. **Given** "Dune" is on my *Currently Reading* shelf
   **When** I change its status to "Finished"
   **Then** "Dune" should appear on my *Finished* shelf and its finish date should be set to today.

4. **Given** "Dune" is already on my *Want to Read* shelf
   **When** I search for "Dune" and click *Add to Shelf* again
   **Then** I should see the message "Dune is already on your Want to Read shelf." and no duplicate should be added.

5. **Given** "Dune" is on my *Finished* shelf
   **When** I click *Remove* and confirm
   **Then** "Dune" should no longer appear on any of my shelves.

### 3. Progress Tracking

**As a reader, I want to log the page I am on so that I can see how far I am through a book.**

#### Examples

1. **Given** "The Hobbit" (300 pages) is on my *Currently Reading* shelf
   **When** I enter 150 as my current page and click Save
   **Then** I should see "150 / 300 pages (50%)".

2. **Given** "The Hobbit" (300 pages) is on my *Currently Reading* shelf
   **When** I enter 300 as my current page and click Save
   **Then** I should see "100% complete" and be asked "Mark The Hobbit as Finished?"

3. **Given** "The Hobbit" (300 pages) is on my *Currently Reading* shelf
   **When** I enter 350 as my current page
   **Then** I should see "Page cannot be greater than 300." and my progress should not change.

4. **Given** "The Hobbit" is on my *Currently Reading* shelf
   **When** I enter -5 as my current page
   **Then** I should see "Page must be 0 or greater." and my progress should not change.

5. **Given** a book on my *Currently Reading* shelf has no page count from either API
   **When** I open Update Progress
   **Then** I should be asked to enter the total page count before logging progress.

### 4. Ratings

**As a reader, I want to rate books from 1 to 5 stars so that I can remember which books I enjoyed most.**

#### Examples

1. **Given** "Project Hail Mary" is on my *Finished* shelf and has no rating
   **When** I select 5 stars
   **Then** "Project Hail Mary" should show 5 stars on my *Finished* shelf.

2. **Given** I rated "Project Hail Mary" 5 stars
   **When** I change the rating to 4 stars
   **Then** "Project Hail Mary" should show 4 stars, and only one rating should be saved.

3. **Given** "Dune" is on my *Want to Read* shelf
   **When** I open its Book Details page
   **Then** the star rating selector should be disabled with the hint "Finish this book to rate it."

4. **Given** an API request to save a rating of 0 or 6 for "Project Hail Mary"
   **When** the request is processed
   **Then** it should be rejected with the error "Rating must be between 1 and 5." and the existing rating should not change.

### 5. Reviews

**As a reader, I want to write a review of a book so that I can record my thoughts about it.**

#### Examples

1. **Given** "The Hobbit" is on my *Finished* shelf
   **When** I write "A charming adventure with a great ending." and click Save
   **Then** that review should appear on the Book Details page for "The Hobbit".

2. **Given** I have a review saved for "The Hobbit"
   **When** I edit the text to "A charming adventure. Smaug steals the show." and click Save
   **Then** the updated text should appear and the "Last edited" date should be today.

3. **Given** I am writing a review for "The Hobbit"
   **When** I click Save with an empty text area
   **Then** I should see "Review cannot be empty." and nothing should be saved.

4. **Given** I am writing a review for "The Hobbit"
   **When** I type 2,001 characters
   **Then** the counter should turn red, show "2001 / 2000", and the Save button should be disabled.

5. **Given** I have a review saved for "The Hobbit"
   **When** I click Delete and confirm
   **Then** the review should be removed, but my star rating for "The Hobbit" should remain.

### 6. Reading Goals

**As a reader, I want to set a yearly reading goal so that I can stay motivated to read more.**

#### Examples

1. **Given** I have no reading goal for 2026
   **When** I set a goal of 24 books for 2026
   **Then** the dashboard should show "0 of 24 books read in 2026".

2. **Given** my 2026 goal is 24 books and I have finished 11 books in 2026
   **When** I mark "Dune" as Finished on October 25, 2026
   **Then** the dashboard should show "12 of 24 books read in 2026 (50%)".

3. **Given** my 2026 goal is 12 books and I have finished 11 books in 2026
   **When** I mark my 12th book as Finished
   **Then** I should see "🎉 You reached your 2026 goal!"

4. **Given** I am on the Reading Goals page
   **When** I enter 0 as my goal and click Save Goal
   **Then** I should see "Goal must be at least 1 book." and no goal should be saved.

5. **Given** my 2026 goal is 24 books
   **When** I finish a book whose finish date is December 31, 2025
   **Then** it should count toward 2025, and my 2026 count should not change.

---

## Data Sources

| Source | Endpoint Example | Format | Used For |
|---|---|---|---|
| [Google Books API](https://developers.google.com/books/docs/v1/using) | `https://www.googleapis.com/books/v1/volumes?q=intitle:the+hobbit` | RESTful JSON | Primary search, cover images, descriptions, page counts |
| [Open Library API](https://openlibrary.org/developers/api) | `https://openlibrary.org/search.json?q=the+hobbit` | RESTful JSON | Fallback search, ISBN lookup (`/isbn/9780441172719.json`), missing page counts and covers |

Results from both sources are matched by ISBN-13 so each book is stored only once.

---

## Database

BookNest stores user data in a SQL Server database (via Entity Framework Core). Book metadata is cached locally so shelves load without calling the APIs every time.

### Tables

**Users**

| Field | Type | Notes |
|---|---|---|
| UserId | int | Primary key |
| Username | nvarchar(50) | Unique |
| Email | nvarchar(100) | Unique |
| PasswordHash | nvarchar(256) | |
| CreatedAt | datetime | |

**Books**

| Field | Type | Notes |
|---|---|---|
| BookId | int | Primary key |
| Isbn13 | nvarchar(13) | Unique |
| GoogleBooksId | nvarchar(20) | Nullable |
| OpenLibraryId | nvarchar(20) | Nullable |
| Title | nvarchar(300) | |
| Authors | nvarchar(300) | |
| PageCount | int | Nullable |
| PublishedDate | nvarchar(20) | |
| CoverUrl | nvarchar(500) | |
| Description | nvarchar(max) | |

**UserBooks** (a user's shelf entry)

| Field | Type | Notes |
|---|---|---|
| UserBookId | int | Primary key |
| UserId | int | Foreign key → Users |
| BookId | int | Foreign key → Books |
| Status | nvarchar(20) | WantToRead, CurrentlyReading, Finished |
| CurrentPage | int | Default 0 |
| StartDate | date | Nullable |
| FinishDate | date | Nullable |
| Rating | tinyint | Nullable, 1–5 |

*(UserId, BookId) is unique, so a book can only be on one of a user's shelves.*

**Reviews**

| Field | Type | Notes |
|---|---|---|
| ReviewId | int | Primary key |
| UserBookId | int | Foreign key → UserBooks, unique |
| ReviewText | nvarchar(2000) | |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

**ReadingGoals**

| Field | Type | Notes |
|---|---|---|
| GoalId | int | Primary key |
| UserId | int | Foreign key → Users |
| Year | int | |
| TargetBooks | int | Must be ≥ 1 |

*(UserId, Year) is unique.*

### Relationships

- One **User** has many **UserBooks**.
- One **Book** has many **UserBooks** (many users can shelve the same book).
- **Users ↔ Books** is many-to-many through **UserBooks**.
- One **UserBook** has zero or one **Review**.
- One **User** has many **ReadingGoals** (one per year).
- Books finished in a year are counted from `UserBooks.FinishDate`.

---

## Team Composition

### Team Members

| Name | Role |
|---|---|
| [Member 1] | Scrum Master / Product Owner |
| [Member 2] | Backend Developer (API integration) |
| [Member 3] | Database Developer |
| [Member 4] | UI/UX Designer & Front-end Developer |
| [Member 5] | QA / Unit Testing |

### Weekly Meeting

- **Time:** Every Sunday, 7:00 PM – 7:30 PM (Eastern Time)
- **Format:** Online standup on Microsoft Teams
- **Agenda:** Each member answers: What did I finish? What will I do next? Is anything blocking me? Then the team updates the GitHub Project board.
- **Between meetings:** Async updates in the team group chat.
