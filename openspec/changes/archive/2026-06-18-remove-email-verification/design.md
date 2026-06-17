## Context

Email verification spans three layers. The backend has a dedicated `EmailVerificationToken` entity, repository, and service method; the `Employer` domain entity carries an `IsEmailVerified` bool; and `IdentityService` enforces the gate on every login. On the frontend, a full `/verify-email` page exists, the `RegisterForm` shows a "check your email" success state, and the `LoginForm` renders an amber warning block when the backend returns `EMAIL_VERIFICATION_FAILED`.

The flow was built for a hosted SaaS where preventing spam accounts mattered. For self-hosted deployments it adds friction (SMTP must work before the first login) with no practical benefit.

## Goals / Non-Goals

**Goals:**
- Registration immediately produces a usable account — no email step required
- Login for employers has no `IsEmailVerified` gate
- All verification-related code paths, endpoints, DB tables, and UI are removed so the codebase and DB schema are clean
- Existing employers who happen to have `IsEmailVerified = false` in the DB can log in after migration

**Non-Goals:**
- Adding any alternative account-activation mechanism
- Changing the password-reset email flow (it stays intact)
- Modifying the employee login path (employees never had email verification)

## Decisions

### 1. Hard delete — no feature flag or soft disable

Keeping a `IsEmailVerified` column set permanently to `true` by default, or a disabled code path, would leave dead weight for contributors to reason about. Full removal is cleaner.

_Alternative considered: default `IsEmailVerified = true` on new registrations, keep column — rejected because it leaves the column and the login-gate code alive with no purpose._

### 2. Drop `IsEmailVerified` column via migration

Since `Employer.IsEmailVerified` is being removed from the domain model, EF will generate a migration that drops the column. Existing employers with `IsEmailVerified = false` (if any) are implicitly unblocked because the login check is also removed before the migration runs in code.

The migration's `Up()` will contain:
```sql
DROP TABLE IF EXISTS "EmailVerificationTokens";
ALTER TABLE "Employers" DROP COLUMN IF EXISTS "IsEmailVerified";
```

`Down()` is left empty — recreating the token table and re-blocking existing accounts is not a meaningful rollback.

### 3. RegisterForm success: switch to login mode directly

After removing the "check your email" success state, the cleanest UX is to call `setMode("login")` immediately on `onSuccess` (the same button they'd click anyway). No new success banner is needed.

_Alternative considered: show a simple "Account created! You can now log in." banner — rejected as unnecessary complexity; the form already has a login tab._

### 4. Test cleanup: remove mock, don't add replacement tests

`IdentityServiceTest` mocks `IEmailVerificationRepository` and passes it to `IdentityService`. Remove the mock field, setup, and constructor argument. The login test that sets `IsEmailVerified = true` on the mock `Employer` object must have that property removed. No new tests are needed — the removed paths had no meaningful business logic.

## Risks / Trade-offs

- **Risk: missed reference to `IsEmailVerified`** — any remaining reference causes a compile error that is immediately visible.
  → _Mitigation: run `dotnet build` after changes; grep for `IsEmailVerified` before declaring done._

- **Risk: existing employers locked out during migration window** — if code is deployed before the migration runs, the gate code has already been removed so there is no lock-out risk. If migration runs first on an old codebase, no impact since the column still exists and the code still checks it.
  → _No special ordering required._

- **Trade-off: self-hosters lose the ability to require email verification** — this is intentional; operators who want it can re-add it. For the open-source baseline, frictionless self-hosting takes priority.

## Migration Plan

1. Delete the three DAL files and update `AppDbContext` / snapshot
2. Strip `IEmailVerificationRepository` from `IdentityService` and `Program.cs`
3. Simplify `RegisterEmployerAsync`, remove `LoginEmployerAsync` gate, remove `VerifyEmailAsync`, clean up `DeleteAccountAsync` and `GoogleAuthEmployerAsync`
4. Remove `IsEmailVerified` from `Employer.cs`
5. Remove `SendEmailVerificationAsync` from `IMailService` / `MailService`
6. Remove `GET /auth/verify-email` from `IdentityController`
7. Update tests
8. Run `dotnet ef migrations add RemoveEmailVerification` from `backend/DAL/`; edit `Up()` to contain only the two SQL statements above
9. Frontend: delete page, remove hook, simplify forms, update routes and i18n
10. Run `dotnet build` and `dotnet test`; run frontend `next build` or type-check
