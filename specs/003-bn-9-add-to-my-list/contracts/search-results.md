# Search Results and Add-to-List Contract

This is the user-facing contract of the Razor Pages search and list flow; the application does not expose a separate public API for this feature.

## Home page

- **Read**: `GET /?Query={search text}` searches the existing book-search service.
- **Signed-in add**: Submit the Add to List form to `POST /?handler=AddToList`.
- **Form fields**:
  - `AddToListInput.Title` (required; at most 250 characters)
  - `AddToListInput.Author` (required; at most 250 characters)
  - `AddToListInput.GoogleBooksId` (required; at most 100 characters)
  - `AddToListInput.CoverUrl` (optional; at most 2048 characters)
- **Authorization**: The server requires an authenticated user with a valid user identifier and a corresponding user record. A visitor who is not signed in is challenged to authenticate. An invalid form returns HTTP 400.
- **Duplicate result**: Redirect to the search page, retain the query, and display `This book is already on your list.`
- **Successful result**: Redirect to My List and display `Added to your list.`

The posted title, author, identifier, and cover are validated server-side. If the identifier matches an existing shared book, its stored descriptive metadata is retained.

## My List page

- Displays entries belonging to the signed-in reader, ordered by book title.
- Shows each book's title, author, status, and cover image.
- Uses the no-cover image fallback when a cover is absent or fails to load.
- A newly created entry has Want to Read status.
