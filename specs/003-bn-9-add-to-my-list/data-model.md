# Data Model: Add a Book to My List

## Reader

An authenticated Identity user who owns personal reading-list entries.

- **Identity**: Integer user key.
- **Relationship**: A reader may have many reading-list entries.
- **Access rule**: A reader may add entries only to their own list.

## Book

A shared catalog record that can be referenced by multiple readers.

- **Identity**: Integer book key.
- **Stable external identity**: Google Books identifier; unique across catalog records.
- **Display data**: Required title and author; optional cover URL.
- **Relationships**: A book may be referenced by many reading-list entries and may have reviews.
- **Validation**: The add form requires title, author, and external identifier, with length bounds; cover URL is optional and length-bounded.
- **Metadata rule**: If the catalog record already exists, adding it to a reader's list does not overwrite its title, author, or cover with posted values.

## Reading-list entry

The association between one reader and one book, including the reader's status for that book.

- **Identity**: Integer list-entry key.
- **Relationships**: References exactly one reader and one book.
- **Uniqueness**: At most one entry per reader/book pair.
- **Initial state**: Status is Want to Read; the existing status enum also supports Reading and Finished.
- **Other data**: Optional finished date.
- **Behavior**: A duplicate add leaves the existing entry unchanged.

## Creation Flow

1. Validate the authenticated reader and submitted search-result fields.
2. Find the shared book by its stable external identifier; create it if absent.
3. If that reader/book pair already exists, return duplicate feedback without creating another entry.
4. Otherwise, create one entry in Want to Read status.

The application uses Azure SQL in this branch. Automated persistence behavior tests use isolated SQLite databases; provider choice does not alter these entity-level relationships.
