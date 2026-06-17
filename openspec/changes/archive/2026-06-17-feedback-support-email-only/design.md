## Context

Feedback and support messages were built for a managed deployment where the maintainer's private admin app consumed the read/management endpoints. Those endpoints write to and read from two PostgreSQL tables (`FeedbackResponses`, `SupportMessages`). Email notifications are already sent on every submission — `MailService` has a hardcoded `AdminEmail = "valnos04@gmail.com"` constant and both `SendFeedbackNotificationAsync` and `SendSupportNotificationAsync` deliver to it.

For the open-source release there is no admin app, so the DB tables and all read/management operations become dead weight that self-hosters have to provision database capacity for with no benefit.

## Goals / Non-Goals

**Goals:**
- Feedback submissions and support message sends deliver exclusively via email to the configured admin address
- `POST /api/feedback/submit` and `POST /api/feedback/support/send-message` continue to work identically from the caller's perspective
- All DB-touching code paths (repositories, domain entities, DAL DTOs, DbSets, admin endpoints) are fully removed so the codebase compiles and no orphaned tables are created on fresh deploys
- An EF migration drops the two tables for existing deployments (safe no-op on fresh installs via `IF EXISTS`)

**Non-Goals:**
- Changing SMTP configuration or email formatting — the existing `MailService` implementation is correct and stays intact
- Adding any alternative storage or queuing for messages
- Any frontend changes — the frontend only uses the two POST endpoints that are kept

## Decisions

### 1. Hard delete — no stub repositories or null-object pattern

The repository interfaces exist solely to persist to and read from the DB. There is no business logic in them. Keeping a no-op stub would add dead abstraction and confuse contributors about why the interfaces exist.

_Alternative considered: replace DB calls with a no-op but keep the interfaces — rejected for the same reasons documented in the analytics removal: it preserves dead constructor dependencies._

### 2. IMailService parameter type fix for SendSupportNotificationAsync

`IMailService.SendSupportNotificationAsync` currently takes `DalSupportMessage`, which is in the DAL.DTO layer being deleted. The method is called from `SupportService` (BLL) and implemented in `MailService` (BLL). The fix is to inline the three fields (`SenderEmail`, `Subject`, `Message`) as separate parameters, or introduce a minimal `BllSupportMessage` record in BLL.DTO as the parameter type.

_Decision: use separate string parameters_ (`string senderEmail, string subject, string message`) since the DTO had exactly these three fields and there is no reason to add a new DTO class for three primitives. This keeps the signature self-documenting and avoids creating a DTO that exists only to group three strings.

_Alternative considered: keep `DalSupportMessage` — rejected because it would require keeping a DAL.DTO type in an interface defined at BLL.Contracts, which violates the layering rule (BLL must not depend on DAL.DTO)._

### 3. Remove BllFeedbackResponse mapping in FeedbackService

`FeedbackService.SubmitFeedbackAsync` currently maps `BllFeedbackResponse` → `DalFeedbackResponse` before calling the repository. Once the repository is removed, the mapping is dead. The method body becomes a single line: `await _mailService.SendFeedbackNotificationAsync(dto);`. `BllFeedbackResponse` itself stays because it is the controller's request body type and the mail service's parameter type.

### 4. Migration strategy: add a drop-if-exists migration, do not delete existing migrations

Unlike the analytics change (where the migration that created the table was also deleted), the feedback/support tables were created as part of a large initial migration (`InitialCreate`). Deleting that migration is not safe. Instead, add a new EF migration `RemoveFeedbackSupportTables` whose `Up()` executes:

```sql
DROP TABLE IF EXISTS "FeedbackResponses";
DROP TABLE IF EXISTS "SupportMessages";
```

and whose `Down()` is empty (no rollback — re-creating the tables is out of scope).

The `AppDbContextModelSnapshot` must have both `DbSet`s and their entity configurations removed so EF's state matches the final schema.

_Alternative considered: only remove the DbSets without a migration — rejected because EF would then report pending migration divergence on every startup._

## Risks / Trade-offs

- **Risk: compile error from missed reference** — any file still referencing `IFeedbackRepository`, `ISupportRepository`, `FeedbackResponse`, `SupportMessage`, `DalFeedbackResponse`, `DalSupportMessage`, or `DalSupportReply` will break the build.
  → _Mitigation: run `dotnet build` after the change and treat any remaining compiler error as the signal to fix._

- **Risk: AppDbContextModelSnapshot drift** — if the snapshot is not updated to remove both entities, EF will report a pending migration.
  → _Mitigation: manually edit the snapshot to remove the two entity configurations, then verify with `dotnet ef migrations list` showing no pending migrations._

- **Trade-off: existing data lost on migration** — running `RemoveFeedbackSupportTables` on an existing deployment drops any stored feedback and support messages. This is acceptable since those messages were already forwarded by email and the admin app that consumed them is being removed.

## Migration Plan

1. Remove `DbSet<FeedbackResponse>` and `DbSet<SupportMessage>` from `AppDbContext` and update model snapshot
2. Delete Domain entities, DAL.Contracts interfaces, DAL repositories, DAL DTOs, and BLL support DTOs
3. Update `IMailService.SendSupportNotificationAsync` signature to use three string parameters; update `MailService` implementation and `SupportService` call site accordingly
4. Simplify `FeedbackService`: remove repository field/constructor param, remove mapping, keep only mail call; remove `GetAllFeedbackAsync`
5. Simplify `SupportService`: remove repository field/constructor param, remove all read/management method implementations; keep only `SendMessageAsync`
6. Slim down `IFeedbackService` and `ISupportService` to match the simplified implementations
7. Remove deregistered repository registrations from `Program.cs`
8. Remove the read/management endpoints from `FeedbackController`; keep the two POST endpoints
9. Add `RemoveFeedbackSupportTables` EF migration with the two `DROP TABLE IF EXISTS` statements
10. Run `dotnet build` — confirm zero errors
11. Run `dotnet test` — confirm all tests pass
