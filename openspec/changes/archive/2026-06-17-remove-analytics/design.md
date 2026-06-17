## Context

The analytics system spans four layers. The `Analytics/` project is a standalone
.csproj that provides a `Channel<AnalyticsEventData>`-backed background service and
an `AnalyticsService` implementation. `IAnalyticsService` is defined in `BLL.Contracts`
and is injected into **8 BLL services** (IdentityService, EmployeeService,
OrganizationService, ScheduleService, and all four schedule generator services) with a
total of **23 `.Track(...)` call sites**. The domain has one entity (`AnalyticsEvent`)
persisted to PostgreSQL via a migration added in June 2026.

The system was never exposed via HTTP — there are no analytics controllers. The only
consumer was the maintainer's private Railway deployment.

## Goals / Non-Goals

**Goals:**
- Fully remove all analytics code so the codebase compiles and tests pass without it
- Produce an EF migration that drops the `AnalyticsEvents` table for any existing
  PostgreSQL instance, using `IF EXISTS` so it is safe on a fresh database
- Keep all unrelated BLL logic in the 8 affected services intact

**Non-Goals:**
- Replacing analytics with any alternative (logging, metrics, OpenTelemetry) — out of
  scope for this change
- Removing the analytics feature card from the marketing landing page is included here
  as a cleanup item (it's a 1-line text change), not a separate change

## Decisions

### 1. Hard delete — no stub or null-object pattern

Since `IAnalyticsService.Track` is void (fire-and-forget) and never influences business
logic, keeping even a no-op stub would add dead abstraction. Remove the interface,
implementation, and all call sites entirely.

_Alternative considered: register a no-op `NullAnalyticsService` — rejected because it
keeps the dependency in every service constructor and confuses contributors about why the
interface exists._

### 2. Migration strategy: delete old migration + add a drop-if-exists migration

Two migration files for `AddAnalyticsEvents` (`.cs` and `.Designer.cs`) will be deleted
from the codebase. A new EF migration `RemoveAnalyticsEvents` will be added that executes
raw SQL:

```sql
DROP TABLE IF EXISTS "AnalyticsEvents";
```

- **New deployments**: `AddAnalyticsEvents` never ran, so there's nothing to drop — the
  `IF EXISTS` makes the migration a safe no-op.
- **Existing deployments**: the migration drops the table cleanly.

The `AppDbContextModelSnapshot` must also have `AnalyticsEvent` removed so EF's state
matches the final schema.

_Alternative considered: keep `AddAnalyticsEvents` and only add a drop migration — rejected
because it creates a confusing create-then-immediately-drop sequence for all new installs._

### 3. Remove the Analytics project from the solution

`Analytics.csproj` is referenced by `API.csproj` via a `<ProjectReference>` and registered
in `Program.cs` via `builder.Services.AddAnalytics()`. Both references must be removed along
with the project from `ShiftEaseAPI.sln`. Deleting the project directory is then safe.

### 4. Test cleanup: remove mocks, don't add replacement tests

The four test files (`IdentityServiceTest`, `EmployeeServiceTest`,
`OrganizationServiceTest`, `ScheduleServiceTest`) each declare
`Mock<IAnalyticsService>` and pass it to the service constructor. Remove the mock field,
the constructor setup line, and the constructor argument. No new tests are needed —
the removed `.Track()` calls had no assertions against them.

`AlgorithmBenchmarkTests` and `GreedyScheduleGeneratorServiceTests` reference analytics
via the service constructors — same treatment.

## Risks / Trade-offs

- **Risk: missed call site** — the 8 services and 23 call sites were found by grep; a
  file that is missed would cause a compile error that's immediately visible in CI.
  → _Mitigation: verify `dotnet build` passes after the change._

- **Risk: migration snapshot drift** — if the model snapshot is not updated after
  deleting the migration, EF will report a pending migration on every startup.
  → _Mitigation: after deleting the migration files, run
  `dotnet ef migrations add RemoveAnalyticsEvents` from the `DAL` project to regenerate
  the snapshot in sync._

- **Trade-off: marketing copy removed** — the landing page "Analytics Dashboard" feature
  card is removed because the feature no longer exists. This makes the landing page
  accurate for self-hosters at the cost of slightly fewer listed features.

## Migration Plan

1. Delete `backend/Analytics/` directory and remove project reference from solution and
   `API.csproj`
2. Delete `Domain/AnalyticsEvent.cs`, `BLL.Contracts/IAnalyticsService.cs`,
   `BLL.Contracts/AnalyticsEventTypes.cs`
3. Remove `using Analytics;` / `AddAnalytics()` from `Program.cs`
4. Remove `DbSet<AnalyticsEvent>` from `AppDbContext`
5. Strip `IAnalyticsService` from all 8 BLL service constructors and delete the 23 call sites
6. Update 6 test files to remove mock fields and constructor arguments
7. Delete the two `AddAnalyticsEvents` migration files
8. From `backend/DAL/`, run `dotnet ef migrations add RemoveAnalyticsEvents` then edit
   the generated `Up()` to contain only `migrationBuilder.Sql("DROP TABLE IF EXISTS \"AnalyticsEvents\";")`
   and the `Down()` to be empty
9. Remove the analytics feature card from `Features.tsx` and the testimonial quote from
   `Testimonials.tsx`
10. Run `dotnet build` and `dotnet test` to verify

**Rollback**: restore files from git. No data is lost (analytics events were internal
tracking only).
