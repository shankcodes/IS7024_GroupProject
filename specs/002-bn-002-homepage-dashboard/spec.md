# Feature Specification: BN-002 Homepage Dashboard

**Feature Branch**: `002-bn-002-homepage-dashboard`

**Created**: 2026-11-16

**Status**: Draft

**Input**: User description: "BN-7 Redesign the BookNest homepage. Use the next available feature number and name \"bn-xx-homepage-dashboard\". As a reader, I want an attractive homepage where I can search books, see Currently Reading, track my yearly reading goal, and find books to read next. Follow the README storyboard and acceptance criteria. Reuse existing services and models. Preserve working search functionality. Do not invent progress or book data. Include mobile responsiveness, signed-out behavior, and empty states."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Find a book from Home (Priority: P1)

As a reader, I want the homepage search to remain prominent and work as it does today so that I can find books and use any existing Add to List functionality without losing a familiar flow.

This story preserves and links to Add to List functionality only if it is already implemented. It does not add that feature. If Add to List is not implemented, Home provides navigation to existing BookNest features and does not display a nonfunctional Add to List button.

**Why this priority**: Book search is the primary homepage action in the README storyboard and an existing working capability that must not regress.

**Independent Test**: From Home, search for a known title and a known author, then verify the matching results and existing validation and empty-result behavior.

**Acceptance Scenarios**:

1. **Given** book search is available and contains "The Hobbit" by J.R.R. Tolkien, **When** I search for "The Hobbit" on Home, **Then** the results include "The Hobbit" by J.R.R. Tolkien.
2. **Given** book search is available and contains "The Martian" and "Project Hail Mary" by Andy Weir, **When** I search for "Andy Weir", **Then** both books appear in the results.
3. **Given** the Home search field is empty, **When** I submit the search, **Then** I see "Please enter a title or author." and no search request is made.
4. **Given** no book matches "xqzvplmnt", **When** I search for it, **Then** I see "No books found."
5. **Given** a search result has a null, empty, or whitespace-only cover URL, a malformed URL, or a URL that is not an absolute HTTP or HTTPS URL, **When** I view the result, **Then** the existing `/images/no-cover.svg` placeholder is shown.
6. **Given** a book-card cover image URL is valid but the image fails to load in the browser, **When** the image load fails, **Then** the card shows `/images/no-cover.svg` and does not retry the failed image or repeatedly handle a failure of the placeholder.
7. **Given** the Home search field contains a genre- or review-related keyword, **When** I submit it, **Then** the query is passed unchanged through the existing Google Books search flow and any results returned by that flow use the existing result presentation; this story does not guarantee genre filtering or searching within review text.
8. **Given** the search service is unavailable, **When** I search for a book, **Then** I see the existing service-unavailable feedback.
9. **Given** I am signed out, **When** I search for a book, **Then** search and its results remain available, while actions requiring a reader account invite me to sign in.

This homepage story preserves the existing Google Books search behavior. It does not introduce a new search algorithm, genre filter, or review-text search.

---

### User Story 2 - Review my reading dashboard (Priority: P1)

As a signed-in reader, I want to see books I am currently reading and books I have finished recently so that I can quickly understand my real reading activity.

**Why this priority**: The dashboard should make a reader's existing list and recent activity immediately useful without fabricating or altering records.

**Independent Test**: Use an account with known Reading and Finished entries, then verify that Home shows only those entries in the appropriate sections and that an account with no entries sees the relevant empty states.

**Acceptance Scenarios**:

1. **Given** I am signed in and have books with Reading status, **When** I open Home, **Then** the Currently Reading section shows those books with their actual titles and covers, using `/images/no-cover.svg` for any missing or invalid cover image.
2. **Given** I am signed in and have no books with Reading status, **When** I open Home, **Then** Currently Reading shows an empty state that invites me to search for a book.
3. **Given** I am signed in and have books marked Finished with completion dates in the current year, **When** I open Home, **Then** the recent reading section shows those books grouped by their actual completion month and identifies the current year.
4. **Given** I have no books finished in the current year, **When** I view the recent reading section, **Then** I see an empty state and no sample or inferred book data.
5. **Given** a Finished book has no completion date, **When** I view recent reading activity, **Then** that book is not assigned to a month or year.
6. **Given** I am signed out, **When** I open Home, **Then** personal reading activity is not displayed and the dashboard offers a sign-in path instead.

---

### User Story 3 - Track my yearly goal (Priority: P2)

As a signed-in reader, I want to see this year's reading goal and progress so that I can stay motivated using my actual completed books.

**Why this priority**: Goal progress is a key storyboard widget and gives readers useful context at a glance, but it depends on an existing goal or completion history.

**Independent Test**: Set an annual goal and add known finished books with dates in that year; verify the displayed target and count, then verify the no-goal and no-completions states.

**Acceptance Scenarios**:

1. **Given** I am signed in, have a goal of 12 for the current year, and have finished 3 books in that year, **When** I open Home, **Then** I see "3 of 12 books read in [current year]" and progress based on those completions.
2. **Given** I am signed in and have no goal for the current year, **When** I open Home, **Then** I see that no goal is set and a clear path to set one; no target or progress is made up.
3. **Given** I am signed in, have no completed books in the current year, and have a goal of 12, **When** I open Home, **Then** I see "0 of 12 books read in [current year]."
4. **Given** I am signed in, have a 2026 goal of 12 and 3 books finished in 2026, **When** I mark another book Finished on November 1, 2026, **Then** Home shows "4 of 12 books read in 2026."
5. **Given** I am signed in and editing my reading goal, **When** I enter 0 and save, **Then** I see "Goal must be at least 1 book." and my saved goal remains unchanged.
6. **Given** I am signed out, **When** I view the goal area, **Then** I see a sign-in prompt instead of another reader's goal or progress.

---

### User Story 4 - Choose a book to read next (Priority: P2)

As a signed-in reader, I want to see books I have already marked Want to Read so that I can choose what to read next.

**Why this priority**: The README storyboard calls for a Read Next area; using saved list entries makes it immediately actionable without inventing recommendations or book records.

**Independent Test**: Add known books to Want to Read and verify they appear under Read Next; then verify the empty state when there are no such entries.

**Acceptance Scenarios**:

1. **Given** I am signed in and have books marked Want to Read, **When** I open Home, **Then** Read Next shows those saved books with their actual covers or `/images/no-cover.svg` for any missing or invalid cover image, and links only to existing list or book actions.
2. **Given** I am signed in and have no books marked Want to Read, **When** I open Home, **Then** Read Next shows an empty state with a path to search for books.
3. **Given** I am signed out, **When** I open Home, **Then** personal Want to Read entries are not shown and the section provides a sign-in path.

### Edge Cases

- A reader has no books, no goal, or only some of the dashboard data; each section independently shows an appropriate empty state without suppressing available sections.
- A finished book has no completion date; it is not assigned to a month or year based on an assumed date.
- The current year changes; the goal and its completion count use the new current year and do not carry progress forward from a prior year.
- A homepage book card has no valid cover image when its `CoverUrl` is null, empty, or whitespace-only; is malformed; or is not a valid absolute URL using HTTP or HTTPS. Show the existing `/images/no-cover.svg` placeholder consistently for Google Books search results, Currently Reading, Read Next, recent-finished books, and any other homepage book cards.
- A valid cover URL can still fail to load in the browser. Replace it with `/images/no-cover.svg` once; disable or remove the image error handler before changing the source so a failed placeholder cannot cause a retry loop.
- If Add to List is not implemented, do not show an inert Add to List button; provide navigation only to existing BookNest features.
- Search returns an unavailable-service or error result; retain the existing reader-facing error behavior and do not replace it with a success or no-results state.
- A narrow viewport is used; content remains readable and usable without horizontal page scrolling or overlapping controls.
- A signed-out visitor tries an action that requires a personal reading list or goal; require sign-in and do not expose personal data.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Home MUST present search as a primary action and preserve the existing Google Books behavior for title and author searches, query submission, result rendering, validation, no-result feedback, missing/invalid-cover fallback, and service-unavailable feedback.
- **FR-002**: Home MUST pass title, author, genre-related, and review-related keywords through the existing search flow unchanged. It MUST NOT add a new search algorithm, specialized genre filtering, or search within review text; matching results depend on the existing Google Books service behavior.
- **FR-003**: For a signed-in reader, Home MUST show the reader's actual books with Reading status in Currently Reading.
- **FR-004**: For a signed-in reader, Home MUST show the reader's actual Want to Read books in Read Next; it MUST NOT present invented or unsourced book recommendations.
- **FR-005**: Home MUST show books actually finished in the current year in recent reading activity, grouped by their recorded completion month; it MUST NOT assign an entry without a completion date to a month or year.
- **FR-006**: For a signed-in reader, Home MUST show the current year, the saved annual reading target when present, and the count of that reader's books actually finished in that year. When a goal exists, Home MUST show the progress percentage calculated from finished count divided by target, rounded to the nearest whole percentage point (halfway values round up) and capped at 100%, with a progress bar reflecting that percentage. If no goal exists, it MUST offer a path to set one without fabricating a target or progress.
- **FR-007**: Home MUST NOT create, infer, or display fabricated books, goals, completion dates, reading counts, progress, covers, or ratings. A missing value MUST be presented as unavailable or through the applicable empty state.
- **FR-008**: A signed-out visitor MUST be able to use book search. Personal reading-list, reading-goal, completion, and progress data MUST remain unavailable to that visitor; actions requiring an account MUST offer sign-in.
- **FR-009**: Each dashboard section MUST provide its own helpful empty state when the signed-in reader has no relevant data, and an empty section MUST NOT hide other sections or search.
- **FR-010**: Home MUST remain usable on mobile-sized screens: its content and controls MUST fit the viewport without horizontal scrolling, clipping, or overlap, and sections MUST retain a clear reading order.
- **FR-011**: Every homepage book card, including Google Books search results, Currently Reading, Read Next, recent-finished books, and any other homepage book card, MUST show the existing `/images/no-cover.svg` placeholder when `CoverUrl` is null, empty, or whitespace-only; malformed; or not a valid absolute HTTP or HTTPS URL. If a valid cover image fails to load in the browser, Home MUST replace it with the placeholder once and disable or remove its error handler before changing the image source to prevent retry loops.
- **FR-012**: Reading data shown on Home MUST belong only to the currently signed-in reader.
- **FR-013**: Home MUST preserve and link to Add to List functionality only if it already exists. This story MUST NOT implement Add to List; if it is not implemented, Home MUST show navigation only to existing features and MUST NOT display a nonfunctional Add to List button.

### Key Entities *(include if feature involves data)*

- **Book**: A catalog book with available title, author, and optional cover information, referenced by reader activity.
- **Reading-list entry**: A reader's book and its status (Want to Read, Reading, or Finished), with an optional actual completion date.
- **Reading goal**: A reader's target book count for a particular year.
- **Reader**: The signed-in account that owns its reading-list entries and yearly goals.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All nine acceptance scenarios in User Story 1 preserve the existing search behavior, including forwarding title, author, genre-related, and review-related queries unchanged, empty-query validation without a service call, existing no-results and service-unavailable feedback, and rendered `/images/no-cover.svg` behavior for missing or invalid search-result covers; keyword searches are verified by their actual supported behavior rather than assumed matching results.
- **SC-002**: In test accounts with known Reading, Want to Read, and Finished entries, every displayed dashboard book and completion month/year matches the saved data, with no undated entry assigned an invented completion date, and every homepage book card renders `/images/no-cover.svg` for a missing or invalid cover URL or a browser image-loading failure.
- **SC-003**: For accounts with a current-year goal, Home displays the current year, saved target, actual finished-book count, and a whole-number progress percentage rounded with halfway values up and corresponding bar capped at 100%; accounts without a goal see a set-goal path and no target or progress. Saving a goal below 1 preserves the existing goal and displays the README validation message.
- **SC-004**: Each personal section shows its defined empty or signed-out state when data is absent or the visitor is unauthenticated, while search remains available.
- **SC-005**: At viewport widths of 320 pixels and wider, homepage content has no horizontal page scroll, clipped primary controls, or overlapping sections.
- **SC-006**: A reader can locate the search field, goal summary, Currently Reading, and Read Next sections on Home without navigating to another page.

## Assumptions

- The existing BookNest search behavior and search result service remain the source of search results; the redesign does not replace or weaken them.
- The existing reader-owned list statuses and yearly goals are the source of dashboard state. Finished books count toward a year only when their actual completion date is within that year.
- Read Next is populated from the signed-in reader's saved Want to Read books because the README and request provide no recommendation source or recommendation data.
- The README storyboard's recently read month/year content is represented by Finished entries completed in the current year, grouped by their recorded completion month; the page does not infer a period for entries without dates.
- Signed-out visitors can access the existing public search experience but cannot see personal reader data.
- The current calendar year determines which annual goal is summarized, and the existing minimum-goal validation remains in effect.
