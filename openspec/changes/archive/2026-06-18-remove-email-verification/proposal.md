## Why

Email verification was built for a hosted service where preventing spam registrations mattered. For self-hosted open-source deployments, the operator controls who has network access, making verification an unnecessary hurdle that requires working SMTP configuration just to log in for the first time.

## What Changes

**Backend — files deleted:**
- `Domain/EmailVerificationToken.cs`
- `DAL.Contracts/IEmailVerificationRepository.cs`
- `DAL/Repositories/EmailVerificationRepository.cs`

**Backend — files modified:**
- **BREAKING** — `Domain/Employer.cs`: remove `IsEmailVerified` property
- `DAL/AppDbContext.cs`: remove `DbSet<EmailVerificationToken>`
- `BLL.Contracts/IIdentityService.cs`: remove `VerifyEmailAsync(string token)`
- `BLL.Contracts/IMailService.cs`: remove `SendEmailVerificationAsync(string toEmail, string verificationToken)`
- `BLL/Services/IdentityService.cs`: remove `_emailVerificationRepo` dependency; simplify `RegisterEmployerAsync` (no token, no email, immediate success); remove `IsEmailVerified` gate from `LoginEmployerAsync`; remove `VerifyEmailAsync`; clean up `GoogleAuthEmployerAsync` and `DeleteAccountAsync`
- `BLL/Services/MailService.cs`: remove `SendEmailVerificationAsync`
- `API/Controllers/IdentityController.cs`: remove `GET /api/auth/verify-email` endpoint
- `API/Program.cs`: remove `IEmailVerificationRepository`/`EmailVerificationRepository` DI registration
- `Tests/UnitTests/IdentityServiceTest.cs`: remove `_emailVerificationRepoMock` and update affected test setup

**Backend — migration:**
- Drop `EmailVerificationTokens` table (`IF EXISTS`)
- Drop `IsEmailVerified` column from `Employers` table

**Frontend — files deleted:**
- `src/app/(public)/verify-email/page.tsx`

**Frontend — files modified:**
- `src/hooks/api/auth.ts`: remove `useVerifyEmail`
- `src/components/features/sign-in/RegisterForm.tsx`: remove "check your email" success state; on successful registration switch directly to login mode
- `src/components/features/sign-in/LoginForm.tsx`: remove `emailNotVerified` state and amber warning block; remove `EMAIL_VERIFICATION_FAILED` error case
- `src/lib/routes.ts`: remove `/verify-email` from `PUBLIC_ROUTES`
- `frontend/messages/en.json`, `et.json`, `ru.json`: remove `auth.emailVerification` translation object
- `src/components/features/sign-in/SignInRightBlock.tsx`: remove the feature item referencing email verification

## Capabilities

### New Capabilities

_None — this is a pure removal._

### Modified Capabilities

- `employer-registration`: Registration no longer requires email verification; employers can log in immediately after registering.
- `employer-auth`: Login no longer blocks on `IsEmailVerified`; the `/verify-email` route and `GET /auth/verify-email` endpoint are removed.

## Impact

- **Backend files deleted**: 3 files (`EmailVerificationToken.cs`, `IEmailVerificationRepository.cs`, `EmailVerificationRepository.cs`)
- **Backend files modified**: 8 files (`Employer.cs`, `AppDbContext.cs`, `IIdentityService.cs`, `IMailService.cs`, `IdentityService.cs`, `MailService.cs`, `IdentityController.cs`, `Program.cs`) + tests
- **Database**: `EmailVerificationTokens` table dropped; `IsEmailVerified` column dropped from `Employers`
- **API surface removed**: `GET /api/auth/verify-email`
- **Frontend files deleted**: 1 page (`/verify-email`)
- **Frontend files modified**: 5 source files + 3 locale JSON files
- **No new dependencies** — pure removal, no alternatives introduced
