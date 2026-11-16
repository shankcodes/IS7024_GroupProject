# Feature Specification: Open Library Cover Fallback

**Feature Branch**: `012-bn-12-open-library-covers`
**Created**: 2026-10-27
**Status**: Draft
**Input**: Jira story BN-12: As a reader, I want BookNest to match each book in Open Library by its ISBN so that I can still see covers when Google Books is missing one.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Show a fallback cover (Priority: P1)

As a reader, I want a book result without a Google Books cover to display the matching Open Library cover so that I can identify the book visually.

**Why this priority**: Cover images are the primary value of this feature and directly improve search-result usefulness.

**Independent Test**: Search for Dune and verify that the single Dune result displays the Open Library cover associated with ISBN 9780441172719 when Google Books has no cover.

**Acceptance Scenarios**:

1. **Given** Google Books returns Dune by Frank Herbert with ISBN 9780441172719 and no cover, and Open Library has a cover for that ISBN, **When** the reader searches for Dune, **Then** exactly one Dune result displays the Open Library cover.

---

### User Story 2 - Preserve a usable result when no cover exists (Priority: P1)

As a reader, I want a consistent placeholder when neither source has a cover for an included book so that missing imagery does not make the search result unusable.

**Why this priority**: Search results must remain complete and visually consistent even when external metadata is incomplete.

**Independent Test**: Load a result with no cover from either source and verify that the result remains visible with `/images/no-cover.svg`.

**Acceptance Scenarios**:

1. **Given** a result has an ISBN and neither Google Books nor Open Library has a cover, **When** the results load, **Then** its image displays `/images/no-cover.svg`.

---

### User Story 3 - Support the available ISBN identifier (Priority: P2)

As a reader, I want Open Library matching to use the identifier available for the result so that books with only an ISBN-10 can still receive a cover.

**Why this priority**: Supporting both common ISBN formats prevents avoidable missing covers while preserving the existing result data.

**Independent Test**: Load a Google Books result that has no ISBN-13 but has an ISBN-10, then verify that the Open Library lookup uses that ISBN-10.

**Acceptance Scenarios**:

1. **Given** a Google Books result has only an ISBN-10, **When** BookNest looks up its cover in Open Library, **Then** the lookup uses the ISBN-10.

---

### User Story 4 - Keep search available during fallback failure (Priority: P1)

As a reader, I want Google Books results to remain visible when Open Library is unavailable so that a secondary service outage does not block searching.

**Why this priority**: The fallback must improve results without making the primary search less reliable.

**Independent Test**: Simulate an unavailable Open Library service and verify that Google Books results still render with the placeholder where no Google Books cover exists.

**Acceptance Scenarios**:

1. **Given** Open Library is unavailable, **When** the reader searches for Dune, **Then** Google Books results still appear and results without a cover display `/images/no-cover.svg`.

### Edge Cases

- A result has neither an ISBN-13 nor an ISBN-10; it is excluded by the existing `MapToSearchItem` rule because books are keyed by ISBN.
- The cover-selection helper receives no ISBN; it defensively returns `/images/no-cover.svg` even though normal mapping excludes no-ISBN results.
- Open Library returns no cover for a valid ISBN; it uses the placeholder cover.
- Open Library returns an error, times out, or cannot be reached; the Google Books result remains visible.
- Google Books already provides a cover; that cover is retained and no fallback cover replaces it.
- A result contains both ISBN-13 and ISBN-10; matching uses ISBN-13 first.
- Multiple matching results are returned; each result is evaluated independently and remains distinct.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST retain a Google Books cover when one is available.
- **FR-002**: The system MUST use the book's ISBN-13 to find a fallback cover when Google Books has no cover.
- **FR-003**: When ISBN-13 is unavailable, the system MUST use the book's ISBN-10 to find a fallback cover.
- **FR-004**: The system MUST display the fallback cover when a matching Open Library cover is available.
- **FR-005**: The system MUST display `/images/no-cover.svg` for an included ISBN-bearing result when neither source provides a cover; the cover-selection helper MUST also return the placeholder defensively when called without an ISBN.
- **FR-006**: An Open Library failure MUST NOT prevent Google Books results from being displayed.
- **FR-007**: The system MUST evaluate cover availability independently for each search result.
- **FR-008**: The feature MUST be limited to cover fallback. It MUST NOT use Open Library as a backup source for book search results when Google Books is unavailable.
- **FR-009**: The feature MUST NOT require a new API key, database table, or migration.

### Key Entities

- **Book search result**: A result returned for a reader's search, including title, author, ISBN information, and its selected cover image.
- **Cover source**: The source that supplies the displayed image, either Google Books, Open Library, or the standard placeholder.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of the defined acceptance scenarios pass for ISBN-13 fallback, no-cover placeholder, ISBN-10 fallback, and Open Library unavailability.
- **SC-002**: 100% of search results that already have a Google Books cover retain that cover.
- **SC-003**: 100% of search results without a usable cover display a non-broken placeholder image.
- **SC-004**: When Open Library is unavailable, 100% of otherwise valid Google Books results remain visible to the reader.
- **SC-005**: The feature adds no new persistent data requirements and requires no reader-visible workflow beyond the existing search.

## Assumptions

- The existing search-results page already renders a cover URL for each result.
- A book's ISBN-13 is the preferred matching identifier; ISBN-10 is used only when ISBN-13 is unavailable.
- Open Library cover availability can change independently of Google Books metadata.
- The existing `/images/no-cover.svg` asset is available to the search-results page.
- Results without an ISBN remain excluded by the existing BN-6 `MapToSearchItem` behavior because books are keyed by ISBN; AC2 applies only to included results that have an ISBN.
- `PickCoverUrl(imageLinks: null, isbn: null)` returns `/images/no-cover.svg` as a defensive fallback, tested directly rather than through normal result mapping.
- Open Library search as a backup when Google Books is down is outside this feature's scope.
- No new database tables, migrations, or API keys are needed.
