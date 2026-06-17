## 1. Delete DAL verification files

- [x] 1.1 Delete `backend/Domain/EmailVerificationToken.cs`
- [x] 1.2 Delete `backend/DAL.Contracts/IEmailVerificationRepository.cs`
- [x] 1.3 Delete `backend/DAL/Repositories/EmailVerificationRepository.cs`

## 2. Update AppDbContext and Program.cs

- [x] 2.1 In `backend/DAL/AppDbContext.cs`, remove `public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }` and any now-unused `using` imports
- [x] 2.2 In `backend/API/Program.cs`, remove `builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();` and any now-unused `using` imports

## 3. Remove IsEmailVerified from Domain and snapshot

- [x] 3.1 In `backend/Domain/Employer.cs`, remove `public bool IsEmailVerified { get; set; } = false;`
- [x] 3.2 In `backend/DAL/Migrations/AppDbContextModelSnapshot.cs`, remove the `Domain.EmailVerificationToken` entity block and the `IsEmailVerified` property from the `Employer` entity block

## 4. Strip IEmailVerificationRepository from IdentityService

- [x] 4.1 In `backend/BLL/Services/IdentityServices/IdentityService.cs`, remove the `_emailVerificationRepo` field, remove `IEmailVerificationRepository emailVerificationRepo` constructor parameter, and remove `_emailVerificationRepo = emailVerificationRepo;` assignment
- [x] 4.2 In `RegisterEmployerAsync`: remove the `verificationToken` / `expiresAt` variables, the `CreateTokenAsync` call, the entire `try { SendEmailVerificationAsync } catch` block, and change the return message to `"Registration successful."`
- [x] 4.3 In `LoginEmployerAsync`: remove the `if (!employer.IsEmailVerified)` guard and its return statement
- [x] 4.4 In `GoogleAuthEmployerAsync`: remove `IsEmailVerified = true` from the `new Employer { ... }` object initializer (field no longer exists)
- [x] 4.5 In `DeleteAccountAsync`: remove the `await _emailVerificationRepo.DeleteByEmailAsync(employer.Email);` call
- [x] 4.6 Delete the `VerifyEmailAsync` method entirely from `IdentityService`
- [x] 4.7 Remove `using DAL.Contracts;` import if it is now unused (check no other DAL.Contracts types remain referenced)

## 5. Update BLL contracts

- [x] 5.1 In `backend/BLL.Contracts/IIdentityService.cs`, remove `Task<BllResult<bool>> VerifyEmailAsync(string token);`
- [x] 5.2 In `backend/BLL.Contracts/IMailService.cs`, remove `Task SendEmailVerificationAsync(string toEmail, string verificationToken);`

## 6. Remove SendEmailVerificationAsync from MailService

- [x] 6.1 In `backend/BLL/Services/MailService.cs`, delete the `SendEmailVerificationAsync` method body entirely

## 7. Remove verify-email endpoint from IdentityController

- [x] 7.1 In `backend/API/Controllers/IdentityController.cs`, remove the `[HttpGet("verify-email")]` action (`VerifyEmail`) and any now-unused `using` imports

## 8. Update tests

- [x] 8.1 In `backend/Tests/UnitTests/IdentityServiceTest.cs`, remove `private readonly Mock<IEmailVerificationRepository> _emailVerificationRepoMock;` field
- [x] 8.2 Remove `_emailVerificationRepoMock = new Mock<IEmailVerificationRepository>();` setup line from the constructor
- [x] 8.3 Remove `_emailVerificationRepoMock.Object` from the `IdentityService` constructor call
- [x] 8.4 Remove the `_emailVerificationRepoMock.Setup(...)` and `_mailServiceMock.Setup(m => m.SendEmailVerificationAsync(...))` lines from any test that sets them up
- [x] 8.5 Remove `_mailServiceMock.Verify(m => m.SendEmailVerificationAsync(...), Times.Once)` assertion
- [x] 8.6 In any `Employer` object initializer in tests that sets `IsEmailVerified = true`, remove that property assignment

## 9. Add EF migration

- [x] 9.1 From `backend/DAL/`, run `dotnet ef migrations add RemoveEmailVerification --startup-project ../API` to generate the migration and update the model snapshot
- [x] 9.2 Edit the generated migration's `Up()` to contain only:
  ```csharp
  migrationBuilder.Sql("DROP TABLE IF EXISTS \"EmailVerificationTokens\";");
  migrationBuilder.Sql("ALTER TABLE \"Employers\" DROP COLUMN IF EXISTS \"IsEmailVerified\";");
  ```
  and leave `Down()` empty

## 10. Frontend — delete verify-email page

- [x] 10.1 Delete `frontend/src/app/(public)/verify-email/page.tsx`

## 11. Frontend — remove useVerifyEmail hook

- [x] 11.1 In `frontend/src/hooks/api/auth.ts`, delete the `useVerifyEmail` function

## 12. Frontend — simplify RegisterForm

- [x] 12.1 In `frontend/src/components/features/sign-in/RegisterForm.tsx`, remove the `if (isSuccess)` block that renders the "check your email" card
- [x] 12.2 Add an `onSuccess` callback to the `mutate` call that calls `setMode("login")` so the user is switched to the login tab immediately after registration
- [x] 12.3 Remove unused imports (`MailCheck` icon, any emailVerification-related i18n keys)

## 13. Frontend — simplify LoginForm

- [x] 13.1 In `frontend/src/components/features/sign-in/LoginForm.tsx`, remove `const [emailNotVerified, setEmailNotVerified] = useState(false);`
- [x] 13.2 Remove the `case 'EMAIL_VERIFICATION_FAILED': setEmailNotVerified(true); break;` branch from the error handler
- [x] 13.3 Remove the amber `{emailNotVerified && (...)}` warning block from the JSX
- [x] 13.4 Remove the `setEmailNotVerified(false)` reset call in `handleSubmit`

## 14. Frontend — routes and i18n

- [x] 14.1 In `frontend/src/lib/routes.ts`, remove `"/verify-email"` from the `PUBLIC_ROUTES` array
- [x] 14.2 In `frontend/messages/en.json`, remove the `"emailVerification"` object from the `"auth"` section
- [x] 14.3 In `frontend/messages/et.json`, remove the `"emailVerification"` object from the `"auth"` section
- [x] 14.4 In `frontend/messages/ru.json`, remove the `"emailVerification"` object from the `"auth"` section

## 15. Frontend — SignInRightBlock cleanup

- [x] 15.1 In `frontend/src/components/features/sign-in/SignInRightBlock.tsx`, remove the feature item with `description: "Enterprise-grade security for all email verification processes"` from the feature cards array

## 16. Verification

- [x] 16.1 Run `dotnet build` from `backend/` — confirm zero errors; grep for `IsEmailVerified`, `EmailVerificationToken`, `IEmailVerificationRepository`, `SendEmailVerificationAsync`, `VerifyEmailAsync` and confirm no references remain
- [x] 16.2 Run `dotnet test` from `backend/` — confirm unit tests pass
- [x] 16.3 Confirm `dotnet ef migrations list` from `backend/DAL/` shows `RemoveEmailVerification` as the latest migration
- [x] 16.4 Run `cd frontend && npx tsc --noEmit` — confirm zero TypeScript errors
