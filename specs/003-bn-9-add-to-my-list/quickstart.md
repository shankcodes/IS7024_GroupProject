# Quickstart: Validate Add to My List

## Prerequisites

- .NET 10 SDK.
- A BookNest database configured as described in the [README database instructions](../../README.md#database).
- A Google Books API key configured in .NET User Secrets if required by the local search configuration.
- A test account, or permission to register one in the configured database.

Do not put database passwords or API keys in source files or commit them. The application applies pending EF Core migrations at startup, so the configured database identity needs the necessary migration permissions.

## Automated validation

From the repository root, run:

```powershell
dotnet test tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj
```

Expected: the test project passes, including persistence checks for adding a book, duplicate adds, preserving existing shared-book metadata, and listing book details.

## Manual end-to-end checks

1. Configure `ConnectionStrings:DefaultConnection` and, if needed, `GoogleBooks:ApiKey` using the README's User Secrets instructions.
2. Start the app:

   ```powershell
   dotnet run --project IS7024_GroupProject.csproj
   ```

3. Register or sign in, then search for "The Martian".
4. Add a result that is not already on the account's list. Open My List and verify the title, author, cover (or fallback), and Want to Read status; verify the success message.
5. Return to the search results and add the same book again. Verify the exact message "This book is already on your list." and confirm only one entry appears.
6. Sign out, search for a book, and choose its sign-in action. Verify the app sends you to the sign-in page and does not create an entry.
7. Optionally add "Dune" by Frank Herbert and verify its title, author, and cover on My List.

## Expected results

- A saved book appears once per reader, initially as Want to Read.
- Repeated adds do not create another copy or overwrite existing shared book metadata.
- A signed-out visitor must authenticate before an add can be completed.
