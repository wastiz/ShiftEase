using BLL.Contracts;
using BLL.DTO.ScheduleDtos;
using BLL.Helpers;
using BLL.Rules;
using DAL;
using Domain.Enums;
using DTOs.DepartmentDtos;
using DTOs.EmployeeDtos;
using DTOs.OrganizationDtos;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

/// <summary>
/// Generates employee schedules using Ant Colony Optimization.
///
/// Each ant constructs a full schedule by probabilistically assigning employees
/// to shifts. The probability is driven by pheromone trails (learned from good
/// past solutions) and a heuristic that encodes domain knowledge:
///   • primary-department employees are preferred over substitutes
///   • employees with less accumulated workload are preferred
///   • legal limits (daily rest, weekly hours, monthly norm) are enforced as
///     hard or soft constraints via LaborRulesProvider
///
/// After every iteration the pheromone matrix is updated using an elitist
/// strategy: only the global-best and the current iteration-best solutions
/// deposit pheromone, so good assignments accumulate trails faster.
/// </summary>
public class AcoScheduleGeneratorService : IAcoScheduleGeneratorService
{
    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly IOrganizationService _organizationService;
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly ILaborRulesProvider _laborRules;
    private readonly AppDbContext _context;
    // ── ACO hyperparameters ───────────────────────────────────────────────────
    private const double Alpha   = 1.0;   // pheromone exponent
    private const double Beta    = 2.5;   // heuristic exponent
    private const double Rho     = 0.15;  // evaporation rate
    private const double Q       = 500.0; // base deposit amount
    private const double TauInit = 1.0;   // initial pheromone
    private const double TauMin  = 0.01;  // pheromone floor
    private const double TauMax  = 20.0;  // pheromone ceiling

    public AcoScheduleGeneratorService(
        IOrganizationService organizationService,
        IEmployeeService employeeService,
        IDepartmentService departmentService,
        ILaborRulesProvider laborRules,
        AppDbContext context)
    {
        _organizationService = organizationService;
        _employeeService     = employeeService;
        _departmentService   = departmentService;
        _laborRules          = laborRules;
        _context             = context;
    }

    // ── Public entry point ────────────────────────────────────────────────────

    public async Task<BllScheduleGenerateResult> GenerateAcoScheduleAsync(
        int orgId,
        BllAcoScheduleGenerateRequest request)
    {
        return await GenerateAcoCoreAsync(orgId, request);
    }

    private async Task<BllScheduleGenerateResult> GenerateAcoCoreAsync(
        int orgId,
        BllAcoScheduleGenerateRequest request)
    {
        // 1. Validate inputs
        if (request.StartDate > request.EndDate)
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.InvalidDateRange);

        var org = await _organizationService.GetOrganizationByIdAsync(orgId);
        if (org == null)
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.OrganizationNotFound);

        if (org.WorkDays == null || !org.WorkDays.Any())
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoWorkDaysConfigured);

        // 2. Load departments
        var departments = await _departmentService.GetAllByOrganizationIdAsync(orgId);
        var depts       = departments.Where(d => d.ShiftTypes.Any()).ToList();
        if (!depts.Any())
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoShiftTypes);

        // 3. Load employees and split into primary / substitute pools per department
        var employees = await _employeeService.GetFullDataByOrganizationIdAsync(orgId);
        if (!employees.Any())
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoEmployeesInDepartment);

        var shiftTypeToDeptId = depts
            .SelectMany(d => d.ShiftTypes.Select(st => (st.Id, DeptId: d.Id)))
            .ToDictionary(x => x.Id, x => x.DeptId);

        var shiftTypeToPattern = depts
            .SelectMany(d => d.ShiftTypes.Select(st => (st.Id, d.DefaultSchedulePattern)))
            .ToDictionary(x => x.Id, x => x.DefaultSchedulePattern);

        // Primary: employee's home department is this department
        var primaryByDept = depts.ToDictionary(
            d => d.Id,
            d => employees.Where(e => e.PrimaryDepartmentId == d.Id).ToList());

        // Substitute: member of this department but home is elsewhere
        var substituteByDept = depts.ToDictionary(
            d => d.Id,
            d => employees
                .Where(e => e.DepartmentIds != null
                         && e.DepartmentIds.Contains(d.Id)
                         && e.PrimaryDepartmentId != d.Id)
                .ToList());

        // 4. Load time-offs (hard constraint)
        var empIds   = employees.Select(e => e.Id).ToList();
        var timeOffs = await LoadTimeOffsAsync(empIds, request.StartDate, request.EndDate);

        // 5. Build the shift skeleton (dates × shift types)
        var allShifts = BuildEmptyShifts(request.StartDate, request.EndDate, org, depts);
        if (!allShifts.Any())
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.ShiftTypesDontFitSchedule);

        // 6. Fetch labor rules
        var maxLimits   = _laborRules.GetMaxLimitsRules();
        var restPeriods = _laborRules.GetRestPeriodRules();
        var monthlyNorm = _laborRules.GetMonthlyNormHours(
            request.StartDate.Year, request.StartDate.Month);

        // 7. Run ACO
        int numAnts       = request.NumAnts       > 0 ? request.NumAnts       : 20;
        int numIterations = request.NumIterations > 0 ? request.NumIterations : 50;

        int empCount   = employees.Count;
        int shiftCount = allShifts.Count;

        // Index maps for the pheromone matrix
        var empToIdx = employees
            .Select((e, i) => (e.Id, i))
            .ToDictionary(x => x.Id, x => x.i);

        // pheromone[empIndex, shiftIndex]
        var pheromone = new double[empCount, shiftCount];
        for (int e = 0; e < empCount; e++)
            for (int s = 0; s < shiftCount; s++)
                pheromone[e, s] = TauInit;

        var rng = new Random();

        List<int>[]? globalBest        = null;
        double       globalBestFitness = double.MinValue;

        for (int iter = 0; iter < numIterations; iter++)
        {
            List<int>[]? iterBest        = null;
            double       iterBestFitness = double.MinValue;

            for (int ant = 0; ant < numAnts; ant++)
            {
                var solution = ConstructSolution(
                    allShifts, employees, empToIdx,
                    primaryByDept, substituteByDept,
                    shiftTypeToDeptId, shiftTypeToPattern,
                    timeOffs, pheromone,
                    maxLimits, restPeriods, monthlyNorm,
                    request.TotalHours, request.HardTotalHours,
                    rng);

                double fitness = EvaluateFitness(solution, allShifts, request.TotalHours, request.HardTotalHours);

                if (fitness > iterBestFitness)
                {
                    iterBestFitness = fitness;
                    iterBest        = solution;
                }

                if (fitness > globalBestFitness)
                {
                    globalBestFitness = fitness;
                    // Deep copy so it isn't mutated by later ants
                    globalBest = solution.Select(s => s.ToList()).ToArray();
                }
            }

            // Evaporate
            for (int e = 0; e < empCount; e++)
                for (int s = 0; s < shiftCount; s++)
                {
                    pheromone[e, s] = Math.Max(TauMin, pheromone[e, s] * (1.0 - Rho));
                }

            // Elitist deposit: global best (stronger) + iteration best
            if (globalBest != null)
                DepositPheromone(pheromone, globalBest, empToIdx, shiftCount, Q * 1.5);

            if (iterBest != null)
                DepositPheromone(pheromone, iterBest, empToIdx, shiftCount, Q);

            // Clamp ceiling
            for (int e = 0; e < empCount; e++)
                for (int s = 0; s < shiftCount; s++)
                    if (pheromone[e, s] > TauMax) pheromone[e, s] = TauMax;
        }

        if (globalBest == null)
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoEmployeesInDepartment);

        // 8. Apply the best solution to shifts and collect warnings
        return ApplySolutionAndBuildResult(globalBest, allShifts, employees, monthlyNorm, request.TotalHours);
    }

    // ── Solution construction ─────────────────────────────────────────────────

    /// <summary>
    /// One ant builds a complete schedule by visiting each shift in order and
    /// probabilistically selecting employees from the eligible pools.
    /// </summary>
    private List<int>[] ConstructSolution(
        List<BllShift>                     shifts,
        List<BllEmployee>                  employees,
        Dictionary<int, int>               empToIdx,
        Dictionary<int, List<BllEmployee>> primaryByDept,
        Dictionary<int, List<BllEmployee>> substituteByDept,
        Dictionary<int, int>               shiftTypeToDeptId,
        Dictionary<int, SchedulePattern>   shiftTypeToPattern,
        Dictionary<int, List<DateRange>>   timeOffs,
        double[,]                          pheromone,
        MaxLimitsRules                     maxLimits,
        RestPeriodsRules                   restPeriods,
        int                                monthlyNorm,
        double?                            totalBudget,
        bool                               hardTotalHours,
        Random                             rng)
    {
        var workloads  = employees.ToDictionary(e => e.Id, _ => new AcoWorkload());
        var empRates   = employees.ToDictionary(e => e.Id, e => (double)e.EmploymentRate);
        var usedBudget = 0.0;

        var solution = new List<int>[shifts.Count];
        for (int i = 0; i < shifts.Count; i++)
            solution[i] = new List<int>();

        for (int si = 0; si < shifts.Count; si++)
        {
            var shift         = shifts[si];
            var shiftDuration = CalcDuration(shift);
            var deptId        = shiftTypeToDeptId[shift.ShiftTypeId];
            var pattern       = shiftTypeToPattern.GetValueOrDefault(shift.ShiftTypeId, SchedulePattern.Flexible);

            var primary    = primaryByDept.GetValueOrDefault(deptId)    ?? new List<BllEmployee>();
            var substitute = substituteByDept.GetValueOrDefault(deptId) ?? new List<BllEmployee>();

            // Compute how many slots this shift may absorb given the remaining budget
            int slotsForShift = shift.MaxEmployees;
            if (totalBudget.HasValue && shiftDuration > 0)
            {
                double remaining = totalBudget.Value - usedBudget;
                int canAfford    = Math.Max(0, (int)(remaining / shiftDuration));
                slotsForShift    = hardTotalHours
                    ? Math.Min(shift.MaxEmployees, canAfford)
                    : Math.Min(shift.MaxEmployees, Math.Max(shift.MinEmployees, canAfford));
            }

            var alreadyAssigned = new HashSet<int>();

            // Phase 1 — primary employees (home department)
            SelectFromPool(
                primary, shift, si, shiftDuration, pattern,
                timeOffs, workloads, empRates,
                pheromone, empToIdx,
                maxLimits, restPeriods, monthlyNorm,
                alreadyAssigned, solution[si],
                slotsToFill: slotsForShift,
                rng, isPrimary: true);

            // Phase 2 — substitutes fill remaining slots
            int remaining2 = slotsForShift - solution[si].Count;
            if (remaining2 > 0)
            {
                SelectFromPool(
                    substitute, shift, si, shiftDuration, pattern,
                    timeOffs, workloads, empRates,
                    pheromone, empToIdx,
                    maxLimits, restPeriods, monthlyNorm,
                    alreadyAssigned, solution[si],
                    slotsToFill: remaining2,
                    rng, isPrimary: false);
            }

            // Update workload state for every employee assigned to this shift
            foreach (var empId in solution[si])
                UpdateWorkload(workloads[empId], shift, shiftDuration);

            if (totalBudget.HasValue)
                usedBudget += solution[si].Count * shiftDuration;
        }

        return solution;
    }

    /// <summary>
    /// Roulette-wheel selection: picks up to <paramref name="slotsToFill"/> employees
    /// from <paramref name="pool"/> using pheromone × heuristic probabilities.
    /// </summary>
    private void SelectFromPool(
        List<BllEmployee> pool,
        BllShift shift,
        int shiftIdx,
        double shiftDuration,
        SchedulePattern pattern,
        Dictionary<int, List<DateRange>> timeOffs,
        Dictionary<int, AcoWorkload> workloads,
        Dictionary<int, double> empRates,
        double[,] pheromone,
        Dictionary<int, int> empToIdx,
        MaxLimitsRules maxLimits,
        RestPeriodsRules restPeriods,
        int monthlyNorm,
        HashSet<int> alreadyAssigned,
        List<int> shiftAssigned,
        int slotsToFill,
        Random rng,
        bool isPrimary)
    {
        // Build scored candidate list (only eligible employees)
        var candidates = new List<(int EmpId, double Score)>();

        foreach (var emp in pool)
        {
            if (alreadyAssigned.Contains(emp.Id)) continue;

            double eta = ComputeHeuristic(
                emp, shift, shiftDuration, workloads[emp.Id], empRates[emp.Id],
                pattern, timeOffs, maxLimits, restPeriods, monthlyNorm, isPrimary);

            if (eta <= 0) continue;

            double tau   = pheromone[empToIdx[emp.Id], shiftIdx];
            double score = Math.Pow(tau, Alpha) * Math.Pow(eta, Beta);
            candidates.Add((emp.Id, score));
        }

        // Roulette-wheel without replacement until slots are filled
        double totalScore = candidates.Sum(c => c.Score);

        for (int slot = 0; slot < slotsToFill && candidates.Count > 0; slot++)
        {
            if (totalScore <= 0) break;

            double spin       = rng.NextDouble() * totalScore;
            double cumulative = 0;
            int    chosen     = candidates.Count - 1;

            for (int i = 0; i < candidates.Count; i++)
            {
                cumulative += candidates[i].Score;
                if (spin <= cumulative) { chosen = i; break; }
            }

            var (empId, chosenScore) = candidates[chosen];
            shiftAssigned.Add(empId);
            alreadyAssigned.Add(empId);
            totalScore -= chosenScore;
            candidates.RemoveAt(chosen);
        }
    }

    // ── Heuristic η(employee, shift) ─────────────────────────────────────────

    /// <summary>
    /// Returns a desirability score for assigning <paramref name="emp"/> to
    /// <paramref name="shift"/>.  Returns 0 when a hard constraint would be
    /// violated (the employee is excluded from the roulette wheel entirely).
    /// </summary>
    private double ComputeHeuristic(
        BllEmployee emp,
        BllShift shift,
        double shiftDuration,
        AcoWorkload wl,
        double rate,
        SchedulePattern pattern,
        Dictionary<int, List<DateRange>> timeOffs,
        MaxLimitsRules maxLimits,
        RestPeriodsRules restPeriods,
        int monthlyNorm,
        bool isPrimary)
    {
        // ── Hard constraints (return 0 → excluded) ────────────────────────────

        // Time-off (vacation / sick / personal day)
        if (IsOnTimeOff(emp.Id, shift.Date, timeOffs))
            return 0;

        // Minimum daily rest between consecutive days
        if (wl.LastShiftDate.HasValue && wl.LastShiftDate.Value.AddDays(1) == shift.Date)
        {
            double rest = CalcRest(wl.LastShiftEndTime, shift.StartTime);
            if (rest < restPeriods.MinDailyRestHours)
                return 0;
        }

        // Weekly hours legal ceiling (MaxHoursPerWeekAverage from labor rules)
        double weeklyMax = maxLimits.MaxHoursPerWeekAverage * rate;
        if (IsSameWeek(shift.Date, wl.LastShiftDate))
        {
            if (wl.WeeklyHours + shiftDuration > weeklyMax)
                return 0;
        }

        // Monthly norm cap at 120 % (overtime beyond this point is excluded)
        double normCap = monthlyNorm * rate;
        if (wl.TotalHours + shiftDuration > normCap * 1.2)
            return 0;

        // Schedule pattern (2on2off etc.) – treated as a hard constraint so
        // the ACO only builds pattern-conforming solutions from the start
        if (pattern != SchedulePattern.Flexible && !MatchesPattern(wl, shift.Date, pattern))
            return 0;

        // ── Soft scoring ──────────────────────────────────────────────────────

        // Primary department employees are strongly preferred
        double eta = isPrimary ? 10.0 : 3.5;

        // Reward employees with lower accumulated workload (fair distribution)
        double workloadRatio = normCap > 0 ? wl.TotalHours / normCap : 0;
        eta *= Math.Max(0.1, 1.0 - workloadRatio * 0.75);

        // Soft penalty when approaching monthly norm (100–120 %)
        if (wl.TotalHours + shiftDuration > normCap)
            eta *= 0.2;

        // Soft penalty when weekly hours are above 85 % of the legal ceiling
        if (IsSameWeek(shift.Date, wl.LastShiftDate))
        {
            double weeklyRatio = weeklyMax > 0
                ? (wl.WeeklyHours + shiftDuration) / weeklyMax
                : 0;
            if (weeklyRatio > 0.85) eta *= 0.4;
        }

        return eta;
    }

    // ── Fitness function ──────────────────────────────────────────────────────

    /// <summary>
    /// Scores a complete ant solution.
    /// Higher is better.  The main objectives are:
    ///   • maximise coverage: penalise understaffed and overstaffed shifts via
    ///     <see cref="CoverageHelper.ShiftCoveragePenalty"/> (mirrors the
    ///     frontend computeCoverage logic — understaffed is heavily penalised
    ///     so the algorithm prioritises fixing uncovered days when employees
    ///     are available)
    ///   • reward filling slots
    ///   • minimise workload variance (fairness)
    /// </summary>
    private static double EvaluateFitness(
        List<int>[] solution, List<BllShift> shifts,
        double? totalBudget = null, bool hardTotalHours = true)
    {
        double score    = 0;
        var    empHours = new Dictionary<int, double>();

        for (int si = 0; si < shifts.Count; si++)
        {
            var shift    = shifts[si];
            var assigned = solution[si];
            double dur   = CalcDuration(shift);

            // Reward every filled slot
            score += assigned.Count * 15.0;

            // Coverage penalty: understaffed (−200/missing) or overstaffed (−50/excess)
            score -= CoverageHelper.ShiftCoveragePenalty(
                assigned.Count, shift.MinEmployees, shift.MaxEmployees);

            foreach (var empId in assigned)
            {
                empHours.TryAdd(empId, 0);
                empHours[empId] += dur;
            }
        }

        // Penalise workload imbalance (standard deviation of hours)
        if (empHours.Count > 1)
        {
            double mean     = empHours.Values.Average();
            double variance = empHours.Values.Select(h => Math.Pow(h - mean, 2)).Average();
            score -= Math.Sqrt(variance) * 1.5;
        }

        // Penalise budget violations
        if (totalBudget.HasValue)
        {
            double totalUsed  = empHours.Values.Sum();
            double overBudget = Math.Max(0, totalUsed - totalBudget.Value);
            if (overBudget > 0)
                score -= overBudget * (hardTotalHours ? 500.0 : 20.0);
        }

        return score;
    }

    // ── Pheromone deposit ─────────────────────────────────────────────────────

    private void DepositPheromone(
        double[,] pheromone,
        List<int>[] solution,
        Dictionary<int, int> empToIdx,
        int shiftCount,
        double depositPerAssignment)
    {
        for (int si = 0; si < shiftCount; si++)
            foreach (var empId in solution[si])
                pheromone[empToIdx[empId], si] += depositPerAssignment;
    }

    // ── Result assembly ───────────────────────────────────────────────────────

    private BllScheduleGenerateResult ApplySolutionAndBuildResult(
        List<int>[]       solution,
        List<BllShift>    allShifts,
        List<BllEmployee> employees,
        int               monthlyNorm,
        double?           totalBudget = null)
    {
        var warnings  = new List<GenerateWarningCode>();
        var empDict   = employees.ToDictionary(e => e.Id);
        var empRates  = employees.ToDictionary(e => e.Id, e => (double)e.EmploymentRate);
        var empHours  = employees.ToDictionary(e => e.Id, _ => 0.0);
        var underMin  = false;

        for (int si = 0; si < allShifts.Count; si++)
        {
            var shift    = allShifts[si];
            var assigned = solution[si];

            if (assigned.Count < shift.MinEmployees)
                underMin = true;

            foreach (var empId in assigned)
            {
                var emp = empDict[empId];
                shift.Employees.Add(new BllEmployeeMinData
                {
                    Id              = empId,
                    Name            = $"{emp.FirstName} {emp.LastName}",
                    DepartmentNames = emp.DepartmentNames ?? new List<string>(),
                    Position        = emp.Position
                });

                empHours[empId] += CalcDuration(shift);
            }
        }

        if (underMin)
            warnings.Add(GenerateWarningCode.NotEnoughEmployeesForMinimum);

        if (totalBudget.HasValue)
        {
            double totalUsed = empHours.Values.Sum();
            if (totalUsed > totalBudget.Value)
                warnings.Add(GenerateWarningCode.BudgetExhausted);
        }

        bool hasHighWorkload = empHours.Any(kv =>
            kv.Value > monthlyNorm * empRates.GetValueOrDefault(kv.Key, 1.0) * 1.25);

        if (hasHighWorkload)
            warnings.Add(GenerateWarningCode.HighWorkloadDetected);

        return warnings.Any()
            ? BllScheduleGenerateResult.WithWarnings(allShifts, warnings)
            : BllScheduleGenerateResult.Success(allShifts);
    }

    // ── Shift skeleton builder ────────────────────────────────────────────────

    private List<BllShift> BuildEmptyShifts(
        DateOnly start,
        DateOnly end,
        BllOrganization org,
        List<BllDepartment> depts)
    {
        var shifts = new List<BllShift>();
        var tempId = -1;
        var current = start;

        while (current <= end)
        {
            var holiday = org.Holidays?.FirstOrDefault(
                h => h.Month == current.Month && h.Day == current.Day);

            if (holiday != null && !holiday.IsShortenedDay)
            {
                current = current.AddDays(1);
                continue;
            }

            var workDay = org.WorkDays?.FirstOrDefault(wd => wd.DayOfWeek == current.DayOfWeek);
            if (workDay == null && holiday == null)
            {
                current = current.AddDays(1);
                continue;
            }

            foreach (var dept in depts)
            {
                if (dept.WorkingDays.Any() && !dept.WorkingDays.Contains(current.DayOfWeek))
                    continue;

                // Apply a time-window filter only when an explicit window exists:
                //   • the department has its own StartTime/EndTime, OR
                //   • it is a shortened holiday with defined hours.
                // On regular working days without dept-level hours the shift
                // templates themselves define when work happens — filtering them
                // against the org-schedule window would silently drop valid shifts
                // (e.g. an evening dept whose shifts end after the org's closing time).
                bool hasDeptHours   = dept.StartTime.HasValue && dept.EndTime.HasValue;
                bool isShortenedDay = holiday?.IsShortenedDay == true
                                      && holiday.StartTime.HasValue
                                      && holiday.EndTime.HasValue;
                bool applyWindow    = hasDeptHours || isShortenedDay;

                var (wStart, wEnd) = applyWindow
                    ? GetWorkHours(workDay, dept, holiday)
                    : (TimeSpan.Zero, TimeSpan.Zero); // unused when applyWindow == false

                foreach (var st in dept.ShiftTypes)
                {
                    if (applyWindow && !(st.StartTime >= wStart && st.EndTime <= wEnd))
                        continue;

                    shifts.Add(new BllShift
                    {
                        Id            = tempId--,
                        Date          = current,
                        ShiftTypeId   = st.Id,
                        ShiftTypeName = st.Name,
                        StartTime     = st.StartTime,
                        EndTime       = st.EndTime,
                        MinEmployees  = st.MinEmployees,
                        MaxEmployees  = st.MaxEmployees,
                        Color         = st.Color,
                        BreakDuration = st.BreakDuration,
                        Employees     = new List<BllEmployeeMinData>()
                    });
                }
            }

            current = current.AddDays(1);
        }

        return shifts;
    }

    // ── Small helpers ─────────────────────────────────────────────────────────

    private static (TimeSpan Start, TimeSpan End) GetWorkHours(
        BllWorkDay? workDay, BllDepartment dept, BllHoliday? holiday)
    {
        if (holiday?.IsShortenedDay == true && holiday.StartTime.HasValue && holiday.EndTime.HasValue)
            return (holiday.StartTime.Value, holiday.EndTime.Value);
        if (dept.StartTime.HasValue && dept.EndTime.HasValue)
            return (dept.StartTime.Value, dept.EndTime.Value);
        if (workDay != null)
            return (TimeSpan.Parse(workDay.StartTime), TimeSpan.Parse(workDay.EndTime));
        return (TimeSpan.Zero, TimeSpan.Zero);
    }

    private async Task<Dictionary<int, List<DateRange>>> LoadTimeOffsAsync(
        List<int> empIds, DateOnly start, DateOnly end)
    {
        var result = empIds.ToDictionary(id => id, _ => new List<DateRange>());

        var vacations = await _context.Vacations
            .Where(v => empIds.Contains(v.EmployeeId) && v.StartDate <= end && v.EndDate >= start)
            .ToListAsync();

        var sickLeaves = await _context.SickLeaves
            .Where(s => empIds.Contains(s.EmployeeId) && s.StartDate <= end && s.EndDate >= start)
            .ToListAsync();

        var personalDays = await _context.PersonalDays
            .Where(p => empIds.Contains(p.EmployeeId) && p.StartDate <= end && p.EndDate >= start)
            .ToListAsync();

        foreach (var v in vacations)
            result[v.EmployeeId].Add(new DateRange(v.StartDate, v.EndDate));
        foreach (var s in sickLeaves)
            result[s.EmployeeId].Add(new DateRange(s.StartDate, s.EndDate));
        foreach (var p in personalDays)
            result[p.EmployeeId].Add(new DateRange(p.StartDate, p.EndDate));

        return result;
    }

    private static bool IsOnTimeOff(int empId, DateOnly date, Dictionary<int, List<DateRange>> timeOffs) =>
        timeOffs.TryGetValue(empId, out var ranges)
        && ranges.Any(r => date >= r.Start && date <= r.End);

    private static double CalcDuration(BllShift shift)
    {
        var d = shift.EndTime - shift.StartTime;
        if (d.TotalHours < 0) d = d.Add(TimeSpan.FromHours(24));
        if (shift.BreakDuration.HasValue) d -= shift.BreakDuration.Value;
        return d.TotalHours;
    }

    private static double CalcRest(TimeSpan? lastEnd, TimeSpan nextStart)
    {
        if (!lastEnd.HasValue) return 24;
        var r = nextStart - lastEnd.Value;
        return r.TotalHours < 0 ? r.Add(TimeSpan.FromHours(24)).TotalHours : r.TotalHours;
    }

    private static bool IsSameWeek(DateOnly a, DateOnly? b)
    {
        if (!b.HasValue) return false;
        static int WeekNum(DateOnly d) =>
            (d.DayNumber - new DateOnly(d.Year, 1, 1).DayNumber) / 7;
        return WeekNum(a) == WeekNum(b.Value);
    }

    private static bool MatchesPattern(AcoWorkload wl, DateOnly date, SchedulePattern pattern)
    {
        if (wl.LastShiftDate == null) return true;
        int daysSince = date.DayNumber - wl.LastShiftDate.Value.DayNumber;
        return pattern switch
        {
            SchedulePattern.TwoOnTwoOff     => wl.ConsecutiveDays < 2 || daysSince >= 2,
            SchedulePattern.ThreeOnThreeOff => wl.ConsecutiveDays < 3 || daysSince >= 3,
            SchedulePattern.FourOnFourOff   => wl.ConsecutiveDays < 4 || daysSince >= 4,
            SchedulePattern.FiveOnTwoOff    => wl.ConsecutiveDays < 5 || daysSince >= 2,
            _                               => true
        };
    }

    private static void UpdateWorkload(AcoWorkload wl, BllShift shift, double hours)
    {
        wl.TotalHours += hours;

        if (IsSameWeek(shift.Date, wl.LastShiftDate))
            wl.WeeklyHours += hours;
        else
            wl.WeeklyHours = hours;

        wl.ConsecutiveDays = wl.LastShiftDate.HasValue
                          && wl.LastShiftDate.Value.AddDays(1) == shift.Date
            ? wl.ConsecutiveDays + 1
            : 1;

        wl.LastShiftDate    = shift.Date;
        wl.LastShiftEndTime = shift.EndTime;
    }

    // ── Private types ─────────────────────────────────────────────────────────

    private class AcoWorkload
    {
        public double   TotalHours      { get; set; }
        public double   WeeklyHours     { get; set; }
        public DateOnly? LastShiftDate  { get; set; }
        public TimeSpan? LastShiftEndTime { get; set; }
        public int      ConsecutiveDays { get; set; }
    }

    private record DateRange(DateOnly Start, DateOnly End);
}