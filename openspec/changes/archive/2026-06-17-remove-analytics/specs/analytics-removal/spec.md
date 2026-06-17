## REMOVED Requirements

### Requirement: Event tracking across BLL services
The system SHALL NOT collect or persist internal analytics events. No `IAnalyticsService`
interface or implementation SHALL exist in the codebase. BLL services (IdentityService,
EmployeeService, OrganizationService, ScheduleService, and schedule generator services)
SHALL NOT carry an analytics dependency in their constructors.

**Reason**: Analytics was personal usage tracking for the maintainer's hosted instance.
Self-hosters have no use for it and it adds unnecessary complexity.
**Migration**: No migration path — this is internal instrumentation with no public API
surface. Existing `AnalyticsEvents` rows in PostgreSQL will remain until the
`RemoveAnalyticsEvents` EF migration drops the table.

#### Scenario: Backend compiles without Analytics project
- **WHEN** the solution is built after this change
- **THEN** `dotnet build` SHALL succeed with no reference to `IAnalyticsService`,
  `AnalyticsEvent`, or `AnalyticsEventTypes`

#### Scenario: Database migration drops the table safely
- **WHEN** the `RemoveAnalyticsEvents` EF migration runs against an existing database
- **THEN** the `AnalyticsEvents` table SHALL be dropped
- **WHEN** the same migration runs against a fresh database that never had the table
- **THEN** the migration SHALL complete without error (using `DROP TABLE IF EXISTS`)

#### Scenario: Schedule generation succeeds without analytics
- **WHEN** any schedule generator service generates a schedule
- **THEN** the operation SHALL complete without invoking any analytics tracking code

#### Scenario: Auth flows succeed without analytics
- **WHEN** an employer registers, logs in, or logs out
- **THEN** the operation SHALL complete without invoking any analytics tracking code

### Requirement: Analytics Dashboard marketing copy
The landing page SHALL NOT display a feature card or testimonial quote describing an
"Analytics Dashboard" capability that no longer exists in the product.

**Reason**: The feature card and testimonial were marketing copy for the hosted service.
Displaying them for a self-hosted open-source release would be misleading.
**Migration**: None — these are UI text strings with no behavioral impact.

#### Scenario: Landing page features list excludes analytics
- **WHEN** a visitor views the landing page features section
- **THEN** no feature card with the title "Analytics Dashboard" (or its i18n equivalent)
  SHALL be rendered
