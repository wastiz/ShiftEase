## 1. Remove Analytics project and solution references

- [x] 1.1 Remove `Analytics.csproj` project reference from `ShiftEaseAPI.sln`
- [x] 1.2 Remove `<ProjectReference Include="..\Analytics\Analytics.csproj" />` from `backend/API/API.csproj`
- [x] 1.3 Remove `using Analytics;` and `builder.Services.AddAnalytics();` from `backend/API/Program.cs`
- [x] 1.4 Delete the entire `backend/Analytics/` directory

## 2. Remove Domain and BLL.Contracts analytics types

- [x] 2.1 Delete `backend/Domain/AnalyticsEvent.cs`
- [x] 2.2 Delete `backend/BLL.Contracts/IAnalyticsService.cs`
- [x] 2.3 Delete `backend/BLL.Contracts/AnalyticsEventTypes.cs`

## 3. Remove analytics from AppDbContext

- [x] 3.1 Remove `using Domain;` analytics-related using (if now unused) and `public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; }` from `backend/DAL/AppDbContext.cs`

## 4. Strip IAnalyticsService from BLL services

- [x] 4.1 `IdentityService`: remove `_analytics` field, remove `IAnalyticsService analytics` constructor parameter, delete all 5 `.Track(...)` call sites
- [x] 4.2 `EmployeeService`: remove `_analytics` field, remove `IAnalyticsService analytics` constructor parameter, delete all 3 `.Track(...)` call sites
- [x] 4.3 `OrganizationService`: remove `_analytics` field, remove `IAnalyticsService analytics` constructor parameter, delete the 1 `.Track(...)` call site
- [x] 4.4 `ScheduleService`: remove `_analytics` field, remove `IAnalyticsService analytics` constructor parameter, delete both `.Track(...)` call sites
- [x] 4.5 `GreedyScheduleGeneratorService`: remove `_analytics` field, remove `IAnalyticsService analytics` constructor parameter, delete all 3 `.Track(...)` call sites
- [x] 4.6 `AcoScheduleGeneratorService`: remove `_analytics` field, remove `IAnalyticsService analytics` constructor parameter, delete all 3 `.Track(...)` call sites
- [x] 4.7 `GaScheduleGeneratorService`: remove `_analytics` field, remove `IAnalyticsService analytics` constructor parameter, delete all 3 `.Track(...)` call sites
- [x] 4.8 `AcoGaScheduleGeneratorService`: remove `_analytics` field, remove `IAnalyticsService analytics` constructor parameter, delete all 3 `.Track(...)` call sites

## 5. Clean up test mocks

- [x] 5.1 `Tests/UnitTests/IdentityServiceTest.cs`: remove `Mock<IAnalyticsService> _analyticsMock` field, remove setup line in constructor, remove from service instantiation call
- [x] 5.2 `Tests/UnitTests/EmployeeServiceTest.cs`: same cleanup
- [x] 5.3 `Tests/UnitTests/OrganizationServiceTest.cs`: same cleanup
- [x] 5.4 `Tests/UnitTests/ScheduleServiceTest.cs`: same cleanup
- [x] 5.5 `Tests/UnitTests/AlgorithmBenchmarkTests.cs`: remove analytics mock / constructor arg
- [x] 5.6 `Tests/UnitTests/GreedyScheduleGeneratorServiceTests.cs`: remove analytics mock / constructor arg

## 6. Database migration

- [x] 6.1 Delete `backend/DAL/Migrations/20260608131738_AddAnalyticsEvents.cs`
- [x] 6.2 Delete `backend/DAL/Migrations/20260608131738_AddAnalyticsEvents.Designer.cs`
- [x] 6.3 From `backend/DAL/`, run `dotnet ef migrations add RemoveAnalyticsEvents` to generate a new migration and update the model snapshot
- [x] 6.4 Edit the generated migration's `Up()` to contain only `migrationBuilder.Sql("DROP TABLE IF EXISTS \"AnalyticsEvents\";");` and leave `Down()` empty

## 7. Verification

- [x] 8.1 Run `dotnet build` from `backend/` — confirm zero errors and no remaining references to `IAnalyticsService`, `AnalyticsEvent`, or `AnalyticsEventTypes`
- [x] 8.2 Run `dotnet test` from `backend/` — confirm all tests pass
- [x] 8.3 Confirm `dotnet ef migrations list` shows no pending migrations (other than the new `RemoveAnalyticsEvents`)
