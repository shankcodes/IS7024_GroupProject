# Homepage UI Contract

## Surface and search behavior

- The homepage remains the root Razor Page and exposes the current book-search flow.
- Search is submitted as a GET form using the existing `Query` binding; title and author searches, whitespace validation, no-results and service-unavailable feedback, results, and cover fallback retain their existing behavior.
- Search remains available to signed-out visitors. A search result action that requires an account must offer the existing sign-in flow.

## Homepage book-cover behavior

- Use the existing `/images/no-cover.svg` placeholder for every homepage book card: Google Books search results, Currently Reading, Read Next, recent-finished entries, and any other book card.
- A cover URL is usable only if it is a well-formed absolute URL with an `http` or `https` scheme. Null, empty, whitespace-only, malformed, relative, or unsupported-scheme values render the placeholder.
- If a valid image URL fails to load in the browser, switch that image to `/images/no-cover.svg` once. Disable or remove the image's error handler before setting the placeholder source, so a failed placeholder does not trigger a retry loop.

## Signed-in dashboard

- **Reading Goal**: Show the current calendar year and the actual goal and completed-book count when a goal exists. If none exists, show a clear link to the existing goal feature rather than a default target. Progress indication must reflect actual values.
- **Currently Reading**: Show only the current reader's entries with Reading status. With no entries, show an inviting search/list empty state.
- **Recently Finished**: Show current-year Finished entries grouped by their actual completion month. Omit undated entries from month/year groups. With no dated current-year entries, show an empty state.
- **Read Next**: Show only the current reader's WantToRead entries. With no entries, show a path to search or manage the reading list.
- Each area operates independently: no data in one area does not suppress search or other dashboard sections.

## Signed-out dashboard

- Search remains visible and usable.
- Personal goal, progress, Currently Reading, Recently Finished, and Read Next records are not rendered.
- Personal areas show a concise sign-in prompt/link; no other reader's information is disclosed.

## Visual, responsive, and accessibility requirements

- Use a warm cream and sage-green visual theme in existing site styling; retain navigation to existing BookNest features.
- At widths of 320 CSS pixels and above, the page has no horizontal page scroll, clipped primary controls, or overlapping content. Sections remain in a logical vertical reading order on narrow screens.
- Use semantic headings, explicit form labels, meaningful image alternative text, keyboard-operable controls, and visible focus styles. Status/validation messages remain perceivable to assistive technology.
- Do not display sample books, placeholder progress values, assumed completion dates, default goal targets, or invented ratings.
