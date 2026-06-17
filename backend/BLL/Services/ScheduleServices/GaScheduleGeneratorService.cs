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
/// Generates employee schedules using a Genetic Algorithm (GA).
///
/// Each individual (chromosome) in the population represents a complete schedule:
/// an array where index i holds the list of employee IDs assigned to shift i.
///
/// Evolution pipeline per generation:
///   1. Evaluate fitness for every individual.
///   2. Preserve the top <see cref="EliteCount"/> individuals unchanged (elitism).
///   3. Fill the rest of the next generation through tournament selection,
///      uniform crossover, and random mutation.
///
/// Constraint handling mirrors the ACO implementation:
///   • Labor rules (daily rest, weekly hours, monthly norm) are enforced as
///     hard constraints during initialisation and mutation.
///   • Primary-department employees are preferred; substitutes are allowed when
///     primary slots cannot be filled (vacation/sick-leave coverage).
///   • Soft violations are penalised in the fitness function rather than
///     rejected outright, to keep the population diverse.
/// </summary>
public class GaScheduleGeneratorService : IGaScheduleGeneratorService
{
    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly IOrganizationService _organizationService;
    private readonly IEmployeeService     _employeeService;
    private readonly IDepartmentService   _departmentService;
    private readonly ILaborRulesProvider  _laborRules;
    private readonly AppDbContext         _context;
    // ── GA hyperparameters ────────────────────────────────────────────────────
    private const double CrossoverRate   = 0.80; // probability of performing crossover
    private const double MutationRate    = 0.12; // probability of mutating each shift
    private const int    TournamentSize  = 3;    // individuals competing per selection round
    private const int    EliteCount      = 2;    // best individuals copied unchanged each generation

    // ── Heuristic multipliers (same semantics as ACO) ─────────────────────────
    private const double PrimaryEta     = 10.0;
    private const double SubstituteEta  = 3.5;

    public GaScheduleGeneratorService(
        IOrganizationService organizationService,
        IEmployeeService     employeeService,
        IDepartmentService   departmentService,
        ILaborRulesProvider  laborRules,
        AppDbContext         context)
    {
        _organizationService = organizationService;
        _employeeService     = employeeService;
        _departmentService   = departmentService;
        _laborRules          = laborRules;
        _context             = context;
    }

    // ── Public entry point ────────────────────────────────────────────────────

    public async Task<BllScheduleGenerateResult> GenerateGaScheduleAsync(
        int orgId,
        BllGaScheduleGenerateRequest request)
    {
        return await GenerateGaCoreAsync(orgId, request);
    }

    private async Task<BllScheduleGenerateResult> GenerateGaCoreAsync(
        int orgId,
        BllGaScheduleGenerateRequest request)
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

        var primaryByDept = depts.ToDictionary(
            d => d.Id,
            d => employees.Where(e => e.PrimaryDepartmentId == d.Id).ToList());

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

        // 7. Run GA
        int popSize     = request.PopulationSize  > 0 ? request.PopulationSize  : 50;
        int numGens     = request.NumGenerations  > 0 ? request.NumGenerations  : 100;

        var rng = new Random();

        // Initialise population — each individual is a random-but-feasible schedule
        var population = Enumerable.Range(0, popSize)
            .Select(_ => BuildRandomIndividual(
                allShifts, employees,
                primaryByDept, substituteByDept,
                shiftTypeToDeptId, shiftTypeToPattern,
                timeOffs, maxLimits, restPeriods, monthlyNorm,
                request.TotalHours, request.HardTotalHours,
                rng))
            .ToList();

        List<int>[]? globalBest        = null;
        double       globalBestFitness = double.MinValue;

        for (int gen = 0; gen < numGens; gen++)
        {
            // Evaluate fitness for every individual
            var scored = population
                .Select(ind => (Individual: ind, Fitness: EvaluateFitness(ind, allShifts, request.TotalHours, request.HardTotalHours)))
                .OrderByDescending(x => x.Fitness)
                .ToList();

            // Track global best
            if (scored[0].Fitness > globalBestFitness)
            {
                globalBestFitness = scored[0].Fitness;
                globalBest        = scored[0].Individual.Select(s => s.ToList()).ToArray();
            }

            // Build next generation
            var nextGen = new List<List<int>[]>(popSize);

            // Elitism — copy top individuals unchanged
            for (int e = 0; e < Math.Min(EliteCount, scored.Count); e++)
                nextGen.Add(scored[e].Individual.Select(s => s.ToList()).ToArray());

            var fitnessValues = scored.Select(x => x.Fitness).ToArray();
            var individuals   = scored.Select(x => x.Individual).ToArray();

            while (nextGen.Count < popSize)
            {
                // Selection
                var parent1 = TournamentSelect(individuals, fitnessValues, TournamentSize, rng);
                var parent2 = TournamentSelect(individuals, fitnessValues, TournamentSize, rng);

                // Crossover
                List<int>[] child;
                if (rng.NextDouble() < CrossoverRate)
                    child = UniformCrossover(parent1, parent2, rng);
                else
                    child = parent1.Select(s => s.ToList()).ToArray();

                // Mutation
                child = Mutate(
                    child, allShifts, employees,
                    primaryByDept, substituteByDept,
                    shiftTypeToDeptId, shiftTypeToPattern,
                    timeOffs, maxLimits, restPeriods, monthlyNorm,
                    request.TotalHours, request.HardTotalHours,
                    rng);

                nextGen.Add(child);
            }

            population = nextGen;
        }

        if (globalBest == null)
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoEmployeesInDepartment);

        // 8. Apply the best solution to shifts and collect warnings
        return ApplySolutionAndBuildResult(globalBest, allShifts, employees, monthlyNorm, request.TotalHours);
    }

    // ── Individual construction ───────────────────────────────────────────────

    /// <summary>
    /// Builds one random but constraint-aware individual.
    /// For each shift, eligible employees are collected (hard constraints respected),
    /// then shuffled and assigned up to MaxEmployees slots.
    /// Primary employees are preferred by filling first; substitutes fill remaining slots.
    /// </summary>
    private List<int>[] BuildRandomIndividual(
        List<BllShift>                      shifts,
        List<BllEmployee>                   employees,
        Dictionary<int, List<BllEmployee>>  primaryByDept,
        Dictionary<int, List<BllEmployee>>  substituteByDept,
        Dictionary<int, int>                shiftTypeToDeptId,
        Dictionary<int, SchedulePattern>    shiftTypeToPattern,
        Dictionary<int, List<DateRange>>    timeOffs,
        MaxLimitsRules                      maxLimits,
        RestPeriodsRules                    restPeriods,
        int                                 monthlyNorm,
        double?                             totalBudget,
        bool                                hardTotalHours,
        Random                              rng)
    {
        var workloads  = employees.ToDictionary(e => e.Id, _ => new GaWorkload());
        var empRates   = employees.ToDictionary(e => e.Id, e => (double)e.EmploymentRate);
        var usedBudget = 0.0;

        var individual = new List<int>[shifts.Count];
        for (int i = 0; i < shifts.Count; i++)
            individual[i] = new List<int>();

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
                double rem    = totalBudget.Value - usedBudget;
                int canAfford = Math.Max(0, (int)(rem / shiftDuration));
                slotsForShift = hardTotalHours
                    ? Math.Min(shift.MaxEmployees, canAfford)
                    : Math.Min(shift.MaxEmployees, Math.Max(shift.MinEmployees, canAfford));
            }

            var alreadyAssigned = new HashSet<int>();

            // Phase 1 — primary employees (home department), randomly shuffled
            FillSlotsRandom(
                primary, shift, si, shiftDuration, pattern,
                timeOffs, workloads, empRates,
                maxLimits, restPeriods, monthlyNorm,
                alreadyAssigned, individual[si],
                slotsToFill: slotsForShift,
                isPrimary: true, rng);

            // Phase 2 — substitutes fill remaining slots
            int remaining = slotsForShift - individual[si].Count;
            if (remaining > 0)
            {
                FillSlotsRandom(
                    substitute, shift, si, shiftDuration, pattern,
                    timeOffs, workloads, empRates,
                    maxLimits, restPeriods, monthlyNorm,
                    alreadyAssigned, individual[si],
                    slotsToFill: remaining,
                    isPrimary: false, rng);
            }

            foreach (var empId in individual[si])
                UpdateWorkload(workloads[empId], shift, shiftDuration);

            if (totalBudget.HasValue)
                usedBudget += individual[si].Count * shiftDuration;
        }

        return individual;
    }

    /// <summary>
    /// Shuffles the pool, then picks eligible employees (hard constraints satisfied)
    /// until <paramref name="slotsToFill"/> are filled or the pool is exhausted.
    /// </summary>
    private void FillSlotsRandom(
        List<BllEmployee>                pool,
        BllShift                         shift,
        int                              shiftIdx,
        double                           shiftDuration,
        SchedulePattern                  pattern,
        Dictionary<int, List<DateRange>> timeOffs,
        Dictionary<int, GaWorkload>      workloads,
        Dictionary<int, double>          empRates,
        MaxLimitsRules                   maxLimits,
        RestPeriodsRules                 restPeriods,
        int                              monthlyNorm,
        HashSet<int>                     alreadyAssigned,
        List<int>                        shiftAssigned,
        int                              slotsToFill,
        bool                             isPrimary,
        Random                           rng)
    {
        var shuffled = pool.OrderBy(_ => rng.Next()).ToList();

        foreach (var emp in shuffled)
        {
            if (shiftAssigned.Count >= slotsToFill) break;
            if (alreadyAssigned.Contains(emp.Id)) continue;

            double eta = ComputeHeuristic(
                emp, shift, shiftDuration, workloads[emp.Id], empRates[emp.Id],
                pattern, timeOffs, maxLimits, restPeriods, monthlyNorm, isPrimary);

            if (eta <= 0) continue;

            shiftAssigned.Add(emp.Id);
            alreadyAssigned.Add(emp.Id);
        }
    }

    // ── Genetic operators ─────────────────────────────────────────────────────

    /// <summary>
    /// Tournament selection: randomly sample <paramref name="k"/> individuals
    /// and return the one with the highest fitness.
    /// </summary>
    private static List<int>[] TournamentSelect(
        List<int>[][] individuals,
        double[]      fitnessValues,
        int           k,
        Random        rng)
    {
        int best = rng.Next(individuals.Length);
        for (int i = 1; i < k; i++)
        {
            int candidate = rng.Next(individuals.Length);
            if (fitnessValues[candidate] > fitnessValues[best])
                best = candidate;
        }
        return individuals[best];
    }

    /// <summary>
    /// Uniform crossover: for each shift slot, take the assignment from
    /// parent1 or parent2 with equal probability.
    /// This preserves good partial solutions from either parent.
    /// </summary>
    private static List<int>[] UniformCrossover(
        List<int>[] parent1,
        List<int>[] parent2,
        Random      rng)
    {
        var child = new List<int>[parent1.Length];
        for (int i = 0; i < parent1.Length; i++)
        {
            child[i] = rng.NextDouble() < 0.5
                ? parent1[i].ToList()
                : parent2[i].ToList();
        }
        return child;
    }

    /// <summary>
    /// Mutation: with probability <see cref="MutationRate"/> per shift, re-assign
    /// that shift using the same constraint-aware random fill used during
    /// initialisation — but reset workloads from scratch to avoid invalid state.
    /// </summary>
    private List<int>[] Mutate(
        List<int>[]                         individual,
        List<BllShift>                      shifts,
        List<BllEmployee>                   employees,
        Dictionary<int, List<BllEmployee>>  primaryByDept,
        Dictionary<int, List<BllEmployee>>  substituteByDept,
        Dictionary<int, int>                shiftTypeToDeptId,
        Dictionary<int, SchedulePattern>    shiftTypeToPattern,
        Dictionary<int, List<DateRange>>    timeOffs,
        MaxLimitsRules                      maxLimits,
        RestPeriodsRules                    restPeriods,
        int                                 monthlyNorm,
        double?                             totalBudget,
        bool                                hardTotalHours,
        Random                              rng)
    {
        // Rebuild workloads from the current (pre-mutation) assignment
        var workloads  = employees.ToDictionary(e => e.Id, _ => new GaWorkload());
        var empRates   = employees.ToDictionary(e => e.Id, e => (double)e.EmploymentRate);
        var usedBudget = 0.0;

        for (int si = 0; si < shifts.Count; si++)
        {
            double dur = CalcDuration(shifts[si]);
            foreach (var empId in individual[si])
                UpdateWorkload(workloads[empId], shifts[si], dur);
            if (totalBudget.HasValue)
                usedBudget += individual[si].Count * dur;
        }

        for (int si = 0; si < shifts.Count; si++)
        {
            if (rng.NextDouble() >= MutationRate) continue;

            double shiftDur    = CalcDuration(shifts[si]);
            double oldContrib  = individual[si].Count * shiftDur;

            // Remove existing assignment for this shift from workload tracking
            foreach (var empId in individual[si])
            {
                workloads[empId].TotalHours  = Math.Max(0, workloads[empId].TotalHours  - shiftDur);
                workloads[empId].WeeklyHours = Math.Max(0, workloads[empId].WeeklyHours - shiftDur);
            }
            if (totalBudget.HasValue)
                usedBudget -= oldContrib;

            // Re-assign this shift from scratch
            var shift     = shifts[si];
            var deptId    = shiftTypeToDeptId[shift.ShiftTypeId];
            var pattern   = shiftTypeToPattern.GetValueOrDefault(shift.ShiftTypeId, SchedulePattern.Flexible);

            var primary    = primaryByDept.GetValueOrDefault(deptId)    ?? new List<BllEmployee>();
            var substitute = substituteByDept.GetValueOrDefault(deptId) ?? new List<BllEmployee>();

            // Respect budget when deciding how many slots to fill
            int slotsForShift = shift.MaxEmployees;
            if (totalBudget.HasValue && shiftDur > 0)
            {
                double rem    = totalBudget.Value - usedBudget;
                int canAfford = Math.Max(0, (int)(rem / shiftDur));
                slotsForShift = hardTotalHours
                    ? Math.Min(shift.MaxEmployees, canAfford)
                    : Math.Min(shift.MaxEmployees, Math.Max(shift.MinEmployees, canAfford));
            }

            var alreadyAssigned = new HashSet<int>();
            var newAssignment   = new List<int>();

            FillSlotsRandom(
                primary, shift, si, shiftDur, pattern,
                timeOffs, workloads, empRates,
                maxLimits, restPeriods, monthlyNorm,
                alreadyAssigned, newAssignment,
                slotsToFill: slotsForShift,
                isPrimary: true, rng);

            int remaining = slotsForShift - newAssignment.Count;
            if (remaining > 0)
            {
                FillSlotsRandom(
                    substitute, shift, si, shiftDur, pattern,
                    timeOffs, workloads, empRates,
                    maxLimits, restPeriods, monthlyNorm,
                    alreadyAssigned, newAssignment,
                    slotsToFill: remaining,
                    isPrimary: false, rng);
            }

            individual[si] = newAssignment;

            foreach (var empId in individual[si])
                UpdateWorkload(workloads[empId], shift, shiftDur);

            if (totalBudget.HasValue)
                usedBudget += individual[si].Count * shiftDur;
        }

        return individual;
    }

    // ── Heuristic η(employee, shift) — shared with initialisation & mutation ──

    private static double ComputeHeuristic(
        BllEmployee                      emp,
        BllShift                         shift,
        double                           shiftDuration,
        GaWorkload                       wl,
        double                           rate,
        SchedulePattern                  pattern,
        Dictionary<int, List<DateRange>> timeOffs,
        MaxLimitsRules                   maxLimits,
        RestPeriodsRules                 restPeriods,
        int                              monthlyNorm,
        bool                             isPrimary)
    {
        // ── Hard constraints (return 0 → excluded) ────────────────────────────

        if (IsOnTimeOff(emp.Id, shift.Date, timeOffs))
            return 0;

        if (wl.LastShiftDate.HasValue && wl.LastShiftDate.Value.AddDays(1) == shift.Date)
        {
            double rest = CalcRest(wl.LastShiftEndTime, shift.StartTime);
            if (rest < restPeriods.MinDailyRestHours)
                return 0;
        }

        double weeklyMax = maxLimits.MaxHoursPerWeekAverage * rate;
        if (IsSameWeek(shift.Date, wl.LastShiftDate))
        {
            if (wl.WeeklyHours + shiftDuration > weeklyMax)
                return 0;
        }

        double normCap = monthlyNorm * rate;
        if (wl.TotalHours + shiftDuration > normCap * 1.2)
            return 0;

        if (pattern != SchedulePattern.Flexible && !MatchesPattern(wl, shift.Date, pattern))
            return 0;

        // ── Soft scoring ──────────────────────────────────────────────────────

        double eta = isPrimary ? PrimaryEta : SubstituteEta;

        double workloadRatio = normCap > 0 ? wl.TotalHours / normCap : 0;
        eta *= Math.Max(0.1, 1.0 - workloadRatio * 0.75);

        if (wl.TotalHours + shiftDuration > normCap)
            eta *= 0.2;

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
    /// Scores one individual (chromosome).
    /// Higher is better.  Coverage is the primary objective:
    ///   • <see cref="CoverageHelper.ShiftCoveragePenalty"/> penalises
    ///     understaffed shifts heavily (−200/missing employee) and overstaffed
    ///     shifts lightly (−50/excess employee), mirroring the frontend
    ///     computeCoverage severity logic.  The heavy understaffed penalty
    ///     drives the GA to fix uncovered days whenever employees are available.
    ///   • Workload variance is penalised as a secondary objective (fairness).
    /// </summary>
    private static double EvaluateFitness(
        List<int>[] individual, List<BllShift> shifts,
        double? totalBudget = null, bool hardTotalHours = true)
    {
        double score    = 0;
        var    empHours = new Dictionary<int, double>();

        for (int si = 0; si < shifts.Count; si++)
        {
            var shift    = shifts[si];
            var assigned = individual[si];
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

    // ── Result assembly ───────────────────────────────────────────────────────

    private static BllScheduleGenerateResult ApplySolutionAndBuildResult(
        List<int>[]       solution,
        List<BllShift>    allShifts,
        List<BllEmployee> employees,
        int               monthlyNorm,
        double?           totalBudget = null)
    {
        var warnings = new List<GenerateWarningCode>();
        var empDict  = employees.ToDictionary(e => e.Id);
        var empRates = employees.ToDictionary(e => e.Id, e => (double)e.EmploymentRate);
        var empHours = employees.ToDictionary(e => e.Id, _ => 0.0);
        var underMin = false;

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

    // ── Shift skeleton builder (identical logic to ACO) ───────────────────────

    private List<BllShift> BuildEmptyShifts(
        DateOnly          start,
        DateOnly          end,
        BllOrganization   org,
        List<BllDepartment> depts)
    {
        var shifts  = new List<BllShift>();
        var tempId  = -1;
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

                bool hasDeptHours   = dept.StartTime.HasValue && dept.EndTime.HasValue;
                bool isShortenedDay = holiday?.IsShortenedDay == true
                                      && holiday.StartTime.HasValue
                                      && holiday.EndTime.HasValue;
                bool applyWindow    = hasDeptHours || isShortenedDay;

                var (wStart, wEnd) = applyWindow
                    ? GetWorkHours(workDay, dept, holiday)
                    : (TimeSpan.Zero, TimeSpan.Zero);

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

    // ── Small helpers (same as ACO) ───────────────────────────────────────────

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

    private static bool MatchesPattern(GaWorkload wl, DateOnly date, SchedulePattern pattern)
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

    private static void UpdateWorkload(GaWorkload wl, BllShift shift, double hours)
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

    private class GaWorkload
    {
        public double    TotalHours       { get; set; }
        public double    WeeklyHours      { get; set; }
        public DateOnly? LastShiftDate    { get; set; }
        public TimeSpan? LastShiftEndTime { get; set; }
        public int       ConsecutiveDays  { get; set; }
    }

    private record DateRange(DateOnly Start, DateOnly End);
}
