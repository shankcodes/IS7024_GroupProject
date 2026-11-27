# Feature Specification: Rate and Review Finished Books

**Feature Branch**: `BN-11-book-reviews-ui-enhancements`
**Created**: 2026-11-27
**Status**: Implemented
**Input**: Jira story BN-11: As a reader, I want to rate and review a finished book and see the average rating from all readers so that I can remember what I thought of it and see what others think.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Rate and review a finished book (Priority: P1)

As a reader, I want to save a star rating, summary, and comment for a finished book so I can remember my opinion.

**Why this priority**: Saving a personal review is the central value of the story.

**Independent Test**: Sign in, mark The Hobbit as Finished, save a 4-star rating with the specified summary and comment, then reopen its details and verify the saved values.

**Acceptance Scenarios**:

1. **Given** I am signed in, The Hobbit is on my list as Finished, and I have not reviewed it, **When** I select 4 stars, enter the summary "Fun adventure" and the comment "Bilbo's journey kept me hooked", and save, **Then** my rating and review are saved for The Hobbit.
2. **Given** I have rated The Hobbit 4 stars, **When** I change my rating to 5 stars and save, **Then** my rating is 5 stars and I still have only one review for the book.
3. **Given** the review form is enabled, **When** I use the keyboard to select 3 stars, **Then** 3 stars is selected and the rating choice has an accessible name.
4. **Given** the review form is enabled, **When** I submit a summary of 200 characters and a comment of 4,000 characters, **Then** both are saved; text beyond either limit is rejected.

---

### User Story 2 - See the average rating (Priority: P1)

As a reader, I want to see the average rating and number of ratings for a book so I can understand its overall reception.

**Why this priority**: The aggregate lets readers compare opinions without exposing another reader's personal review text.

**Independent Test**: Have two readers rate The Hobbit 4 and 5 stars, open its details, and verify the displayed average and count.

**Acceptance Scenarios**:

1. **Given** Alex rated The Hobbit 4 stars and Sam rated it 5 stars, **When** I open its Book Details page, **Then** I see "Average rating: 4.5 (2 ratings)."
2. **Given** another reader has saved a summary and comment, **When** I open the book's details, **Then** I see the shared average and count but not that reader's summary or comment.

---

### User Story 3 - Rate only after finishing (Priority: P1)

As a reader, I want rating controls unavailable until I finish a book so my ratings reflect completed reading.

**Why this priority**: The finished-book restriction is an explicit business rule for submitting a rating or review.

**Independent Test**: Put The Martian on my list as Want to Read, open its details, and verify that the rating and review controls are disabled with the required explanation.

**Acceptance Scenarios**:

1. **Given** The Martian is on my list as Want to Read, **When** I open its Book Details page, **Then** my rating and review controls are disabled and I see "Finish this book to rate it."

### Edge Cases

- An unauthenticated reader cannot save a rating or review.
- A book has no ratings yet; its details still show an average and a zero rating count.
- A rating outside the 1–5 range or text beyond the supported length is rejected without creating or changing a review.
- A reader changes the summary or comment while keeping the same rating; the saved review remains a single review for that reader and book.
- The rating count is one; the label uses the singular word "rating."
- A book is Reading or Want to Read; the reader cannot submit a review until its status is Finished.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow a signed-in reader to save a rating and review only for a book that reader has marked Finished.
- **FR-002**: A saved rating MUST be an integer from 1 through 5 stars.
- **FR-003**: A review MUST support a summary of up to 200 characters and a comment of up to 4,000 characters; either text field may be left blank.
- **FR-004**: The system MUST keep at most one review per reader for a given book. Saving again MUST update that reader's existing review rather than create another.
- **FR-005**: The reader MUST be able to see and update their own saved rating, summary, and comment.
- **FR-006**: Book Details MUST show the average of all submitted ratings, rounded to one decimal place, together with the exact number of ratings.
- **FR-007**: The rating-count label MUST use "rating" for one rating and "ratings" for every other count.
- **FR-008**: When the reader has not marked the book Finished, the rating and review controls MUST be disabled and the page MUST show "Finish this book to rate it."
- **FR-009**: A reader MUST NOT see another reader's summary or comment as part of this story; other readers' opinions are represented by the shared average and count.
- **FR-010**: The star-rating control MUST identify each value to assistive technology and remain usable with keyboard navigation.

### Key Entities

- **Reader**: A signed-in person who may create or update one review per book.
- **Book**: A title that readers can mark Finished and review.
- **Review**: A reader's 1–5 star rating, optional summary, and optional comment for one book.
- **Reading status**: The reader-specific state that determines whether rating and review controls are available.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of valid review submissions persist the selected rating, summary, and comment and are visible to the same reader on the next visit.
- **SC-002**: Repeated saves by the same reader for the same book leave exactly one review for that reader and book.
- **SC-003**: The displayed average matches the arithmetic mean of all submitted ratings rounded to one decimal place, and the displayed count matches the number of submitted ratings.
- **SC-004**: All books not marked Finished have disabled rating and review controls and display the required explanation.
- **SC-005**: A one-rating result uses the singular label, and a two-rating result uses the plural label.

## Assumptions

- A reader must be signed in to create or update a review.
- Summary and comment are optional, subject to the stated maximum lengths.
- Reviews are private to their author in this story; a public list of other readers' review text is outside this scope.
- If a book has no ratings, the average is displayed as 0.0 with a count of zero.

## Out of Scope

- The Reading List card redesign, navbar styling/active-page underline, and login/Identity restyling were a separate UI spike on the same branch. They are not part of the BN-11 Jira card, acceptance criteria, or this feature's implementation tasks.
