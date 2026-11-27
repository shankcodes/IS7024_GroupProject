# Research: BN-11 Rate and Review Finished Books

**Date**: 2026-11-27
**Scope**: Record BN-11 design decisions and the implementation follow-up that replaced rating-row materialization with a database aggregate.

## Decisions

### Reuse the existing review record and reader/book uniqueness rule

- **Decision**: Save the rating, summary, and comment on the existing `BookReview` entity. The existing unique index on `(UserId, BookId)` enforces one review per reader and book.
- **Rationale**: This supports create and update behavior without duplicating rating data or changing the database schema.
- **Alternatives considered**: A separate rating table or a new review entity was rejected because the existing record already stores the required fields and enforces the needed uniqueness.

### Require the reader's own Finished status on every save

- **Decision**: Determine eligibility from the current reader's `ReadingList` entry and enforce it in the server-side save handler. The UI also disables the form and displays the required message before the book is Finished.
- **Rationale**: UI state alone is not an authorization boundary; server-side validation prevents an invalid POST from saving a review.
- **Alternatives considered**: Allowing any signed-in reader to rate any book was rejected because BN-11 explicitly limits rating to finished books. Relying only on disabled controls was rejected because a crafted request could bypass the page.

### Keep review text private and show the shared aggregate

- **Decision**: Prefill the signed-in reader's own review on Book Details and show the aggregate average/count across readers. Do not display another reader's summary or comment in this story.
- **Rationale**: BN-11's acceptance criteria require the average and count but do not require a public review feed. The reader's own summary and comment remain available for editing.
- **Alternatives considered**: A public list of all reader comments was left out of scope because it introduces a separate visibility and moderation experience.

### Preserve bounded input and rating rules

- **Decision**: Accept integer ratings from 1 through 5, optional summaries up to 200 characters, and optional comments up to 4,000 characters; reject out-of-range or over-limit submissions.
- **Rationale**: These bounds match the existing form model and the scenarios exercised for BN-11.
- **Alternatives considered**: Unbounded review text and fractional star ratings are not part of the story and are not supported by the current form.

### Calculate the average and count from all submitted ratings

- **Decision**: Use a database-side count and sum of every non-null rating for the selected book, round the arithmetic mean to one decimal place, and display the exact count with singular/plural wording.
- **Rationale**: This satisfies the example `Average rating: 4.5 (2 ratings).` and the one-rating UI polish requirement without loading every rating into application memory.
- **Alternatives considered**: Averaging only the current reader's rating or including unrated review records in the count would not represent the stated all-reader aggregate.

### Reuse existing app boundaries and validation tools

- **Decision**: Keep the experience in the existing Razor Pages application and validate with the current xUnit/SQLite test project plus browser checks.
- **Rationale**: The repository already provides Identity, the EF Core context, review persistence, responsive site styles, and page-model testing patterns.
- **Alternatives considered**: A separate API, new package, new database table, or migration is unnecessary for the implemented story.

## Resolved Questions

- **Does saving again create another review?** No. It updates the existing reader/book record.
- **Can a reader review before finishing?** No. The form is disabled and the server handler rejects the save.
- **Can a reader see another reader's comment?** Not in BN-11. The shared feedback is the average and count.
- **Is a database migration required?** No. The existing entity, unique index, and database schema support the story.
- **Is a Google Books API key needed for review behavior?** No. Book search may need separate external API configuration, but it is not a dependency of rating or review.
- **Are the Reading List, navbar, and login visual changes part of BN-11?** No. They are a separate UI spike on the same branch and are excluded from the Jira card's requirements.
