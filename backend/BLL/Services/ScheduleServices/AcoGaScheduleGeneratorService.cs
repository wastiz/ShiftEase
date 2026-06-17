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
/// Generates employee schedules using a hybrid ACO+GA algorithm.
///
/// The algorithm runs in two phases:
///
///   Phase 1 — Ant Colony Optimization (ACO):
///     Ants construct feasible schedules probabilistically using pheromone trails
///     and a domain-aware heuristic. After every iteration pheromone is updated
///     with an elitist strategy so good employee–shift assignments accumulate
///     stronger trails. This phase learns the "shape" of good solutions.
///
///   Phase 2 — Genetic Algorithm (GA) seeded from pheromone:
///     Instead of a purely random initial population, each individual is built
///     using the pheromone matrix from Phase 1 as a biased guide (same roulette-
///     wheel selection as ACO). The ACO global-best solution is also injected
///     directly into the seed population. GA then refines the solutions through
///     tournament selection, uniform crossover, and constraint-aware mutation
///     for the configured number of generations.
///
/// Constraint handling (identical to standalone ACO and GA implementations):
///   • Vacations, sick leaves, and personal days → hard exclusion.
///   • Minimum daily rest (11 h), weekly hours ceiling, monthly norm (120 % cap)
///     and schedule patterns (2on2off etc.) → hard constraints in heuristic.
///   • Primary-department employees preferred over substitutes (soft weighting).
///   • Workload fairness penalised in fitness (standard deviation of hours).
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
    private const double Alpha   = 1.0;   // pheromone exponent
    private const double Beta    = 2.5;   // heuristic exponent
    private const double Rho     = 0.15;  // evaporation rate
    private const double Q       = 500.0; // base deposit amount
    private const double TauInit = 1.0;   // initial pheromone
    private const double TauMin  = 0.01;  // pheromone floor
    private const double TauMax  = 20.0;  // pheromone ceiling

    // ── GA hyperparameters ────────────────────────────────────────────────────
    private const double CrossoverRate  = 0.80;
    private const double MutationRate   = 0.12;
    private const int    TournamentSize = 3;
    private const int    EliteCount     = 2;

    // ── Heuristic multipliers ─────────────────────────────────────────────────
    private const double PrimaryEta    = 10.0;
    private const double SubstituteEta = 3.5;

    public AcoGaScheduleGeneratorService(
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

    public async Task<BllScheduleGenerateResult> GenerateAcoGaScheduleAsync(
        int orgId,
        BllAcoGaScheduleGenerateRequest request)
    {
        return await GenerateAcoGaCoreAsync(orgId, request);
    }

    private async Task<BllScheduleGenerateResult> GenerateAcoGaCoreAsync(
        int orgId,
        BllAcoGaScheduleGenerateRequest request)
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

        // 4. Load time-offs (hard constraint — vacations, sick leaves, personal days)
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

        // 7. Prepare shared state
        int numAnts        = request.NumAnts          > 0 ? request.NumAnts          : 20;
        int numAcoIter     = request.NumAcoIterations  > 0 ? request.NumAcoIterations  : 30;
        int popSize        = request.PopulationSize    > 0 ? request.PopulationSize    : 50;
        int numGaGens      = request.NumGaGenerations  > 0 ? request.NumGaGenerations  : 80;

        int empCount   = employees.Count;
        int shiftCount = allShifts.Count;

        var empToIdx = employees
            .Select((e, i) => (e.Id, i))
            .ToDictionary(x => x.Id, x => x.i);

        // pheromone[empIndex, shiftIndex]
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

                if (fitness > iterBestFitness)
                {
                    iterBestFitness = fitness;
                    iterBest        = solution;
                }

                if (fitness > acoBestFitness)
                {
                    acoBestFitness = fitness;
                    acoBest        = solution.Select(s => s.ToList()).ToArray();
                }
            }

            // Evaporate
            for (int e = 0; e < empCount; e++)
                for (int s = 0; s < shiftCount; s++)
                    pheromone[e, s] = Math.Max(TauMin, pheromone[e, s] * (1.0 - Rho));

            // Elitist deposit: global-best (×1.5) + iteration-best
            if (acoBest != null)
                DepositPheromone(pheromone, acoBest, empToIdx, shiftCount, Q * 1.5);
            if (iterBest != null)
                DepositPheromone(pheromone, iterBest, empToIdx, shiftCount, Q);

            // Clamp ceiling
            for (int e = 0; e < empCount; e++)
                for (int s = 0; s < shiftCount; s++)
                    if (pheromone[e, s] > TauMax) pheromone[e, s] = TauMax;
        }

        if (acoBest == null)
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoEmployeesInDepartment);

        // ── Phase 2: GA seeded from pheromone ────────────────────────────────

        // Build initial population using pheromone-biased construction.
        // The ACO global-best solution is injected directly as the first individual
        // so the GA starts from at least one high-quality seed.
        var population = new List<List<int>[]>(popSize)
        {
            acoBest.Select(s => s.ToList()).ToArray()   // seed with ACO best
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

        List<int>[]? globalBest        = acoBest.Select(s => s.ToList()).ToArray();
        double       globalBestFitness = acoBestFitness;

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

            // Elitism
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

    /// <summary>
    /// One ant constructs a complete schedule by visiting each shift and selecting
    /// employees via roulette-wheel weighted by pheromone × heuristic.
    /// Used in both the ACO phase and the GA seeding phase.
    /// </summary>
    private List<int>[] ConstructAcoSolution(
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
        var workloads  = employees.ToDictionary(e => e.Id, _ => new Workload());
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
                double rem    = totalBudget.Value - usedBudget;
                int canAfford = Math.Max(0, (int)(rem / shiftDuration));
                slotsForShift = hardTotalHours
                    ? Math.Min(shift.MaxEmployees, canAfford)
                    : Math.Min(shift.MaxEmployees, Math.Max(shift.MinEmployees, canAfford));
            }

            var alreadyAssigned = new HashSet<int>();

            // Phase A — primary employees
            RouletteSelect(
                primary, shift, si, shiftDuration, pattern,
                timeOffs, workloads, empRates,
                pheromone, empToIdx,
                maxLimits, restPeriods, monthlyNorm,
                alreadyAssigned, solution[si],
                slotsToFill: slotsForShift,
                rng, isPrimary: true);

            // Phase B — substitutes fill remaining slots
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
        List<BllEmployee>                pool,
        BllShift                         shift,
        int                              shiftIdx,
        double                           shiftDuration,
        SchedulePattern                  pattern,
        Dictionary<int, List<DateRange>> timeOffs,
        Dictionary<int, Workload>        workloads,
        Dictionary<int, double>          empRates,
        double[,]                        pheromone,
        Dictionary<int, int>             empToIdx,
        MaxLimitsRules                   maxLimits,
        RestPeriodsRules                 restPeriods,
        int                              monthlyNorm,
        HashSet<int>                     alreadyAssigned,
        List<int>                        shiftAssigned,
        int                              slotsToFill,
        Random                           rng,
        bool                             isPrimary)
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
        double[,]          pheromone,
        List<int>[]        solution,
        Dictionary<int, int> empToIdx,
        int                shiftCount,
        double             deposit)
    {
        for (int si = 0; si < shiftCount; si++)
            foreach (var empId in solution[si])
                pheromone[empToIdx[empId], si] += deposit;
    }

    // ── GA: genetic operators ─────────────────────────────────────────────────

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

    private static List<int>[] UniformCrossover(
        List<int>[] parent1,
        List<int>[] parent2,
        Random      rng)
    {
        var child = new List<int>[parent1.Length];
        for (int i = 0; i < parent1.Length; i++)
            child[i] = rng.NextDouble() < 0.5 ? parent1[i].ToList() : parent2[i].ToList();
        return child;
    }

    /// <summary>
    /// Mutation: with probability <see cref="MutationRate"/> per shift, re-assign
    /// that shift from scratch using the constraint-aware heuristic (not pheromone —
    /// plain random shuffle, same as GA-only mutation), so mutated shifts remain valid.
    /// </summary>
    private List<int>[] Mutate(
        List<int>[]                        individual,
        List<BllShift>                     shifts,
        List<BllEmployee>                  employees,
        Dictionary<int, List<BllEmployee>> primaryByDept,
        Dictionary<int, List<BllEmployee>> substituteByDept,
        Dictionary<int, int>               shiftTypeToDeptId,
        Dictionary<int, SchedulePattern>   shiftTypeToPattern,
        Dictionary<int, List<DateRange>>   timeOffs,
        MaxLimitsRules                     maxLimits,
        RestPeriodsRules                   restPeriods,
        int                                monthlyNorm,
        double?                            totalBudget,
        bool                               hardTotalHours,
        Random                             rng)
    {
        var workloads  = employees.ToDictionary(e => e.Id, _ => new Workload());
        var empRates   = employees.ToDictionary(e => e.Id, e => (double)e.EmploymentRate);
        var usedBudget = 0.0;

        // Rebuild workload state from the current individual
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

            // Remove this shift's contribution from workload and budget tracking
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
    /// Random-shuffle fill used during mutation: shuffles the pool then picks
    /// eligible employees until slots are filled (no pheromone influence).
    /// </summary>
    private void FillSlotsRandom(
        List<BllEmployee>                pool,
        BllShift                         shift,
        double                           shiftDuration,
        SchedulePattern                  pattern,
        Dictionary<int, List<DateRange>> timeOffs,
        Dictionary<int, Workload>        workloads,
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

    // ── Shared heuristic η(employee, shift) ──────────────────────────────────

    /// <summary>
    /// Returns a desirability score. Returns 0 when a hard constraint is violated
    /// (employee is excluded from candidate selection entirely).
    /// </summary>
    private static double ComputeHeuristic(
        BllEmployee                      emp,
        BllShift                         shift,
        double                           shiftDuration,
        Workload                         wl,
        double                           rate,
        SchedulePattern                  pattern,
        Dictionary<int, List<DateRange>> timeOffs,
        MaxLimitsRules                   maxLimits,
        RestPeriodsRules                 restPeriods,
        int                              monthlyNorm,
        bool                             isPrimary)
    {
        // ── Hard constraints ──────────────────────────────────────────────────

        // Time-off: vacation, sick leave, or personal day
        if (IsOnTimeOff(emp.Id, shift.Date, timeOffs))
            return 0;

        // Minimum daily rest between consecutive days
        if (wl.LastShiftDate.HasValue && wl.LastShiftDate.Value.AddDays(1) == shift.Date)
        {
            if (CalcRest(wl.LastShiftEndTime, shift.StartTime) < restPeriods.MinDailyRestHours)
                return 0;
        }

        // Weekly hours legal ceiling (labor rules)
        double weeklyMax = maxLimits.MaxHoursPerWeekAverage * rate;
        if (IsSameWeek(shift.Date, wl.LastShiftDate) && wl.WeeklyHours + shiftDuration > weeklyMax)
            return 0;

        // Monthly norm cap at 120 %
        double normCap = monthlyNorm * rate;
        if (wl.TotalHours + shiftDuration > normCap * 1.2)
            return 0;

        // Schedule pattern (2on2off, 3on3off, 4on4off, 5on2off)
        if (pattern != SchedulePattern.Flexible && !MatchesPattern(wl, shift.Date, pattern))
            return 0;

        // ── Soft scoring ──────────────────────────────────────────────────────

        // Primary department employees are strongly preferred
        double eta = isPrimary ? PrimaryEta : SubstituteEta;

        // Reward lower accumulated workload (fair distribution)
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

    // ── Shared fitness function ───────────────────────────────────────────────

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

            score += assigned.Count * 15.0;

            score -= CoverageHelper.ShiftCoveragePenalty(
                assigned.Count, shift.MinEmployees, shift.MaxEmployees);

            foreach (var empId in assigned)
            {
                empHours.TryAdd(empId, 0);
                empHours[empId] += dur;
            }
        }

        // Penalise workload imbalance (fairness)
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

    // ── Shift skeleton builder ────────────────────────────────────────────────

    private static List<BllShift> BuildEmptyShifts(
        DateOnly            start,
        DateOnly            end,
        BllOrganization     org,
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

    private static bool MatchesPattern(Workload wl, DateOnly date, SchedulePattern pattern)
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

    private static void UpdateWorkload(Workload wl, BllShift shift, double hours)
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

    private class Workload
    {
        public double    TotalHours       { get; set; }
        public double    WeeklyHours      { get; set; }
        public DateOnly? LastShiftDate    { get; set; }
        public TimeSpan? LastShiftEndTime { get; set; }
        public int       ConsecutiveDays  { get; set; }
    }

    private record DateRange(DateOnly Start, DateOnly End);
}
