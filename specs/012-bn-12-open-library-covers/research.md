# Research: Open Library Cover Fallback

## Decision 1: Use the direct Open Library ISBN cover URL

- **Decision**: When Google Books has no thumbnail or small thumbnail, construct the Open Library cover URL directly from the preferred ISBN:
  `https://covers.openlibrary.org/b/isbn/{isbn}-M.jpg?default=false`
- **Rationale**: Open Library provides a public image endpoint keyed by ISBN. The browser can request the image directly, so the application does not need a second server-side HTTP request, new service, new `HttpClient`, API key, or error-handling path for Open Library availability.
- **Alternatives considered**: A server-side Open Library lookup was rejected because it adds latency, service coupling, failure handling, and configuration beyond the requested covers-only change.

## Decision 2: Preserve the existing cover priority

- **Decision**: Keep the current Google Books thumbnail/small-thumbnail selection and HTTPS normalization first. Use Open Library only when Google Books has no usable cover.
- **Rationale**: Google Books remains the primary result source and existing cover behavior does not change for results that already contain an image.
- **Alternatives considered**: Always preferring Open Library was rejected because it would replace an available primary-source image unnecessarily.

## Decision 3: Use ISBN-13 first, then ISBN-10

- **Decision**: Pass the ISBN-13 to the cover URL when available; otherwise pass the ISBN-10. If neither exists, retain `/images/no-cover.svg`.
- **Rationale**: ISBN is the requested matching key, and ISBN-13 is the preferred identifier while ISBN-10 supports results where Google Books supplies only that identifier.
- **Alternatives considered**: Title or author matching was rejected because it can be ambiguous and is outside the feature scope.

## Decision 4: Use browser-side fallback for unavailable Open Library images

- **Decision**: Add `onerror="this.onerror=null;this.src='/images/no-cover.svg'"` to the existing search-result image markup on the BN-7 Home page.
- **Rationale**: A failed Open Library image request is handled at the point where it occurs. Google Books results remain available because the server does not call Open Library.
- **Alternatives considered**: Server-side availability checks were rejected because they would require an Open Library call and violate the stated constraints.

## Scope confirmation

This plan changes only the Google Books cover-selection method. It does not add Open Library search fallback, persistence, migrations, API keys, services, HTTP clients, or application startup registration. If the BN-7 Home-page search markup is not yet present, the image `onerror` change remains a BN-7 follow-up and no new page is created by BN-12.
