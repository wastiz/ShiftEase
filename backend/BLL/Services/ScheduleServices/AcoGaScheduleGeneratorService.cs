using BLL.Contracts;
using BLL.DTO.ScheduleDtos;
using BLL.Rules;
using DAL;
using Domain.Enums;
using DTOs.DepartmentDtos;
using DTOs.EmployeeDtos;
using static BLL.Services.ScheduleGeneratorShared;

namespace BLL.Services;

/// <summary>
/// Generates employee schedules using a hybrid ACO+GA algorithm.
///
/// Phase 1 — Ant Colony Optimization (ACO):
///   Ants construct feasible schedules probabilistically using pheromone trails
///   and a domain-aware heuristic. After every iteration pheromone is updated
///   with an elitist strategy so good employee–shift assignments accumulate
///   stronger trails. This phase learns the "shape" of good solutions.
///
/// Phase 2 — Genetic Algorithm (GA) seeded from pheromone:
///   Instead of a purely random initial population, each individual is built
///   using the pheromone matrix from Phase 1 as a biased guide (same roulette-
///   wheel selection as ACO). The ACO global-best solution is also injected
///   directly into the seed population. GA then refines the solutions through
///   tournament selection, uniform crossover, and constraint-aware mutation.
/// </summary>
public class AcoGaScheduleGeneratorService : IAcoGaScheduleGeneratorService
{
    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly IOrganizationService _organizationService;
    private readonly IEmployeeService     _employeeService;
    private readonly IDepartmentService   _departmentService;
    private readonly ILaborRulesProvider  _laborRules;
    private readonly AppDbContext         _context;

    // ── ACO hyperparameters ───────────────────────────────────────────────────
    private const double Alpha   = 1.0;
    private const double Beta    = 2.5;
    private const double Rho     = 0.15;
    private const double Q       = 500.0;
    private const double TauInit = 1.0;
    private const double TauMin  = 0.01;
    private const double TauMax  = 20.0;

    // ── GA hyperparameters ────────────────────────────────────────────────────
    private const double CrossoverRate  = 0.80;
    private const double MutationRate   = 0.12;
    private const int    TournamentSize = 3;
    private const int    EliteCount     = 2;

    public AcoGaScheduleGeneratorService(
        IOrganizationService organizationService,
        IEmployeeService     employeeService,
        IDepartmentService   departmentService,
        ILaborRulesProvider  laborRules,
        AppDbContext         context
        )
    {
        _organizationService = organizationService;
        _employeeService     = employeeService;
        _departmentService   = departmentService;
        _laborRules          = laborRules;
        _context             = context;
    }

    // ── Public entry point ────────────────────────────────────────────────────

    public async Task<BllScheduleGenerateResult> GenerateAcoGaScheduleAsync(
        int orgId, BllAcoGaScheduleGenerateRequest request)
    {

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await GenerateAcoGaCoreAsync(orgId, request);
        sw.Stop();

        return result;
    }

    private async Task<BllScheduleGenerateResult> GenerateAcoGaCoreAsync(
        int orgId, BllAcoGaScheduleGenerateRequest request)
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

        // 6. Prepare shared state
        int numAnts    = request.NumAnts          > 0 ? request.NumAnts          : 20;
        int numAcoIter = request.NumAcoIterations > 0 ? request.NumAcoIterations : 30;
        int popSize    = request.PopulationSize   > 0 ? request.PopulationSize   : 50;
        int numGaGens  = request.NumGaGenerations > 0 ? request.NumGaGenerations : 80;

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

        // ── Phase 1: ACO ──────────────────────────────────────────────────────

        List<int>[]? acoBest        = null;
        double       acoBestFitness = double.MinValue;

        for (int iter = 0; iter < numAcoIter; iter++)
        {
            List<int>[]? iterBest        = null;
            double       iterBestFitness = double.MinValue;

            for (int ant = 0; ant < numAnts; ant++)
            {
                var solution = ConstructAcoSolution(
                    allShifts, employees, empToIdx,
                    primaryByDept, substituteByDept,
                    shiftTypeToDeptId, shiftTypeToPattern,
                    timeOffs, pheromone,
                    maxLimits, restPeriods, monthlyNorm,
                    request.TotalHours, request.HardTotalHours,
                    rng);

                double fitness = EvaluateFitness(solution, allShifts, request.TotalHours, request.HardTotalHours);

                if (fitness > iterBestFitness) { iterBestFitness = fitness; iterBest = solution; }
                if (fitness > acoBestFitness)
                {
                    acoBestFitness = fitness;
                    acoBest = solution.Select(s => s.ToList()).ToArray();
                }
            }

            for (int e = 0; e < empCount; e++)
                for (int s = 0; s < shiftCount; s++)
                    pheromone[e, s] = Math.Max(TauMin, pheromone[e, s] * (1.0 - Rho));

            if (acoBest  != null) DepositPheromone(pheromone, acoBest,  empToIdx, shiftCount, Q * 1.5);
            if (iterBest != null) DepositPheromone(pheromone, iterBest, empToIdx, shiftCount, Q);

            for (int e = 0; e < empCount; e++)
                for (int s = 0; s < shiftCount; s++)
                    if (pheromone[e, s] > TauMax) pheromone[e, s] = TauMax;
        }

        if (acoBest == null)
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoEmployeesInDepartment);

        // ── Phase 2: GA seeded from pheromone ────────────────────────────────

        var population = new List<List<int>[]>(popSize)
        {
            acoBest.Select(s => s.ToList()).ToArray()
        };

        while (population.Count < popSize)
        {
            population.Add(ConstructAcoSolution(
                allShifts, employees, empToIdx,
                primaryByDept, substituteByDept,
                shiftTypeToDeptId, shiftTypeToPattern,
                timeOffs, pheromone,
                maxLimits, restPeriods, monthlyNorm,
                request.TotalHours, request.HardTotalHours,
                rng));
        }

        List<int>[] globalBest        = acoBest.Select(s => s.ToList()).ToArray();
        double      globalBestFitness = acoBestFitness;

        for (int gen = 0; gen < numGaGens; gen++)
        {
            var scored = population
                .Select(ind => (Individual: ind, Fitness: EvaluateFitness(ind, allShifts, request.TotalHours, request.HardTotalHours)))
                .OrderByDescending(x => x.Fitness)
                .ToList();

            if (scored[0].Fitness > globalBestFitness)
            {
                globalBestFitness = scored[0].Fitness;
                globalBest        = scored[0].Individual.Select(s => s.ToList()).ToArray();
            }

            var nextGen = new List<List<int>[]>(popSize);

            for (int e = 0; e < Math.Min(EliteCount, scored.Count); e++)
                nextGen.Add(scored[e].Individual.Select(s => s.ToList()).ToArray());

            var fitnessValues = scored.Select(x => x.Fitness).ToArray();
            var individuals   = scored.Select(x => x.Individual).ToArray();

            while (nextGen.Count < popSize)
            {
                var parent1 = TournamentSelect(individuals, fitnessValues, TournamentSize, rng);
                var parent2 = TournamentSelect(individuals, fitnessValues, TournamentSize, rng);

                List<int>[] child = rng.NextDouble() < CrossoverRate
                    ? UniformCrossover(parent1, parent2, rng)
                    : parent1.Select(s => s.ToList()).ToArray();

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

        return ApplySolutionAndBuildResult(globalBest, allShifts, employees, monthlyNorm, request.TotalHours);
    }

    // ── ACO: solution construction ────────────────────────────────────────────

    private List<int>[] ConstructAcoSolution(
        List<BllShift>                           shifts,
        List<BllEmployee>                        employees,
        Dictionary<int, int>                     empToIdx,
        Dictionary<int, List<BllEmployee>>       primaryByDept,
        Dictionary<int, List<BllEmployee>>       substituteByDept,
        Dictionary<int, int>                     shiftTypeToDeptId,
        Dictionary<int, SchedulePattern>         shiftTypeToPattern,
        Dictionary<int, List<ScheduleDateRange>> timeOffs,
        double[,]                                pheromone,
        MaxLimitsRules                           maxLimits,
        RestPeriodsRules                         restPeriods,
        int                                      monthlyNorm,
        double?                                  totalBudget,
        bool                                     hardTotalHours,
        Random                                   rng)
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

            RouletteSelect(
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
                RouletteSelect(
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
    private void RouletteSelect(
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

    // ── ACO: pheromone deposit ────────────────────────────────────────────────

    private static void DepositPheromone(
        double[,]            pheromone,
        List<int>[]          solution,
        Dictionary<int, int> empToIdx,
        int                  shiftCount,
        double               deposit)
    {
        for (int si = 0; si < shiftCount; si++)
            foreach (var empId in solution[si])
                pheromone[empToIdx[empId], si] += deposit;
    }

    // ── GA: genetic operators ─────────────────────────────────────────────────

    private static List<int>[] TournamentSelect(
        List<int>[][] individuals, double[] fitnessValues, int k, Random rng)
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

    private static List<int>[] UniformCrossover(List<int>[] parent1, List<int>[] parent2, Random rng)
    {
        var child = new List<int>[parent1.Length];
        for (int i = 0; i < parent1.Length; i++)
            child[i] = rng.NextDouble() < 0.5 ? parent1[i].ToList() : parent2[i].ToList();
        return child;
    }

    private static List<int>[] Mutate(
        List<int>[]                              individual,
        List<BllShift>                           shifts,
        List<BllEmployee>                        employees,
        Dictionary<int, List<BllEmployee>>       primaryByDept,
        Dictionary<int, List<BllEmployee>>       substituteByDept,
        Dictionary<int, int>                     shiftTypeToDeptId,
        Dictionary<int, SchedulePattern>         shiftTypeToPattern,
        Dictionary<int, List<ScheduleDateRange>> timeOffs,
        MaxLimitsRules                           maxLimits,
        RestPeriodsRules                         restPeriods,
        int                                      monthlyNorm,
        double?                                  totalBudget,
        bool                                     hardTotalHours,
        Random                                   rng)
    {
        var workloads  = employees.ToDictionary(e => e.Id, _ => new ScheduleWorkload());
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

            double shiftDur   = CalcDuration(shifts[si]);
            double oldContrib = individual[si].Count * shiftDur;

            foreach (var empId in individual[si])
            {
                workloads[empId].TotalHours  = Math.Max(0, workloads[empId].TotalHours  - shiftDur);
                workloads[empId].WeeklyHours = Math.Max(0, workloads[empId].WeeklyHours - shiftDur);
            }
            if (totalBudget.HasValue)
                usedBudget -= oldContrib;

            var shift   = shifts[si];
            var deptId  = shiftTypeToDeptId[shift.ShiftTypeId];
            var pattern = shiftTypeToPattern.GetValueOrDefault(shift.ShiftTypeId, SchedulePattern.Flexible);

            var primary    = primaryByDept.GetValueOrDefault(deptId)    ?? new List<BllEmployee>();
            var substitute = substituteByDept.GetValueOrDefault(deptId) ?? new List<BllEmployee>();

            int slotsForShift = CalcSlotCount(shift, shiftDur, totalBudget, usedBudget, hardTotalHours);

            var alreadyAssigned = new HashSet<int>();
            var newAssignment   = new List<int>();

            FillSlotsRandom(
                primary, shift, shiftDur, pattern,
                timeOffs, workloads, empRates,
                maxLimits, restPeriods, monthlyNorm,
                alreadyAssigned, newAssignment,
                slotsToFill: slotsForShift,
                isPrimary: true, rng);

            int remaining = slotsForShift - newAssignment.Count;
            if (remaining > 0)
            {
                FillSlotsRandom(
                    substitute, shift, shiftDur, pattern,
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

    /// <summary>
    /// Random-shuffle fill used during mutation (no pheromone influence).
    /// </summary>
    private static void FillSlotsRandom(
        List<BllEmployee>                        pool,
        BllShift                                 shift,
        double                                   shiftDuration,
        SchedulePattern                          pattern,
        Dictionary<int, List<ScheduleDateRange>> timeOffs,
        Dictionary<int, ScheduleWorkload>        workloads,
        Dictionary<int, double>                  empRates,
        MaxLimitsRules                           maxLimits,
        RestPeriodsRules                         restPeriods,
        int                                      monthlyNorm,
        HashSet<int>                             alreadyAssigned,
        List<int>                                shiftAssigned,
        int                                      slotsToFill,
        bool                                     isPrimary,
        Random                                   rng)
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
}
