## Why

The landing page is marketing content that has no place in an open-source, self-hosted tool. Visitors who reach the root URL are self-hosters or developers who should go straight to sign-in, not a promotional page.

## What Changes

- **Remove** the landing page route (`app/page.tsx` or equivalent root route file) and all components used exclusively by it.
- **Update** Next.js middleware to redirect `GET /` → `/sign-in` (permanent or temporary redirect).
- **Update** the public-routes list in middleware — `/` no longer needs to be a passthrough public route; it becomes a redirect-only entry so unauthenticated users land on `/sign-in`.
- **Remove** any i18n translation keys that exist solely for landing page copy.

## Capabilities

### New Capabilities

- `root-redirect`: Root URL (`/`) permanently redirects to `/sign-in` instead of rendering a landing page.

### Modified Capabilities

- `auth-routing`: The middleware public-route list changes — `/` is removed as a standalone public page and treated as a redirect source.

## Impact

- **Frontend files**: root route page component deleted; Next.js middleware updated; possibly `messages/*.json` translation files trimmed.
- **No backend changes**: purely a frontend routing concern.
- **No auth-flow changes**: `/sign-in`, `/reset-password`, and `/onboarding` remain public; protected routes are unaffected.
- **SEO**: no impact (landing page SEO metadata will be handled in a separate cleanup task).
