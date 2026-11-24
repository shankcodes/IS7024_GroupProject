# Research: Add a Book to My List

This is a retrospective record of decisions embodied by the completed branch. No unresolved technical choices remain for BN-9.

## Decisions

### 1. Reuse the search-result identity to find a catalog book

- **Decision**: Use the stable Google Books identifier to locate the shared `Book` record; create it from the search-result metadata only when it does not already exist.
- **Rationale**: Search results already carry that identifier, and the `Book` model enforces its uniqueness. Reusing an existing record prevents an add request from replacing canonical title, author, or cover values used by other readers.
- **Alternatives considered**: Use title/author matching, which is not a reliable unique identity; always create a new book, which duplicates catalog entries.

### 2. Enforce one list entry per reader and book

- **Decision**: Check the reader's list before adding and use the unique reader/book pair constraint as a persistence-level guard.
- **Rationale**: Repeated submissions must be idempotent from the reader's perspective and must not create multiple copies.
- **Alternatives considered**: Rely only on client-side button state, which cannot prevent repeated or concurrent requests.

### 3. Keep authentication and submitted data checks on the server

- **Decision**: Require a valid authenticated user for the add handler and apply server-side validation to the posted book fields. Signed-out users receive the sign-in flow; a successful or duplicate operation returns visible status feedback.
- **Rationale**: UI state is not an authorization boundary, and search-result fields must be validated independently of the browser.
- **Alternatives considered**: Trust the visible button or accept unvalidated form values, which would permit unauthorized or malformed requests.

### 4. Use the existing page/service/model structure

- **Decision**: Keep the action on the Home search page, persist through `ReadingListStore`, and render saved metadata on My List.
- **Rationale**: This follows the existing application organization and keeps the feature within the current .NET 10 Razor Pages application.
- **Alternatives considered**: Add a separate API or duplicate persistence logic in a new page, neither of which is needed for this user flow.

### 5. Use Azure SQL for application persistence as an approved scope change

- **Decision**: Use the SQL Server EF Core provider and a SQL Server initial migration for the application database; retain SQLite and InMemory providers in the test project.
- **Rationale**: The project owner separately requested the application connect to the Azure SQL database. The user's deployment credentials belong in .NET User Secrets locally and protected deployment configuration, never in the repository.
- **Alternatives considered**: Keep SQLite as the application provider, as shown in the BN-9 handout example. This was rejected only because the owner explicitly requested Azure SQL. A schema migration does not copy prior SQLite data.

### 6. Avoid SQL Server multiple-cascade-path migration failures

- **Decision**: Configure the direct user relationships for review comments and review likes with `NoAction`; preserve their review-parent cascades.
- **Rationale**: SQL Server rejects the multiple cascade paths produced by the previous relationship configuration. The migration applied successfully with this provider-compatible relationship behavior.
- **Alternatives considered**: Leave all cascading deletes enabled, which SQL Server will not accept; replace the provider or restructure unrelated social features, which would expand scope.

### 7. Apply schema migrations at application startup

- **Decision**: Use the existing startup migration pattern to apply pending migrations.
- **Rationale**: It lets a newly configured database receive the application's schema without a separate manual migration command.
- **Alternatives considered**: Require each local developer to apply migrations manually. Production rollout still requires a valid configured connection and suitable database permissions.

## Risks and Follow-up

- The Azure App Service connection string and identity/permissions still need deployment-environment verification; local database connectivity does not prove deployment readiness.
- The database firewall was documented as broadly open in the README. This branch did not narrow the firewall; restrict it to the required application and developer networks before production use.
- The app's SQL Server startup migration requires the configured identity to have migration privileges.
- PR #9 was still open at the last review. Confirm the search handler and Index page integrate cleanly when that PR's homepage changes are merged.
