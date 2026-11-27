# Book Details Review Contract

This contract describes the existing server-rendered Razor Pages interaction for BN-11. It is not a separate JSON API.

## Page

### `GET /BookDetails?id={bookId}`

**Authentication**: The page can be viewed without signing in.

**Successful response**:

- Shows the selected book's title, author, and cover when available.
- Shows the average across all non-null ratings for that book, formatted to one decimal place, and the rating count with correct singular/plural wording.
- For an authenticated reader, shows only that reader's rating, summary, and comment in the review form.
- Enables the review form only when the authenticated reader has a `ReadingList` entry for the book with status `Finished`.
- Otherwise, shows `Finish this book to rate it.` and disables the rating controls, summary, comment, and Save button.
- For an unauthenticated reader, shows a sign-in link in place of the review form.

**Unknown book**: Renders the page with `That book could not be found.`

## Save Review

### `POST /BookDetails?handler=SaveReview`

The Razor form posts `ReviewInput.BookId`, `ReviewInput.Rating`, `ReviewInput.Summary`, and `ReviewInput.Review`. The `Review` property is labeled **Comment** in the UI.

**Preconditions**:

- The request must be from an authenticated reader with a valid integer user ID.
- The reader must have a Finished reading-list entry for the submitted book.
- Rating is required and must be an integer from 1 to 5.
- Summary is optional and limited to 200 characters.
- Comment is optional and limited to 4,000 characters.

**Successful response**:

- Creates the reader/book review if one does not exist, or updates the existing review for the same reader and book.
- Redirects to the Book Details page for that book.
- Displays `Your rating and review were saved.` and reloads the reader's saved form fields and updated aggregate.

**Validation or eligibility failure**:

- Re-renders Book Details with field validation messages.
- If the reader has not finished the book, shows `Finish this book to rate it.` and does not persist the submitted review.
- Invalid rating displays `Select a rating from 1 to 5.`

**Unauthenticated save**: Challenges the request to authenticate; no review is saved.

## Aggregate Display

The page counts and averages all non-null ratings for the book. Example: two ratings of 4 and 5 display `Average rating: 4.5 (2 ratings).` A single rating uses `1 rating`. With no ratings, the display is `Average rating: 0.0 (0 ratings).`

## Privacy and Accessibility

- The review form is scoped to the current reader's `BookReview`; another reader's summary and comment are not rendered by this feature.
- Rating choices are 1–5 radio inputs styled as selectable star icons. Their accessible names expose the star value, and the group has the label `Choose a rating`.
- Disabled controls provide the same gating in the rendered UI, while the POST handler independently enforces the Finished-status precondition.
