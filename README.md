<p align="center">
  <img src="images/booknest-logo.svg" alt="BookNest logo" width="150">
</p>

<h1 align="center">BookNest</h1>
<p align="center"><em>Discover books, organize your reading list, and track your reading goals.</em></p>

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

**BookNest** is a simple personal reading tracker. Search for a book, add it to your list, mark where you are with it, and leave a quick rating and review when you finish.

**Features**

- **Search** – Find books by title or author.
- **Reading List** – Mark books as *Want to Read*, *Reading*, or *Finished*.
- **Rate & Review** – Give finished books 1–5 stars and a short review.
- **Reading Goal** – Set how many books you want to read this year and see your progress.

Built with **C# / ASP.NET Core** in **Visual Studio 2026**.

---

## Icon / Logo

<img src="images/booknest-logo.svg" alt="BookNest logo" width="100">

An open book resting in a nest: a cozy home for your reading list.

---

## Storyboard

![BookNest Storyboard](images/storyboard.png)

| Screen | Widgets |
|---|---|
| **Home** | Reading goal progress bar, *Reading* list, link to Search |
| **Search** | Search box, Search button, results (cover, title, author), *Add to List* button |
| **My List** | Books grouped by status, status dropdown, *Remove* button |
| **Book Details** | Cover, title, author, description, star rating, review box, *Save* button |

---

## Projects

Work is tracked on our [Jira Project board](https://mail-team-g68n68ck.atlassian.net/jira/software/projects/BN/boards/1/backlog) with the columns **To Do → In Progress → Done**.

| Sprint | Epic | Goal |
|---|---|---|
| Week 1 | Design, Setup & Book Search | Storyboard and design document first, then repo setup, database, Google Books search, Search page |
| Week 2 | Reading List & Reviews | My List, status, ratings and reviews, Open Library backup |
| Week 3 | Reading Goal, Partner Service & Demo | Reading goal, partner team's service, tests, final demo |

---

## Requirements

### 1. Search for Books

**As a reader, I want to search for books by title or author so that I can find books to add to my list.**

1. **Given** Google Books is available,
   **When** I search for "The Hobbit",
   **Then** I should see "The Hobbit" by J.R.R. Tolkien in the results.

2. **Given** Google Books is available,
   **When** I search for "Andy Weir",
   **Then** I should see "The Martian" in the results.

3. **Given** I am on the Search page,
   **When** I click Search with an empty search box,
   **Then** I should see "Please enter a title or author."

4. **Given** Google Books is available,
   **When** I search for "xqzvplmnt",
   **Then** I should see "No books found."

### 2. Manage My Reading List

**As a reader, I want to mark books as Want to Read, Reading, or Finished so that I can keep track of what I am reading.**

1. **Given** "The Martian" is not on my list,
   **When** I click *Add to List*,
   **Then** "The Martian" should appear on my list as *Want to Read*.

2. **Given** "The Martian" is on my list as *Want to Read*,
   **When** I change its status to *Reading*,
   **Then** "The Martian" should show as *Reading*.

3. **Given** "The Martian" is already on my list,
   **When** I click *Add to List* again,
   **Then** I should see "This book is already on your list." and only one copy should exist.

### 3. Rate and Review Books

**As a reader, I want to rate and review books I finish so that I can remember what I thought of them.**

1. **Given** "The Hobbit" is on my list as *Finished*,
   **When** I select 4 stars, type "Fun adventure," and click Save,
   **Then** "The Hobbit" should show 4 stars and the review "Fun adventure."

2. **Given** "The Hobbit" has a 4-star rating,
   **When** I change it to 5 stars and click Save,
   **Then** "The Hobbit" should show 5 stars.

3. **Given** "The Martian" is on my list as *Want to Read*,
   **When** I open its Book Details page,
   **Then** the rating and review box should be disabled with the message "Finish this book to rate it."

### 4. Set a Reading Goal

**As a reader, I want to set a yearly reading goal so that I can stay motivated to read.**

1. **Given** I have no goal for 2026,
   **When** I set a goal of 12 books,
   **Then** the Home page should show "0 of 12 books read in 2026."

2. **Given** my 2026 goal is 12 books and I have finished 3 books,
   **When** I mark "The Hobbit" as *Finished*,
   **Then** the Home page should show "4 of 12 books read in 2026."

3. **Given** I am setting my reading goal,
   **When** I enter 0 and click Save,
   **Then** I should see "Goal must be at least 1 book."

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

| Source | Example | Used For |
|---|---|---|
| [Google Books API](https://developers.google.com/books/docs/v1/using) | `https://www.googleapis.com/books/v1/volumes?q=the+hobbit` | Main book search (RESTful JSON) |
| [Open Library API](https://openlibrary.org/developers/api) | `https://openlibrary.org/search.json?q=the+hobbit` | Backup search and missing covers (RESTful JSON) |
| [Other team's service] | [Their JSON endpoint] | [What we use it for] |

---

## Database

A small SQLite database stores the reading list and goal.

**Books**

| Field | Type | Notes |
|---|---|---|
| BookId | int | Primary key |
| Title | text | |
| Author | text | |
| CoverUrl | text | |
| GoogleBooksId | text | Unique, so a book is not saved twice |

**ReadingList**

| Field | Type | Notes |
|---|---|---|
| ReadingListId | int | Primary key |
| BookId | int | Foreign key → Books |
| Status | text | WantToRead, Reading, Finished |
| Rating | int | 1–5, nullable |
| Review | text | Nullable |
| FinishedDate | date | Nullable |

**ReadingGoal**

| Field | Type | Notes |
|---|---|---|
| Year | int | Primary key |
| TargetBooks | int | At least 1 |

**Relationships:** Each book has one ReadingList entry. The reading goal counts the ReadingList entries marked *Finished* in that year.

---

## Team Composition

### Team Members

| Name | Role |
|---|---|
| Sashaank Suresh | Scrum Master, API integration |
| Pranava Koyyada | UI/UX design, storyboard, testing |
| Greeshma Saroja Tummala | Database, backend |

### Weekly Meeting

**Sundays, 7:00–7:30 PM ET, on Microsoft Teams.** Each member shares what they finished, what's next, and anything blocking them, and then we update the GitHub Project board.
