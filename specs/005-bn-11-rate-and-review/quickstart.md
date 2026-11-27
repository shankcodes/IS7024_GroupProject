# BN-11 Validation Quickstart

This guide validates the existing implementation; it does not change application data or require a migration.

## Prerequisites

- .NET 10 SDK.
- The repository's configured local database and a working database connection.
- For browser checks, local Alex/Sam accounts and a test book already present in the database with no existing ratings. The average scenario expects exactly Alex's 4-star rating and Sam's 5-star rating. To add a new book through search, configure a valid Google Books API key as described in the project setup; search availability is not required for the review tests.

## Automated Validation

From the repository root in PowerShell:

```powershell
dotnet test .\tests\IS7024_GroupProject.Tests\IS7024_GroupProject.Tests.csproj
```

Expected: the test project builds and all tests pass, including review create/update, one-review-per-reader/book, invalid and maximum-length input, average/count/rounding, private review loading, unauthenticated access, and Finished-status cases.

## Browser Validation

1. Start the app from the repository root:

   ```powershell
   dotnet run --launch-profile https
   ```

2. Open `https://localhost:7210/` and sign in as reader Alex. A browser warning may appear if the local ASP.NET Core development certificate is not trusted.
3. Ensure a test book is on Alex's Reading List with status **Finished**, then open its Book Details page.
4. Choose four filled stars, enter summary `Fun adventure` and comment `Bilbo's journey kept me hooked`, then save.
   - Expected: the success message appears after redirect; the 4-star selection, summary, and comment remain in Alex's form.
5. Change Alex's rating to five stars and save again.
   - Expected: the form now shows five stars and the same summary/comment. The automated uniqueness test verifies this is still one reader/book review record.
6. Change Alex's rating back to four stars and save. This sets up the two-reader aggregate scenario.
7. In a separate browser profile or private window, sign in as reader Sam. Mark the same test book **Finished**, open Book Details, and save a five-star rating with a distinct summary and comment.
   - Expected in Alex's session after reload: `Average rating: 4.5 (2 ratings).` Alex's saved summary/comment remain visible, while Sam's summary/comment are not rendered.
8. As either reader, open a book whose Reading List status is **Want to Read**.
   - Expected: `Finish this book to rate it.` appears; rating stars, summary, comment, and Save are disabled. The automated handler test verifies an unfinished book cannot be saved by posting directly.
9. With exactly one rating for a separate test book, verify the label says `1 rating`, not `1 ratings`.
10. The automated tests verify ratings outside 1–5 and summary/comment values beyond 200/4,000 characters are rejected without changing an existing review; exact maximum lengths are accepted.

## Related Design References

- [Book Details review contract](contracts/book-details.md) defines page and form behavior.
- [Data model](data-model.md) defines the uniqueness, validation, and aggregate rules.
