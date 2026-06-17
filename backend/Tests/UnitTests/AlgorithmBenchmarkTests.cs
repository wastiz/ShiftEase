using BLL.Contracts;
using BLL.DTO.ScheduleDtos;
using BLL.Rules;
using BLL.Services;
using DAL;
using Domain;
using Domain.Enums;
using DTOs;
using DTOs.DepartmentDtos;
using DTOs.EmployeeDtos;
using DTOs.OrganizationDtos;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Xunit.Abstractions;

/// <summary>
/// Benchmarks all schedule-generation algorithms on simple and complex mock data
/// and ranks results using a weighted penalty score:
///
///   score = 1000 × uncovered shifts (assigned &lt; minEmployees)
///         +  500 × labour-law violations (rest &lt; 11 h, weekly hours &gt; 48 × rate)
///         +  100 × assignments during employee absence
///         +   20 × overloaded employees (monthly hours &gt; 176 h)
///         +   10 × std-dev of monthly hours across active employees
///         +    5 × "4+ days on → 5+ days off" rhythm occurrences
///
/// NOTE: this test runs heavy iterative algorithms; expect a few minutes for the
///       complex scenario with GA-100/200 and ACO+GA-40/60/100/160.
/// </summary>
public class AlgorithmBenchmarkTests
{
    private readonly ITestOutputHelper _out;

    // Shared stateless mocks — both scenarios reuse these with different orgId keys
    private readonly Mock<IOrganizationService>  _orgMock      = new();
    private readonly Mock<IEmployeeService>       _empMock      = new();
    private readonly Mock<IDepartmentService>     _deptMock     = new();
    private readonly Mock<IShiftTemplateService>  _stMock       = new();
    private readonly Mock<ILaborRulesProvider>    _laborMock    = new();

    private static readonly DateOnly PeriodStart = new(2026, 5, 1);
    private static readonly DateOnly PeriodEnd   = new(2026, 5, 31);

    public AlgorithmBenchmarkTests(ITestOutputHelper output)
    {
        _out = output;
        SetupLaborMock();
    }

    // ── Main test ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CompareAllAlgorithms_SimpleAndComplex_PrintScoreTable()
    {
        var simpleRows = await RunScenarioAsync(
            orgId:       1,
            org:         BuildSimpleOrg(),
            depts:       BuildSimpleDepts(),
            employees:   BuildSimpleEmployees(),
            seedTimeOffs: null,
            timeOffMap:   new Dictionary<int, List<(DateOnly, DateOnly)>>());

        var complexRows = await RunScenarioAsync(
            orgId:       2,
            org:         BuildComplexOrg(),
            depts:       BuildComplexDepts(),
            employees:   BuildComplexEmployees(),
            seedTimeOffs: SeedComplexTimeOffsAsync,
            timeOffMap:   BuildComplexTimeOffMap());

        _out.WriteLine(FormatTable("SIMPLE  — 4 depts · 12 employees · no absences", simpleRows));
        _out.WriteLine(string.Empty);
        _out.WriteLine(FormatTable("COMPLEX — 12 depts · 35 employees · 7 vacations + 2 sick leaves", complexRows));
    }

    // ── Scenario runner ───────────────────────────────────────────────────────

    private async Task<List<BenchRow>> RunScenarioAsync(
        int orgId,
        BllOrganization org,
        List<BllDepartment> depts,
        List<BllEmployee> employees,
        Func<AppDbContext, Task>? seedTimeOffs,
        Dictionary<int, List<(DateOnly start, DateOnly end)>> timeOffMap)
    {
        _orgMock .Setup(x => x.GetOrganizationByIdAsync(orgId)).ReturnsAsync(org);
        _deptMock.Setup(x => x.GetAllByOrganizationIdAsync(orgId)).ReturnsAsync(depts);
        _empMock .Setup(x => x.GetFullDataByOrganizationIdAsync(orgId)).ReturnsAsync(employees);

        var dbOpts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Bench_{orgId}_{Guid.NewGuid()}")
            .Options;
        await using var db = new AppDbContext(dbOpts);
        if (seedTimeOffs != null)
            await seedTimeOffs(db);

        var greedy = new GreedyScheduleGeneratorService(_orgMock.Object, _stMock.Object, _empMock.Object, _deptMock.Object, db);
        var aco    = new AcoScheduleGeneratorService   (_orgMock.Object, _empMock.Object, _deptMock.Object, _laborMock.Object, db);
        var ga     = new GaScheduleGeneratorService    (_orgMock.Object, _empMock.Object, _deptMock.Object, _laborMock.Object, db);
        var acoGa  = new AcoGaScheduleGeneratorService (_orgMock.Object, _empMock.Object, _deptMock.Object, _laborMock.Object, db);

        var rows = new List<BenchRow>();

        async Task Run(string label, Task<BllScheduleGenerateResult> task)
        {
            var sw = Stopwatch.StartNew();
            var r  = await task;
            sw.Stop();
            rows.Add(new BenchRow(label, r.Status, CalcScore(r, employees, timeOffMap), sw.Elapsed));
        }

        await Run("Greedy",
            greedy.GenerateGreedyScheduleAsync(orgId, new BllScheduleGenerateRequest
            {
                StartDate = PeriodStart,
                EndDate   = PeriodEnd
            }));

        await Run("ACO  20 ants / 50 iter",
            aco.GenerateAcoScheduleAsync(orgId, new BllAcoScheduleGenerateRequest
            {
                StartDate    = PeriodStart,
                EndDate      = PeriodEnd,
                NumAnts      = 20,
                NumIterations = 50
            }));

        await Run("ACO  40 ants / 100 iter",
            aco.GenerateAcoScheduleAsync(orgId, new BllAcoScheduleGenerateRequest
            {
                StartDate    = PeriodStart,
                EndDate      = PeriodEnd,
                NumAnts      = 40,
                NumIterations = 100
            }));

        await Run("GA   pop 50 / 100 gen",
            ga.GenerateGaScheduleAsync(orgId, new BllGaScheduleGenerateRequest
            {
                StartDate      = PeriodStart,
                EndDate        = PeriodEnd,
                PopulationSize = 50,
                NumGenerations = 100
            }));

        await Run("GA   pop 100 / 200 gen",
            ga.GenerateGaScheduleAsync(orgId, new BllGaScheduleGenerateRequest
            {
                StartDate      = PeriodStart,
                EndDate        = PeriodEnd,
                PopulationSize = 100,
                NumGenerations = 200
            }));

        await Run("ACO+GA  20/30 ants+iter / 50/80 pop+gen",
            acoGa.GenerateAcoGaScheduleAsync(orgId, new BllAcoGaScheduleGenerateRequest
            {
                StartDate       = PeriodStart,
                EndDate         = PeriodEnd,
                NumAnts         = 20,
                NumAcoIterations = 30,
                PopulationSize  = 50,
                NumGaGenerations = 80
            }));

        await Run("ACO+GA  40/60 ants+iter / 100/160 pop+gen",
            acoGa.GenerateAcoGaScheduleAsync(orgId, new BllAcoGaScheduleGenerateRequest
            {
                StartDate       = PeriodStart,
                EndDate         = PeriodEnd,
                NumAnts         = 40,
                NumAcoIterations = 60,
                PopulationSize  = 100,
                NumGaGenerations = 160
            }));

        return rows;
    }

    // ── Scoring ───────────────────────────────────────────────────────────────

    private record ScoreBreakdown(
        int    UncoveredShifts,
        int    LaborViolations,
        int    AbsenceAssignments,
        int    OverloadedEmployees,
        double HoursStdDev,
        double UndesirableRhythm,
        int    Total);

    private static ScoreBreakdown CalcScore(
        BllScheduleGenerateResult result,
        List<BllEmployee> employees,
        Dictionary<int, List<(DateOnly start, DateOnly end)>> timeOffMap)
    {
        if (result.Status == GenerateStatus.Error)
            return new ScoreBreakdown(999, 0, 0, 0, 0, 0, 999_000);

        var shifts = result.Shifts.ToList();

        int    uncovered  = CountUncoveredShifts(shifts);
        int    laborViols = CountLaborViolations(shifts, employees);
        int    absences   = CountAbsenceAssignments(shifts, timeOffMap);
        var    hours      = ComputeMonthlyHours(shifts, employees);
        int    overloaded = hours.Count(kv => kv.Value > 176.0);
        double stdDev     = StdDev(hours.Values.Where(h => h > 0).ToList());
        double rhythm     = CountUndesirableRhythm(shifts, employees);

        int total = (int)(
            20 * uncovered  +
            500 * laborViols +
            500 * absences   +
            100 * overloaded +
            100 * stdDev     +
            150 * rhythm);

        return new ScoreBreakdown(uncovered, laborViols, absences, overloaded, stdDev, rhythm, total);
    }

    // ── Score components ──────────────────────────────────────────────────────

    private static int CountUncoveredShifts(List<BllShift> shifts) =>
        shifts.Count(s => s.Employees.Count < s.MinEmployees);

    private static int CountLaborViolations(List<BllShift> shifts, List<BllEmployee> employees)
    {
        int violations = 0;

        // Group shifts by employee
        var perEmp = new Dictionary<int, List<BllShift>>();
        foreach (var s in shifts)
            foreach (var e in s.Employees)
            {
                if (!perEmp.TryGetValue(e.Id, out var list))
                    perEmp[e.Id] = list = new List<BllShift>();
                list.Add(s);
            }

        foreach (var (empId, list) in perEmp)
        {
            var emp    = employees.FirstOrDefault(e => e.Id == empId);
            double rate = (double)(emp?.EmploymentRate ?? 1.0f);

            var sorted = list.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToList();

            // Min 11 h daily rest between consecutive shifts
            for (int i = 1; i < sorted.Count; i++)
            {
                var prevEnd   = sorted[i - 1].Date.ToDateTime(TimeOnly.FromTimeSpan(sorted[i - 1].EndTime));
                var nextStart = sorted[i].Date.ToDateTime(TimeOnly.FromTimeSpan(sorted[i].StartTime));
                double gapH   = (nextStart - prevEnd).TotalHours;
                if (gapH is >= 0 and < 11.0)
                    violations++;
            }

            // Weekly hours > 48 × employment rate
            var byWeek = sorted.GroupBy(s =>
                ISOWeek.GetWeekOfYear(s.Date.ToDateTime(TimeOnly.MinValue)));

            foreach (var week in byWeek)
                if (week.Sum(ShiftWorkHours) > 48.0 * rate)
                    violations++;
        }

        return violations;
    }

    private static int CountAbsenceAssignments(
        List<BllShift> shifts,
        Dictionary<int, List<(DateOnly start, DateOnly end)>> timeOffMap)
    {
        int count = 0;
        foreach (var s in shifts)
            foreach (var e in s.Employees)
                if (timeOffMap.TryGetValue(e.Id, out var ranges) &&
                    ranges.Any(r => s.Date >= r.start && s.Date <= r.end))
                    count++;
        return count;
    }

    private static Dictionary<int, double> ComputeMonthlyHours(
        List<BllShift> shifts,
        List<BllEmployee> employees)
    {
        var map = employees.ToDictionary(e => e.Id, _ => 0.0);
        foreach (var s in shifts)
            foreach (var e in s.Employees)
                if (map.ContainsKey(e.Id))
                    map[e.Id] += ShiftWorkHours(s);
        return map;
    }

    private static double ShiftWorkHours(BllShift s)
    {
        double raw = s.EndTime >= s.StartTime
            ? (s.EndTime - s.StartTime).TotalHours
            : (s.EndTime.Add(TimeSpan.FromHours(24)) - s.StartTime).TotalHours;
        return raw - (s.BreakDuration?.TotalHours ?? 0);
    }

    private static double StdDev(IList<double> values)
    {
        if (values.Count < 2) return 0;
        double avg      = values.Average();
        double variance = values.Sum(v => (v - avg) * (v - avg)) / values.Count;
        return Math.Sqrt(variance);
    }

    // Counts occurrences of: employee works 4+ consecutive days → rests 5+ consecutive days
    private static double CountUndesirableRhythm(List<BllShift> shifts, List<BllEmployee> employees)
    {
        var workDays = new Dictionary<int, HashSet<DateOnly>>();
        foreach (var s in shifts)
            foreach (var e in s.Employees)
            {
                if (!workDays.TryGetValue(e.Id, out var set))
                    workDays[e.Id] = set = new HashSet<DateOnly>();
                set.Add(s.Date);
            }

        double total = 0;

        foreach (var emp in employees)
        {
            var days = workDays.GetValueOrDefault(emp.Id) ?? new HashSet<DateOnly>();
            var runs = new List<(bool isWork, int length)>();
            bool? curType = null;
            int   runLen  = 0;

            for (var d = PeriodStart; d <= PeriodEnd; d = d.AddDays(1))
            {
                bool works = days.Contains(d);
                if (works == curType)
                {
                    runLen++;
                }
                else
                {
                    if (curType.HasValue) runs.Add((curType.Value, runLen));
                    curType = works;
                    runLen  = 1;
                }
            }
            if (curType.HasValue) runs.Add((curType.Value, runLen));

            // Bad cycle: 4+ work days immediately followed by 5+ rest days
            for (int i = 0; i < runs.Count - 1; i++)
                if (runs[i].isWork && runs[i].length >= 4 &&
                    !runs[i + 1].isWork && runs[i + 1].length >= 5)
                    total++;
        }

        return total;
    }

    // ── Mock data: SIMPLE scenario (orgId = 1) ────────────────────────────────
    //
    // 4 departments · 12 employees · Mon–Fri only · 1 shift type per dept
    // No absences — algorithms should produce near-zero scores.

    private static BllOrganization BuildSimpleOrg() => new()
    {
        Id               = 1,
        Name             = "Simple Corp",
        OrganizationType = "Retail",
        IsOpen24_7       = false,
        NightShiftBonus  = 0,
        HolidayBonus     = 0,
        EmployeeCount    = 12,
        Holidays         = new List<BllHoliday>
        {
            new() { HolidayName = "Spring Day", Month = 5, Day = 1, IsShortenedDay = false }
        },
        WorkDays = new List<BllWorkDay>
        {
            new() { DayOfWeek = DayOfWeek.Monday,    StartTime = "08:00", EndTime = "18:00" },
            new() { DayOfWeek = DayOfWeek.Tuesday,   StartTime = "08:00", EndTime = "18:00" },
            new() { DayOfWeek = DayOfWeek.Wednesday, StartTime = "08:00", EndTime = "18:00" },
            new() { DayOfWeek = DayOfWeek.Thursday,  StartTime = "08:00", EndTime = "18:00" },
            new() { DayOfWeek = DayOfWeek.Friday,    StartTime = "08:00", EndTime = "18:00" },
        }
    };

    private static List<BllDepartment> BuildSimpleDepts() => new()
    {
        SimpleDepart(10, "Retail A", new() { SimpleST(10, 9, 17, 1, 4, 10, 1) }),
        SimpleDepart(11, "Retail B", new() { SimpleST(11, 9, 17, 1, 3, 11, 1) }),
        SimpleDepart(12, "Retail C", new() { SimpleST(12, 9, 17, 1, 3, 12, 1) }),
        SimpleDepart(13, "Admin",    new() { SimpleST(13, 9, 17, 1, 2, 13, 1) }),
    };

    private static List<BllEmployee> BuildSimpleEmployees()
    {
        var list = new List<BllEmployee>();
        for (int i = 1; i <= 4;  i++) list.Add(SimpleEmp(i, 10));
        for (int i = 5; i <= 7;  i++) list.Add(SimpleEmp(i, 11));
        for (int i = 8; i <= 10; i++) list.Add(SimpleEmp(i, 12));
        for (int i = 11; i <= 12; i++) list.Add(SimpleEmp(i, 13));
        return list;
    }

    private static BllDepartment SimpleDepart(int id, string name, List<BllShiftTemplate> shifts) => new()
    {
        Id                     = id,
        Name                   = name,
        Color                  = "#6366f1",
        WorkingDays            = new List<DayOfWeek>(),
        DefaultSchedulePattern = SchedulePattern.Flexible,
        ShiftTypes             = shifts
    };

    private static BllShiftTemplate SimpleST(
        int id, int startH, int endH, int min, int max, int deptId, int orgId) => new()
    {
        Id             = id,
        Name           = "Day",
        StartTime      = TimeSpan.FromHours(startH),
        EndTime        = TimeSpan.FromHours(endH),
        MinEmployees   = min,
        MaxEmployees   = max,
        Color          = "#6366f1",
        BreakDuration  = TimeSpan.FromMinutes(30),
        DepartmentId   = deptId,
        OrganizationId = orgId
    };

    private static BllEmployee SimpleEmp(int id, int deptId) => new()
    {
        Id                  = id,
        FirstName           = $"Emp{id:D2}",
        LastName            = "Simple",
        Email               = $"emp{id}@simple.com",
        Position            = "Staff",
        EmploymentRate      = 1.0f,
        DepartmentIds       = new List<int> { deptId },
        PrimaryDepartmentId = deptId
    };

    // ── Mock data: COMPLEX scenario (orgId = 2) ───────────────────────────────
    //
    // 12 departments · 35 employees · open Mon–Sun
    //
    // Deliberate stress points:
    //   Cashiers (6, open daily, min 3 morning + 2 evening)
    //     → Anna (1) vacation May 4–15: exactly min coverage from remaining 5
    //   Security (4, 2on2off, open daily)
    //     → George (7) vacation May 1–10: odd count breaks even 2on2off groups
    //   Warehouse (3, Mon–Fri, min 2 morning)
    //     → Karl (11) vacation May 5–18: only 2 workers left
    //   Cleaning (2, Mon–Fri, min 1)
    //     → Nina (14) sick May 1–10: only Oscar covers alone
    //   Kitchen (4, Mon–Fri, min 2 morning + 1 afternoon)
    //     → Vera (22) vacation May 1–7: exactly 3 workers, 3 slots/day
    //   Delivery (3, Mon–Fri, min 2)
    //     → Zara (26) vacation May 11–24: exactly 2 workers left
    //   Marketing (2, Mon–Fri, min 1)
    //     → Edward (31) vacation May 5–19: only Fatima for 15 days
    //   HR (2, Mon–Fri, min 1)
    //     → Gustav (33) vacation May 12–25: only Helena for 14 days
    //   Accounting (1, Mon–Fri, min 1) ← CRITICAL: single employee
    //     → Igor (35) sick May 8–14: 5 GUARANTEED uncovered shifts (no substitute)
    //
    // Cross-department substitutes:
    //   emp 5 & 6  (primary Cashiers)      → can sub in Customer Service
    //   emp 13     (primary Warehouse)     → can sub in Delivery
    //   emp 18 & 19 (primary Cust.Service) → can sub as Cashier
    //   emp 28     (primary Delivery)      → can sub in Warehouse

    private static BllOrganization BuildComplexOrg() => new()
    {
        Id               = 2,
        Name             = "MegaStore",
        OrganizationType = "Retail",
        IsOpen24_7       = false,
        NightShiftBonus  = 5,
        HolidayBonus     = 20,
        EmployeeCount    = 35,
        Holidays         = new List<BllHoliday>
        {
            new() { HolidayName = "Spring Day", Month = 5, Day = 1, IsShortenedDay = false }
        },
        WorkDays = new List<BllWorkDay>
        {
            new() { DayOfWeek = DayOfWeek.Monday,    StartTime = "06:00", EndTime = "23:00" },
            new() { DayOfWeek = DayOfWeek.Tuesday,   StartTime = "06:00", EndTime = "23:00" },
            new() { DayOfWeek = DayOfWeek.Wednesday, StartTime = "06:00", EndTime = "23:00" },
            new() { DayOfWeek = DayOfWeek.Thursday,  StartTime = "06:00", EndTime = "23:00" },
            new() { DayOfWeek = DayOfWeek.Friday,    StartTime = "06:00", EndTime = "23:00" },
            new() { DayOfWeek = DayOfWeek.Saturday,  StartTime = "07:00", EndTime = "22:00" },
            new() { DayOfWeek = DayOfWeek.Sunday,    StartTime = "07:00", EndTime = "22:00" },
        }
    };

    private static List<BllDepartment> BuildComplexDepts()
    {
        var weekdays = new List<DayOfWeek>
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday
        };

        BllDepartment D(int id, string name, string color,
            List<DayOfWeek> days, SchedulePattern pattern,
            List<BllShiftTemplate> shifts) => new()
        {
            Id                     = id,
            Name                   = name,
            Color                  = color,
            WorkingDays            = days,
            DefaultSchedulePattern = pattern,
            ShiftTypes             = shifts
        };

        BllShiftTemplate ST(int id, string name, int sh, int eh, int min, int max, int deptId) =>
            ComplexST(id, name, sh, eh, min, max, deptId, orgId: 2);

        return new List<BllDepartment>
        {
            // ── Open all 7 days ───────────────────────────────────────────────
            D(1,  "Cashiers",        "#ef4444", new List<DayOfWeek>(), SchedulePattern.Flexible, new()
            {
                ST(101, "Cash Morning",  8, 16, 3, 6, 1),   // critical: min 3, Anna away
                ST(102, "Cash Evening", 14, 22, 2, 4, 1),
            }),
            D(2,  "Security",        "#1e40af", new List<DayOfWeek>(), SchedulePattern.TwoOnTwoOff, new()
            {
                ST(103, "Security Day",  8, 20, 1, 2, 2),   // 12 h shift
            }),

            // ── Mon–Fri only ──────────────────────────────────────────────────
            D(3,  "Warehouse",       "#d97706", weekdays, SchedulePattern.Flexible, new()
            {
                ST(104, "WH Morning",    7, 15, 2, 4, 3),   // critical: min 2, Karl away
                ST(105, "WH Afternoon", 15, 23, 1, 3, 3),
            }),
            D(4,  "Cleaning",        "#16a34a", weekdays, SchedulePattern.Flexible, new()
            {
                ST(106, "Cleaning",      6, 14, 1, 2, 4),   // Nina sick: only Oscar
            }),
            D(5,  "Customer Service","#7c3aed", weekdays, SchedulePattern.Flexible, new()
            {
                ST(107, "CS Day",        9, 17, 2, 4, 5),
            }),
            D(6,  "IT Support",      "#0891b2", weekdays, SchedulePattern.Flexible, new()
            {
                ST(108, "IT Day",        9, 18, 1, 2, 6),
            }),
            D(7,  "Kitchen",         "#b45309", weekdays, SchedulePattern.Flexible, new()
            {
                ST(109, "Kitchen AM",    7, 15, 2, 3, 7),   // Vera away: 3 workers for 3 slots/day
                ST(110, "Kitchen PM",   13, 21, 1, 2, 7),
            }),
            D(8,  "Delivery",        "#be185d", weekdays, SchedulePattern.Flexible, new()
            {
                ST(111, "Delivery AM",   8, 16, 2, 4, 8),   // Zara away: exactly 2 left
            }),
            D(9,  "Management",      "#475569", weekdays, SchedulePattern.Flexible, new()
            {
                ST(112, "Mgmt Day",      9, 18, 1, 2, 9),
            }),
            D(10, "Marketing",       "#db2777", weekdays, SchedulePattern.Flexible, new()
            {
                ST(113, "Mktg Day",      9, 18, 1, 2, 10),  // Edward away: only Fatima
            }),
            D(11, "HR",              "#0f766e", weekdays, SchedulePattern.Flexible, new()
            {
                ST(114, "HR Day",        9, 18, 1, 2, 11),  // Gustav away: only Helena
            }),
            D(12, "Accounting",      "#92400e", weekdays, SchedulePattern.Flexible, new()
            {
                ST(115, "Acct Day",      9, 18, 1, 2, 12),  // Igor sick → guaranteed uncovered
            }),
        };
    }

    private static BllShiftTemplate ComplexST(
        int id, string name, int startH, int endH,
        int min, int max, int deptId, int orgId) => new()
    {
        Id             = id,
        Name           = name,
        StartTime      = TimeSpan.FromHours(startH),
        EndTime        = TimeSpan.FromHours(endH),
        MinEmployees   = min,
        MaxEmployees   = max,
        Color          = "#6366f1",
        BreakDuration  = TimeSpan.FromMinutes(30),
        DepartmentId   = deptId,
        OrganizationId = orgId
    };

    private static List<BllEmployee> BuildComplexEmployees() =>
    [
        // Cashiers (6) — emp 1–6
        Emp( 1, "Anna",    "Saar",   1,  new[] { 1 }),           // vacation May 4–15
        Emp( 2, "Boris",   "Tamm",   1,  new[] { 1 }),
        Emp( 3, "Clara",   "Kask",   1,  new[] { 1 }),
        Emp( 4, "David",   "Mets",   1,  new[] { 1 }),
        Emp( 5, "Elena",   "Rand",   1,  new[] { 1, 5 }),        // sub in CS
        Emp( 6, "Felix",   "Lepp",   1,  new[] { 1, 5 }),        // sub in CS
        // Security (4) — emp 7–10
        Emp( 7, "George",  "Oras",   2,  new[] { 2 }),           // vacation May 1–10
        Emp( 8, "Hanna",   "Pärn",   2,  new[] { 2 }),
        Emp( 9, "Ivan",    "Nõmm",   2,  new[] { 2 }),
        Emp(10, "Julia",   "Põder",  2,  new[] { 2 }),
        // Warehouse (3) — emp 11–13
        Emp(11, "Karl",    "Kukk",   3,  new[] { 3 }),           // vacation May 5–18
        Emp(12, "Laura",   "Sepp",   3,  new[] { 3 }),
        Emp(13, "Mike",    "Uus",    3,  new[] { 3, 8 }),        // sub in Delivery
        // Cleaning (2) — emp 14–15
        Emp(14, "Nina",    "Allik",  4,  new[] { 4 }),           // sick May 1–10
        Emp(15, "Oscar",   "Jõgi",   4,  new[] { 4 }),
        // Customer Service (4) — emp 16–19
        Emp(16, "Pavel",   "Vaher",  5,  new[] { 5 }),
        Emp(17, "Quinn",   "Lepik",  5,  new[] { 5 }),
        Emp(18, "Rose",    "Aavik",  5,  new[] { 5, 1 }),        // sub as Cashier
        Emp(19, "Sam",     "Kivi",   5,  new[] { 5, 1 }),        // sub as Cashier
        // IT Support (2) — emp 20–21
        Emp(20, "Tina",    "Laas",   6,  new[] { 6 }),
        Emp(21, "Ulrich",  "Palu",   6,  new[] { 6 }),
        // Kitchen (4) — emp 22–25
        Emp(22, "Vera",    "Soo",    7,  new[] { 7 }),           // vacation May 1–7
        Emp(23, "Walter",  "Lill",   7,  new[] { 7 }),
        Emp(24, "Xena",    "Toom",   7,  new[] { 7 }),
        Emp(25, "Yuri",    "Roos",   7,  new[] { 7 }),
        // Delivery (3) — emp 26–28
        Emp(26, "Zara",    "Viks",   8,  new[] { 8 }),           // vacation May 11–24
        Emp(27, "Anton",   "Kuusk",  8,  new[] { 8 }),
        Emp(28, "Bella",   "Lumi",   8,  new[] { 8, 3 }),        // sub in Warehouse
        // Management (2) — emp 29–30
        Emp(29, "Carlos",  "Tuul",   9,  new[] { 9 }),
        Emp(30, "Diana",   "Pund",   9,  new[] { 9 }),
        // Marketing (2) — emp 31–32
        Emp(31, "Edward",  "Rauk",  10,  new[] { 10 }),          // vacation May 5–19
        Emp(32, "Fatima",  "Ilves", 10,  new[] { 10 }),
        // HR (2) — emp 33–34
        Emp(33, "Gustav",  "Vares", 11,  new[] { 11 }),          // vacation May 12–25
        Emp(34, "Helena",  "Ants",  11,  new[] { 11 }),
        // Accounting (1) — emp 35: ONLY accountant
        Emp(35, "Igor",    "Metsa", 12,  new[] { 12 }),          // sick May 8–14 → uncovered
    ];

    private static BllEmployee Emp(int id, string first, string last, int primaryDept, int[] deptIds) => new()
    {
        Id                  = id,
        FirstName           = first,
        LastName            = last,
        Email               = $"{first.ToLower()}.{last.ToLower()}@megastore.com",
        Position            = "Staff",
        EmploymentRate      = 1.0f,
        DepartmentIds       = deptIds.ToList(),
        PrimaryDepartmentId = primaryDept
    };

    private static async Task SeedComplexTimeOffsAsync(AppDbContext db)
    {
        db.Vacations.AddRange(
            // 7 vacations
            new Vacation { EmployeeId =  1, StartDate = new DateOnly(2026, 5,  4), EndDate = new DateOnly(2026, 5, 15) },
            new Vacation { EmployeeId =  7, StartDate = new DateOnly(2026, 5,  1), EndDate = new DateOnly(2026, 5, 10) },
            new Vacation { EmployeeId = 11, StartDate = new DateOnly(2026, 5,  5), EndDate = new DateOnly(2026, 5, 18) },
            new Vacation { EmployeeId = 22, StartDate = new DateOnly(2026, 5,  1), EndDate = new DateOnly(2026, 5,  7) },
            new Vacation { EmployeeId = 26, StartDate = new DateOnly(2026, 5, 11), EndDate = new DateOnly(2026, 5, 24) },
            new Vacation { EmployeeId = 31, StartDate = new DateOnly(2026, 5,  5), EndDate = new DateOnly(2026, 5, 19) },
            new Vacation { EmployeeId = 33, StartDate = new DateOnly(2026, 5, 12), EndDate = new DateOnly(2026, 5, 25) });

        db.SickLeaves.AddRange(
            // 2 sick leaves
            new SickLeave { EmployeeId = 35, StartDate = new DateOnly(2026, 5,  8), EndDate = new DateOnly(2026, 5, 14) },
            new SickLeave { EmployeeId = 14, StartDate = new DateOnly(2026, 5,  1), EndDate = new DateOnly(2026, 5, 10) });

        await db.SaveChangesAsync();
    }

    private static Dictionary<int, List<(DateOnly start, DateOnly end)>> BuildComplexTimeOffMap() =>
        new()
        {
            [ 1] = [(new DateOnly(2026, 5,  4), new DateOnly(2026, 5, 15))],
            [ 7] = [(new DateOnly(2026, 5,  1), new DateOnly(2026, 5, 10))],
            [11] = [(new DateOnly(2026, 5,  5), new DateOnly(2026, 5, 18))],
            [14] = [(new DateOnly(2026, 5,  1), new DateOnly(2026, 5, 10))],
            [22] = [(new DateOnly(2026, 5,  1), new DateOnly(2026, 5,  7))],
            [26] = [(new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 24))],
            [31] = [(new DateOnly(2026, 5,  5), new DateOnly(2026, 5, 19))],
            [33] = [(new DateOnly(2026, 5, 12), new DateOnly(2026, 5, 25))],
            [35] = [(new DateOnly(2026, 5,  8), new DateOnly(2026, 5, 14))],
        };

    // ── Output ────────────────────────────────────────────────────────────────

    private record BenchRow(
        string         Label,
        GenerateStatus Status,
        ScoreBreakdown Score,
        TimeSpan       Elapsed);

    private static string FormatTable(string title, List<BenchRow> rows)
    {
        const int LW = 42; // label column width
        var sb = new StringBuilder();

        sb.AppendLine($"┌─ {title}");
        sb.AppendLine($"│  {"Algorithm",-42} {"Status",-8} {"SCORE",6}  {"Uncov",5} {"Labor",5} {"Abs",4} {"Over",5} {"StdDev",7} {"Rhythm",7} {"Time",6}");
        sb.AppendLine($"│  {new string('─', LW + 62)}");

        foreach (var r in rows.OrderBy(r => r.Score.Total))
        {
            var s   = r.Score;
            var tag = r.Status == GenerateStatus.Error ? "ERROR" : r.Status.ToString();
            sb.AppendLine(
                $"│  {r.Label,-LW} {tag,-8} {s.Total,6}  " +
                $"{s.UncoveredShifts,5} {s.LaborViolations,5} {s.AbsenceAssignments,4} " +
                $"{s.OverloadedEmployees,5} {s.HoursStdDev,7:F1} {s.UndesirableRhythm,7:F0} " +
                $"{r.Elapsed.TotalSeconds,5:F1}s");
        }

        sb.AppendLine($"└{new string('─', LW + 64)}");
        return sb.ToString();
    }

    // ── Labor rules mock ──────────────────────────────────────────────────────

    private void SetupLaborMock()
    {
        var rules = new LaborRules
        {
            Country        = "EE",
            StandardWork   = new StandardWorkRules   { HoursPerDay = 8, HoursPerWeek = 40 },
            MaxLimits      = new MaxLimitsRules       { MaxHoursPerWeekAverage = 48, MaxShiftLengthHours = 13 },
            RestPeriods    = new RestPeriodsRules     { MinDailyRestHours = 11, MinWeeklyRestHours = 48, MinWeeklyRestHoursSummarized = 36, BreakAfterHours = 6, MinBreakDurationMinutes = 30 },
            Overtime       = new OvertimeRules        { Allowed = true, RequiresAgreement = false },
            SummarizedWorkTime = new SummarizedWorkTimeRules { Enabled = true, MaxPeriodMonths = 4, OvertimeCalculatedAtEnd = true },
            NightWork      = new NightWorkRules       { Start = TimeSpan.FromHours(22), End = TimeSpan.FromHours(6) }
        };

        _laborMock.Setup(x => x.GetLaborRules()).Returns(rules);
        _laborMock.Setup(x => x.GetMaxLimitsRules()).Returns(rules.MaxLimits);
        _laborMock.Setup(x => x.GetRestPeriodRules()).Returns(rules.RestPeriods);
        _laborMock.Setup(x => x.GetMonthlyNormHours(It.IsAny<int>(), It.IsAny<int>())).Returns(168);
        _laborMock.Setup(x => x.IsWorkingDay(It.IsAny<DateTime>()))
            .Returns<DateTime>(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday);
        _laborMock.Setup(x => x.IsHoliday(It.IsAny<DateTime>())).Returns(false);
    }
}
