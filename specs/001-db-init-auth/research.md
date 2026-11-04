# Research: Multi-User Reading Data and Authentication

## Decision: Use ASP.NET Core Identity with integer keys

**Rationale**: The application already uses ASP.NET Core and EF Core. Identity supplies password hashing, account lifecycle behavior, authentication cookies, and role support without creating a custom credential system. Integer keys preserve the existing `UserId` foreign keys in reading data.

**Alternatives considered**:

- Custom password storage: rejected because credential security and account recovery would become application-owned risk.
- External OAuth-only login: deferred because the current MVP needs a local email/password path.

## Decision: Use a shared `Book` catalog and user-owned join records

**Rationale**: Books are shared by all users, while reading status, goals, reviews, comments, and likes are personal or user-authored. Unique `(UserId, BookId)` and `(UserId, Year)` constraints prevent duplicate personal records.

**Alternatives considered**:

- Copying a book record per user: rejected because it duplicates catalog data and makes updates inconsistent.
- Storing all user data as JSON: rejected because relational constraints and aggregate queries would be weaker.

## Decision: Keep reviews separate from reading progress

**Rationale**: A `BookReview` table preserves each user's rating and text, allowing aggregate ratings and moderation without placing a single rating on the shared `Book` row. Comments and likes are separate tables because they are one-to-many and many-to-many interaction data.

**Alternatives considered**:

- Rating columns on `Book`: rejected because it cannot preserve individual ratings or calculate trustworthy aggregates.
- Rating and review columns on `ReadingList`: rejected because reading progress and community content have different ownership and lifecycle concerns.

## Decision: Store moderation state on reviews and comments

**Rationale**: Pending, approved, rejected, and hidden states support a minimum public-content moderation workflow without deleting content needed for review or audit decisions.

**Alternatives considered**:

- Hard deletion only: rejected because moderation needs reversible visibility decisions.
- Separate moderation history in the MVP: deferred until audit-history requirements are known.

## Decision: Apply migrations during startup and defer Admin provisioning

**Rationale**: The local project can initialize an empty SQLite database automatically. Admin role and account provisioning should be controlled by the cloud deployment process rather than automatically changing a shared production database at application startup.

**Alternatives considered**:

- Seed an Admin user and password in source: rejected because it would expose a secret and create an unsafe default account.
- Automatic Admin role seeding at startup: deferred because shared cloud environments should provision roles explicitly.
- Manual-only migration application: retained as an operational option, but startup migration keeps local setup reliable for the project.
