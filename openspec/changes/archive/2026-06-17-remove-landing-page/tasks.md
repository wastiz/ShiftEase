## 1. Audit Landing Page Files

- [x] 1.1 Locate the root route file (`app/page.tsx` or `app/[locale]/page.tsx`) and identify all components/hooks it imports
- [x] 1.2 For each imported component/hook, grep the codebase to confirm it is not used by any other route — mark files safe to delete
- [x] 1.3 Identify any landing-page-only i18n keys in `messages/en.json`, `messages/et.json`, `messages/ru.json`

## 2. Delete Landing Page Code

- [x] 2.1 Delete the root route page file (`app/page.tsx` / `app/[locale]/page.tsx`)
- [x] 2.2 Delete all components/assets confirmed as landing-page-only in task 1.2
- [x] 2.3 Remove orphaned i18n keys identified in task 1.3 from all locale files

## 3. Update Middleware Routing

- [x] 3.1 Open `middleware.ts` and remove `/` from the public-routes list (or equivalent matcher array)
- [x] 3.2 Add a redirect rule so that requests matching `/` are redirected to `/sign-in` (307) before any auth check runs
- [x] 3.3 Verify that `/sign-in`, `/reset-password`, and `/onboarding` remain in the public-routes list unchanged

## 4. Verify

- [x] 4.1 Run `next build` and confirm zero TypeScript / import errors
- [ ] 4.2 Start the dev server and manually confirm `GET /` redirects to `/sign-in` in the browser
- [ ] 4.3 Confirm authenticated employer session at `/` still redirects to `/sign-in` (which then forwards to `/dashboard`)
- [ ] 4.4 Confirm other public routes (`/sign-in`, `/reset-password`, `/onboarding`) still load without redirect
