# Data Model: Homepage Dashboard

This feature reads existing persisted entities only. It adds no entity, migration, table, or stored example data.

## Existing entities used

### Reader (`User`)

- **Role**: Owner of reading-list entries and yearly goals.
- **Relationship**: One user has many `ReadingList` entries and many `ReadingGoal` entries.
- **Access rule**: Resolve the current authenticated user and filter every personal query by that user's integer ID. Signed-out requests load no personal records.

### Book (`Book`)

- **Role**: Catalog information displayed for a reader's saved entries.
- **Dashboard fields**: Existing title, author, and optional cover URL.
- **Relationships**: A book may be referenced by multiple users' `ReadingList` entries.
- **Display rule**: Use the existing no-cover placeholder if there is no cover URL; do not generate or infer book details.

### ReadingList (`ReadingList`)

- **Role**: Associates one reader and one book with a reading state.
- **Existing fields used**: `UserId`, `BookId`, `Status`, nullable `FinishedDate`, and associated `Book`.
- **Relevant states**: `Reading` for Currently Reading; `WantToRead` for Read Next; `Finished` for completed activity.
- **Counting rule**: Count a completion for year Y only when the entry is `Finished` and its actual `FinishedDate` is on or after January 1 of Y and before January 1 of Y+1. A null date is not assigned a year or month.
- **Relationship/constraint**: Existing unique reader/book association prevents duplicate list entries.

### ReadingGoal (`ReadingGoal`)

- **Role**: A single reader's target for a particular year.
- **Existing fields used**: `UserId`, `Year`, `TargetBooks`.
- **Relationship/constraint**: Composite primary key on `(UserId, Year)`.
- **Validation**: `TargetBooks` must be at least 1. Absence of the current-year row represents no configured goal and must not be replaced with a made-up target.

## Page view data

The page model should expose only presentation data derived from the above entities: current year; optional configured target; actual finished count; current reader's Reading/WantToRead book entries; and current-year finished entries grouped by recorded month. Search results remain the existing `BookSearchResult` and `BookSearchItem` data. This presentation state is not persisted as a new domain model or database entity.
