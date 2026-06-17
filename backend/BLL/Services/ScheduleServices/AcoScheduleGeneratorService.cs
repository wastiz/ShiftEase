using BLL.Contracts;
using BLL.DTO.ScheduleDtos;
using BLL.Rules;
using DAL;
using Domain.Enums;
using DTOs.DepartmentDtos;
using DTOs.EmployeeDtos;
using Microsoft.EntityFrameworkCore;
using static BLL.Services.ScheduleGeneratorShared;

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
    private readonly IEmployeeService     _employeeService;
    private readonly IDepartmentService   _departmentService;
    private readonly ILaborRulesProvider  _laborRules;
    private readonly AppDbContext         _context;
    private readonly IAnalyticsService    _analytics;

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
        IEmployeeService     employeeService,
        IDepartmentService   departmentService,
        ILaborRulesProvider  laborRules,
        AppDbContext         context,
        IAnalyticsService    analytics)
    {
        _organizationService = organizationService;
        _employeeService     = employeeService;
        _departmentService   = departmentService;
        _laborRules          = laborRules;
        _context             = context;
        _analytics           = analytics;
    }

    // ── Public entry point ────────────────────────────────────────────────────

    public async Task<BllScheduleGenerateResult> GenerateAcoScheduleAsync(
        int orgId, BllAcoScheduleGenerateRequest request)
    {
        _analytics.Track(AnalyticsEventTypes.ScheduleGenerationRequested, organizationId: orgId,
            metadata: new() { ["algorithm"] = "aco" });

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await GenerateAcoCoreAsync(orgId, request);
        sw.Stop();

        if (result.Status == GenerateStatus.Error)
            _analytics.Track(AnalyticsEventTypes.ScheduleGenerationFailed, organizationId: orgId,
                metadata: new() { ["algorithm"] = "aco", ["duration_ms"] = (object?)sw.ElapsedMilliseconds, ["error"] = result.Error?.ToString() });
        else
            _analytics.Track(AnalyticsEventTypes.ScheduleGenerationSuccess, organizationId: orgId,
                metadata: new() { ["algorithm"] = "aco", ["duration_ms"] = (object?)sw.ElapsedMilliseconds, ["shift_count"] = result.Shifts.Count, ["employee_count"] = result.Shifts.SelectMany(s => s.Employees).Select(e => e.Id).Distinct().Count() });

        return result;
    }

    private async Task<BllScheduleGenerateResult> GenerateAcoCoreAsync(
        int orgId, BllAcoScheduleGenerateRequest request)
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

        // 3. Load employees and build department pools
        var employees = await _employeeService.GetFullDataByOrganizationIdAsync(orgId);
        if (!employees.Any())
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoEmployeesInDepartment);

        var (shiftTypeToDeptId, shiftTypeToPattern) = BuildShiftTypeMaps(depts);
        var (primaryByDept, substituteByDept)        = BuildEmployeePools(depts, employees);

        // 4. Load time-offs and build shift skeleton
        var empIds   = employees.Select(e => e.Id).ToList();
        var timeOffs = await LoadTimeOffsAsync(empIds, request.StartDate, request.EndDate, _context);

        var allShifts = BuildEmptyShifts(request.StartDate, request.EndDate, org, depts);
        if (!allShifts.Any())
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.ShiftTypesDontFitSchedule);

        // 5. Fetch labor rules
        var maxLimits   = _laborRules.GetMaxLimitsRules();
        var restPeriods = _laborRules.GetRestPeriodRules();
        var monthlyNorm = _laborRules.GetMonthlyNormHours(
            request.StartDate.Year, request.StartDate.Month);

        // 6. Run ACO
        int numAnts       = request.NumAnts       > 0 ? request.NumAnts       : 20;
        int numIterations = request.NumIterations > 0 ? request.NumIterations : 50;

        int empCount   = employees.Count;
        int shiftCount = allShifts.Count;

        var empToIdx = employees
            .Select((e, i) => (e.Id, i))
            .ToDictionary(x => x.Id, x => x.i);

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

                if (fitness > iterBestFitness) { iterBestFitness = fitness; iterBest = solution; }
                if (fitness > globalBestFitness)
                {
                    globalBestFitness = fitness;
                    globalBest = solution.Select(s => s.ToList()).ToArray();
                }
            }

            // Evaporate
            for (int e = 0; e < empCount; e++)
                for (int s = 0; s < shiftCount; s++)
                    pheromone[e, s] = Math.Max(TauMin, pheromone[e, s] * (1.0 - Rho));

            if (globalBest != null) DepositPheromone(pheromone, globalBest, empToIdx, shiftCount, Q * 1.5);
            if (iterBest   != null) DepositPheromone(pheromone, iterBest,   empToIdx, shiftCount, Q);

            for (int e = 0; e < empCount; e++)
                for (int s = 0; s < shiftCount; s++)
                    if (pheromone[e, s] > TauMax) pheromone[e, s] = TauMax;
        }

        if (globalBest == null)
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoEmployeesInDepartment);

        return ApplySolutionAndBuildResult(globalBest, allShifts, employees, monthlyNorm, request.TotalHours);
    }

    // ── Solution construction ─────────────────────────────────────────────────

    private List<int>[] ConstructSolution(
        List<BllShift>                            shifts,
        List<BllEmployee>                         employees,
        Dictionary<int, int>                      empToIdx,
        Dictionary<int, List<BllEmployee>>        primaryByDept,
        Dictionary<int, List<BllEmployee>>        substituteByDept,
        Dictionary<int, int>                      shiftTypeToDeptId,
        Dictionary<int, SchedulePattern>          shiftTypeToPattern,
        Dictionary<int, List<ScheduleDateRange>>  timeOffs,
        double[,]                                 pheromone,
        MaxLimitsRules                            maxLimits,
        RestPeriodsRules                          restPeriods,
        int                                       monthlyNorm,
        double?                                   totalBudget,
        bool                                      hardTotalHours,
        Random                                    rng)
    {
        var workloads  = employees.ToDictionary(e => e.Id, _ => new ScheduleWorkload());
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

            int slotsForShift = CalcSlotCount(shift, shiftDuration, totalBudget, usedBudget, hardTotalHours);

            var alreadyAssigned = new HashSet<int>();

            SelectFromPool(
                primary, shift, si, shiftDuration, pattern,
                timeOffs, workloads, empRates,
                pheromone, empToIdx,
                maxLimits, restPeriods, monthlyNorm,
                alreadyAssigned, solution[si],
                slotsToFill: slotsForShift,
                rng, isPrimary: true);

            int remaining = slotsForShift - solution[si].Count;
            if (remaining > 0)
            {
                SelectFromPool(
                    substitute, shift, si, shiftDuration, pattern,
                    timeOffs, workloads, empRates,
                    pheromone, empToIdx,
                    maxLimits, restPeriods, monthlyNorm,
                    alreadyAssigned, solution[si],
                    slotsToFill: remaining,
                    rng, isPrimary: false);
            }

            foreach (var empId in solution[si])
                UpdateWorkload(workloads[empId], shift, shiftDuration);

            if (totalBudget.HasValue)
                usedBudget += solution[si].Count * shiftDuration;
        }

        return solution;
    }

    /// <summary>
    /// Roulette-wheel without replacement using τ^α × η^β weights.
    /// </summary>
    private void SelectFromPool(
        List<BllEmployee>                        pool,
        BllShift                                 shift,
        int                                      shiftIdx,
        double                                   shiftDuration,
        SchedulePattern                          pattern,
        Dictionary<int, List<ScheduleDateRange>> timeOffs,
        Dictionary<int, ScheduleWorkload>        workloads,
        Dictionary<int, double>                  empRates,
        double[,]                                pheromone,
        Dictionary<int, int>                     empToIdx,
        MaxLimitsRules                           maxLimits,
        RestPeriodsRules                         restPeriods,
        int                                      monthlyNorm,
        HashSet<int>                             alreadyAssigned,
        List<int>                                shiftAssigned,
        int                                      slotsToFill,
        Random                                   rng,
        bool                                     isPrimary)
    {
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

    // ── Pheromone deposit ─────────────────────────────────────────────────────

    private static void DepositPheromone(
        double[,]          pheromone,
        List<int>[]        solution,
        Dictionary<int, int> empToIdx,
        int                shiftCount,
        double             depositPerAssignment)
    {
        for (int si = 0; si < shiftCount; si++)
            foreach (var empId in solution[si])
                pheromone[empToIdx[empId], si] += depositPerAssignment;
    }
}
