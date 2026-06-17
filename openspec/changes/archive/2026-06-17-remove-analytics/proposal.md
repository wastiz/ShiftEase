## Why

The analytics system was built for personal usage tracking of the hosted ShiftEase
service and has no value for self-hosters running their own instance. Keeping it in the
open-source codebase adds a PostgreSQL table, a background service, injected dependencies
across multiple BLL services, and dead complexity that new contributors have to understand.

## What Changes

- **BREAKING** — Delete the entire `backend/Analytics/` project (`AnalyticsBackgroundService`,
  `AnalyticsChannel`, `AnalyticsEventData`, `AnalyticsExtensions`, `AnalyticsService`,
  `Analytics.csproj`)
- **BREAKING** — Remove `Domain/AnalyticsEvent.cs` and the `AnalyticsEvents` DbSet from
  `AppDbContext`
- **BREAKING** — Remove `BLL.Contracts/IAnalyticsService.cs` and
  `BLL.Contracts/AnalyticsEventTypes.cs`
- Remove the `AddAnalytics()` registration from `API/Program.cs`
- Remove `IAnalyticsService` injection and all `.Track(...)` calls from BLL services
  (`IdentityService`, `EmployeeService`, `OrganizationService`, all four schedule
  generator services, `ScheduleService`)
- Remove the `AddAnalyticsEvents` EF migration and drop the `AnalyticsEvents` table
  reference from the model snapshot
- Remove `Analytics.csproj` from `ShiftEaseAPI.sln`
- Update tests that mock `IAnalyticsService` to remove the mock setup
- Remove the "Analytics Dashboard" feature card from the landing page (`Features.tsx`)
  and the related testimonial quote from `Testimonials.tsx`

## Capabilities

### New Capabilities

_None — this change is a pure removal._

### Modified Capabilities

_None — no remaining feature requirements change. Analytics was a standalone
cross-cutting concern with no externally visible API surface._

## Impact

- **Backend projects deleted**: `Analytics/` (entire project)
- **Backend files deleted**: `Domain/AnalyticsEvent.cs`,
  `BLL.Contracts/IAnalyticsService.cs`, `BLL.Contracts/AnalyticsEventTypes.cs`,
  `DAL/Migrations/20260608131738_AddAnalyticsEvents.cs` (and `.Designer.cs`)
- **Backend files modified**: `API/Program.cs`, `DAL/AppDbContext.cs`,
  `DAL/Migrations/AppDbContextModelSnapshot.cs`, `ShiftEaseAPI.sln`,
  and all BLL services that call `IAnalyticsService`
  `src/components/features/landing/Testimonials.tsx`
- **Database**: `AnalyticsEvents` table is no longer created by migrations; a cleanup
  migration should drop it for existing deployments
- **No API surface change** — analytics was never exposed via HTTP endpoints
- **No frontend API hooks** — no TanStack Query hooks or `api.*` calls reference analytics
