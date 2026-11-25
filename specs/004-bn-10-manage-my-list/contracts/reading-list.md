# My List User Interface Contract

This feature uses the existing server-rendered Razor Page; it does not expose a separate public API.

## Page access

- **Read**: `GET /ReadingList`.
- **Authorization**: The page requires an authenticated reader. A visitor is challenged by the existing authentication policy.
- **List scope**: The response includes only entries belonging to the authenticated reader, ordered by book title as in the existing store behavior.

## Existing Add to List flow

- **Action**: Existing default `POST /ReadingList`.
- **Behavior**: Remains available and continues to create one Want to Read entry through the existing Add to List path.
- **Feedback**: Preserve existing success and duplicate messages.

## Status update flow

- **Action**: Submit a status form for one visible entry to a named POST handler.
- **Input**: Existing reading-list entry identifier and one of the exact supported statuses: WantToRead, Reading, or Finished.
- **Authorization**: The server uses the authenticated reader identity; a posted entry identifier alone is never sufficient.
- **Validation**: Unsupported status values or missing/invalid entry identifiers are rejected with actionable feedback and do not modify data.
- **Success**: Redirect back to `/ReadingList`; the updated status is displayed. Finished displays today's date; unfinished statuses do not display a finished date.

## Remove flow

- **Action**: Submit a remove form for one visible entry to a named POST handler.
- **Input**: Existing reading-list entry identifier.
- **Authorization**: The removal is scoped to the authenticated reader.
- **Success**: Redirect back to `/ReadingList`; the entry is absent. Removing the final entry displays the empty-state message.
- **Invalid or unauthorized request**: No unrelated entry is deleted and the reader receives actionable feedback.

## Empty state

When the authenticated reader has no entries, display exactly:

`Your list is empty. Search for a book to get started.`

When at least one entry exists, do not display the empty-state message.

## Presentation and accessibility

- Reuse the existing BookNest layout, Bootstrap controls, responsive book-card grid, cover fallback, status feedback, focus styles, labels, and validation message patterns.
- Status selection must have an associated accessible label and expose the three supported choices.
- Remove must be a clearly labeled form action for the associated book and must not rely on color alone.
