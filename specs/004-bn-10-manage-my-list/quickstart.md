# Quickstart: Validate Manage My List

## Prerequisites

- .NET 10 SDK.
- A BookNest database configured as described in the [README database instructions](../../README.md#database).
- A test account, or permission to register one in the configured database.
- The existing Add to List flow available for creating a starting entry.

Do not put database passwords or API keys in source files or commit them. The application applies pending EF Core migrations at startup; this feature itself does not add a migration.

## Automated validation

From the repository root, run:

```powershell
dotnet test tests/IS7024_GroupProject.Tests/IS7024_GroupProject.Tests.csproj
```

Expected: all existing tests and the new My List tests pass. Automated coverage includes persistence and PageModel behavior for status transitions, FinishedDate set/clear behavior, removal, empty-state conditions, invalid input, ownership protection, authentication, and preservation of Add to List. The current test project has no Razor rendering host, so the rendered markup checks below are intentionally manual.

## Manual end-to-end checks

1. Start the application with the configured BookNest database:

   ```powershell
   dotnet run --project IS7024_GroupProject.csproj
   ```

2. Register or sign in as Reader A, search for The Martian, and add it if it is not already on the list.
3. Open My Reading List and verify The Martian appears as Want to Read with its existing title, author, and cover/fallback.
4. Change The Martian to Reading and submit. Verify it remains on the list as Reading.
5. Change The Martian to Finished and submit. Verify it shows Finished with today's date.
6. Change The Martian back to Want to Read or Reading. Verify the finished date is cleared.
7. Select Remove. Verify The Martian no longer appears and the exact empty-state message is displayed:
   `Your list is empty. Search for a book to get started.`
8. Add The Martian again using the existing Add to List flow. Verify it returns as Want to Read and the empty-state message is absent.
9. Register or sign in as Reader B and add a separate book. Attempt to submit a crafted status or remove request using Reader A's entry identifier. Verify Reader A's entry is unchanged and remains inaccessible to Reader B.
10. Sign out and request My Reading List directly. Verify the existing authentication challenge occurs.

11. In the browser, inspect the rendered page for an associated label and exactly three status choices: Want to Read, Reading, and Finished. Submit each transition and verify the displayed status and FinishedDate.
12. In the browser, verify the rendered Remove form targets the selected entry, verify the exact empty-state message after removing the final entry, and verify that the message is absent after adding a book again.
13. Resize the browser to a narrow viewport and verify the cards, status controls, Remove action, validation feedback, and FinishedDate remain usable and follow the existing BookNest layout.

## References

- [Data model and state transitions](data-model.md)
- [My List user interface contract](contracts/reading-list.md)
