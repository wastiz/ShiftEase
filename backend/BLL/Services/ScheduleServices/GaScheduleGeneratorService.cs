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
    private const double CrossoverRate  = 0.80;
    private const double MutationRate   = 0.12;
    private const int    TournamentSize = 3;
    private const int    EliteCount     = 2;

    public GaScheduleGeneratorService(
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

    public async Task<BllScheduleGenerateResult> GenerateGaScheduleAsync(
        int orgId, BllGaScheduleGenerateRequest request)
    {

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await GenerateGaCoreAsync(orgId, request);
        sw.Stop();

        return result;
    }

    private async Task<BllScheduleGenerateResult> GenerateGaCoreAsync(
        int orgId, BllGaScheduleGenerateRequest request)
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

        // 6. Run GA
        int popSize = request.PopulationSize > 0 ? request.PopulationSize : 50;
        int numGens = request.NumGenerations > 0 ? request.NumGenerations : 100;

        var rng = new Random();

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

        if (globalBest == null)
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoEmployeesInDepartment);

        return ApplySolutionAndBuildResult(globalBest, allShifts, employees, monthlyNorm, request.TotalHours);
    }

    // ── Individual construction ───────────────────────────────────────────────

    private List<int>[] BuildRandomIndividual(
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

            int slotsForShift = CalcSlotCount(shift, shiftDuration, totalBudget, usedBudget, hardTotalHours);

            var alreadyAssigned = new HashSet<int>();

            FillSlotsRandom(
                primary, shift, shiftDuration, pattern,
                timeOffs, workloads, empRates,
                maxLimits, restPeriods, monthlyNorm,
                alreadyAssigned, individual[si],
                slotsToFill: slotsForShift,
                isPrimary: true, rng);

            int remaining = slotsForShift - individual[si].Count;
            if (remaining > 0)
            {
                FillSlotsRandom(
                    substitute, shift, shiftDuration, pattern,
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
    /// Shuffles the pool and picks eligible employees until slots are filled.
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

    // ── Genetic operators ─────────────────────────────────────────────────────

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
}
