## Context

Currently the Next.js App Router has a root route (`app/page.tsx`) that renders a landing/marketing page. The middleware treats `/` as a public route alongside `/sign-in`, `/reset-password`, and `/onboarding`. For the open-source release this page adds noise and no value — the target audience (self-hosters, developers) should land directly on sign-in.

## Goals / Non-Goals

**Goals:**
- Delete the landing page route and all components/assets used exclusively by it.
- Make `GET /` issue a redirect to `/sign-in` so no blank or 404 response is returned.
- Keep the rest of the public-route list (`/sign-in`, `/reset-password`, `/onboarding`) untouched.
- Remove any i18n translation keys used only by the landing page.

**Non-Goals:**
- Changing auth logic, token handling, or cookie behaviour.
- Removing SEO metadata (separate task in the open-source cleanup list).
- Touching the backend in any way.

## Decisions

### Redirect in Next.js middleware vs. a Next.js `page.tsx` redirect component

**Decision**: Implement the redirect inside the existing Next.js middleware (`middleware.ts`) rather than keeping a minimal `app/page.tsx` that calls `redirect()`.

**Rationale**: The middleware already owns all routing decisions (public vs. protected routes). Putting the `/` → `/sign-in` redirect there keeps routing logic in one place and avoids leaving a vestigial page file. A `page.tsx` redirect would also require RSC execution before the redirect fires, adding unnecessary latency.

**Alternative considered**: Keep `app/page.tsx` with a single `redirect('/sign-in')` call. Rejected because it leaves an empty route file and splits routing responsibilities.

### Redirect status code

**Decision**: Use HTTP 307 (Temporary Redirect) inside middleware (Next.js `NextResponse.redirect` default), not 301.

**Rationale**: A 301 would be cached aggressively by browsers, making it hard to change the destination in a future iteration without cache-busting. 307 is safe and still semantically correct here. If the redirect ever becomes permanent and stable, it can be upgraded later.

## Risks / Trade-offs

- **Bookmarked `/` URLs**: Existing users or browser bookmarks pointing to `/` will be transparently redirected to `/sign-in`. No data loss; minor UX no-op. → No mitigation needed.
- **Landing page component shared code**: Some components or hooks originally written for the landing page might be shared with other routes. Before deleting files, verify each import is not used elsewhere. → Covered in the tasks (grep for usages before removal).

## Migration Plan

1. Delete landing page files (route + feature components).
2. Update middleware to redirect `/` → `/sign-in`.
3. Prune orphaned i18n keys.
4. Run `next build` to verify no dead-import errors remain.
5. No rollback complexity — reverting is a git revert.
