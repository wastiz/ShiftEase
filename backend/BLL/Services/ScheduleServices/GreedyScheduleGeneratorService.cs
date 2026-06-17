using BLL.DTO.ScheduleDtos;
using BLL.Contracts;
using DAL;
using Domain;
using Domain.Enums;
using DTOs;
using DTOs.EmployeeDtos;
using DTOs.DepartmentDtos;
using DTOs.OrganizationDtos;
using BLL.DTO.ScheduleDtos;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class GreedyScheduleGeneratorService : IScheduleGeneratorService
{
    private readonly IOrganizationService  _organizationService;
    private readonly IShiftTemplateService _shiftTemplateService;
    private readonly IEmployeeService      _employeeService;
    private readonly IDepartmentService    _departmentService;
    private readonly AppDbContext          _context;
    public GreedyScheduleGeneratorService(
        IOrganizationService  organizationService,
        IShiftTemplateService shiftTemplateService,
        IEmployeeService      employeeService,
        IDepartmentService    departmentService,
        AppDbContext          context)
    {
        _organizationService  = organizationService;
        _shiftTemplateService = shiftTemplateService;
        _employeeService      = employeeService;
        _departmentService    = departmentService;
        _context              = context;
    }

    public async Task<BllScheduleGenerateResult> GenerateGreedyScheduleAsync(
        int orgId,
        BllScheduleGenerateRequest request)
    {
        return await GenerateGreedyCoreAsync(orgId, request);
    }

    private async Task<BllScheduleGenerateResult> GenerateGreedyCoreAsync(
        int orgId,
        BllScheduleGenerateRequest request)
    {
        var warnings = new List<GenerateWarningCode>();

        if (request.StartDate > request.EndDate)
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.InvalidDateRange);

        var organizationData = await _organizationService.GetOrganizationByIdAsync(orgId);
        if (organizationData == null)
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.OrganizationNotFound);

        if (organizationData.WorkDays == null || !organizationData.WorkDays.Any())
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoWorkDaysConfigured);

        // Load all departments in the org with their shift types
        var departments          = await _departmentService.GetAllByOrganizationIdAsync(orgId);
        var departmentsWithShifts = departments.Where(g => g.ShiftTypes.Any()).ToList();
        if (!departmentsWithShifts.Any())
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoShiftTypes);

        // Map shiftTypeId → departmentId for assignment
        var shiftTypeToDepartmentId = departmentsWithShifts
            .SelectMany(g => g.ShiftTypes.Select(st => (ShiftTypeId: st.Id, DepartmentId: g.Id)))
            .ToDictionary(x => x.ShiftTypeId, x => x.DepartmentId);

        // Map shiftTypeId → department's schedule pattern
        var shiftTypeToDepartmentPattern = departmentsWithShifts
            .SelectMany(g => g.ShiftTypes.Select(st => (ShiftTypeId: st.Id, Pattern: g.DefaultSchedulePattern)))
            .ToDictionary(x => x.ShiftTypeId, x => x.Pattern);

        // All employees in the org with department memberships
        var employees = await _employeeService.GetFullDataByOrganizationIdAsync(orgId);
        if (!employees.Any())
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoEmployeesInDepartment);

        // departmentId → employees who belong to that department, split by primary / substitute
        var employeesByDepartment = departmentsWithShifts.ToDictionary(
            g => g.Id,
            g => employees.Where(e => e.DepartmentIds != null && e.DepartmentIds.Contains(g.Id)).ToList());

        var primaryEmployeesByDepartment = departmentsWithShifts.ToDictionary(
            g => g.Id,
            g => employees.Where(e => e.PrimaryDepartmentId == g.Id).ToList());

        var substituteEmployeesByDepartment = departmentsWithShifts.ToDictionary(
            g => g.Id,
            g => employees.Where(e => e.DepartmentIds != null
                                   && e.DepartmentIds.Contains(g.Id)
                                   && e.PrimaryDepartmentId != g.Id).ToList());

        var employeeIds = employees.Select(e => e.Id).ToList();
        var timeOffs    = await LoadTimeOffsAsync(employeeIds, request.StartDate, request.EndDate);

        var availableEmployeesCount = employees.Count(e =>
            !IsEmployeeFullyOnTimeOff(e.Id, request.StartDate, request.EndDate, timeOffs));
        if (availableEmployeesCount == 0)
            warnings.Add(GenerateWarningCode.AllEmployeesOnTimeOff);

        var workloads = InitializeWorkloads(employees);

        // Generate empty shifts for all departments on every working day
        var allShifts             = new List<BllShift>();
        var hasDaysWithoutShifts  = false;
        var hasIncompleteCoverage = false;
        var tempId                = -1;

        var currentDate = request.StartDate;
        while (currentDate <= request.EndDate)
        {
            var holiday = organizationData.Holidays.FirstOrDefault(
                h => h.Month == currentDate.Month && h.Day == currentDate.Day);
            if (holiday != null && !holiday.IsShortenedDay)
            {
                currentDate = currentDate.AddDays(1);
                continue;
            }

            var workDay = GetWorkDayForDate(currentDate, organizationData.WorkDays);
            if (workDay == null && holiday == null)
            {
                currentDate = currentDate.AddDays(1);
                continue;
            }

            var dayHasAnyShift = false;

            foreach (var department in departmentsWithShifts)
            {
                if (department.WorkingDays.Any() && !department.WorkingDays.Contains(currentDate.DayOfWeek))
                    continue;

                var (workStart, workEnd) = GetWorkingHours(workDay, department, holiday);
                var dayDepartmentShifts  = new List<BllShift>();

                foreach (var shiftType in department.ShiftTypes)
                {
                    if (IsShiftWithinDepartmentTime(shiftType, workDay, department, holiday))
                    {
                        dayDepartmentShifts.Add(new BllShift
                        {
                            Id            = tempId--,
                            Date          = currentDate,
                            ShiftTypeId   = shiftType.Id,
                            ShiftTypeName = shiftType.Name,
                            StartTime     = shiftType.StartTime,
                            EndTime       = shiftType.EndTime,
                            MinEmployees  = shiftType.MinEmployees,
                            MaxEmployees  = shiftType.MaxEmployees,
                            Color         = shiftType.Color,
                            BreakDuration = shiftType.BreakDuration,
                            Employees     = new List<BllEmployeeMinData>()
                        });
                    }
                }

                if (dayDepartmentShifts.Any())
                {
                    dayHasAnyShift = true;
                    if (!IsWorkingHoursFullyCovered(dayDepartmentShifts, workStart, workEnd))
                        hasIncompleteCoverage = true;
                    allShifts.AddRange(dayDepartmentShifts);
                }
            }

            if (!dayHasAnyShift)
                hasDaysWithoutShifts = true;

            currentDate = currentDate.AddDays(1);
        }

        if (!allShifts.Any())
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.ShiftTypesDontFitSchedule);

        if (hasDaysWithoutShifts)  warnings.Add(GenerateWarningCode.SomeDaysWithoutShifts);
        if (hasIncompleteCoverage) warnings.Add(GenerateWarningCode.NoSuitableShiftTypes);

        var assignmentResult = AssignEmployeesToShifts(
            allShifts, employees,
            primaryEmployeesByDepartment, substituteEmployeesByDepartment,
            shiftTypeToDepartmentId, shiftTypeToDepartmentPattern,
            workloads, timeOffs,
            request.TotalHours, request.HardTotalHours);

        if (assignmentResult.BudgetExhausted)
            warnings.Add(GenerateWarningCode.BudgetExhausted);

        if (assignmentResult.HasShiftsUnderMinimum)
            warnings.Add(GenerateWarningCode.NotEnoughEmployeesForMinimum);

        if (assignmentResult.HasConstraintViolations)
            warnings.Add(GenerateWarningCode.EmployeesAssignedWithConstraintViolations);

        if (assignmentResult.HasHighWorkload)
            warnings.Add(GenerateWarningCode.HighWorkloadDetected);

        return warnings.Any()
            ? BllScheduleGenerateResult.WithWarnings(allShifts, warnings)
            : BllScheduleGenerateResult.Success(allShifts);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Assignment
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Assigns employees to shifts using a greedy approach with optional budget
    /// constraint. The budget tracks total working hours (break time excluded)
    /// distributed across all employees.
    ///
    /// Hard budget: no assignment is made if it would exceed the remaining budget.
    /// Soft budget: minimum shift coverage (MinEmployees) may exceed the budget;
    ///              a BudgetExhausted warning is raised when this occurs.
    /// </summary>
    private AssignmentResult AssignEmployeesToShifts(
        List<BllShift>                      shifts,
        List<BllEmployee>                   employees,
        Dictionary<int, List<BllEmployee>>  primaryEmployeesByDepartment,
        Dictionary<int, List<BllEmployee>>  substituteEmployeesByDepartment,
        Dictionary<int, int>                shiftTypeToDepartmentId,
        Dictionary<int, SchedulePattern>    shiftTypeToDepartmentPattern,
        Dictionary<int, EmployeeWorkload>   workloads,
        Dictionary<int, List<DateRange>>    timeOffs,
        double?                             totalBudget   = null,
        bool                                hardTotalHours = true)
    {
        var sortedShifts = shifts
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToList();

        var shiftsWithConstraintViolations = 0;
        var shiftsUnderMinimum             = 0;
        var budgetExhausted                = false;
        var usedBudget                     = 0.0;

        foreach (var shift in sortedShifts)
        {
            var shiftDuration     = CalculateShiftDuration(shift);
            var departmentId      = shiftTypeToDepartmentId[shift.ShiftTypeId];
            var departmentPattern = shiftTypeToDepartmentPattern.GetValueOrDefault(shift.ShiftTypeId, SchedulePattern.Flexible);
            var primaryDepartment    = primaryEmployeesByDepartment.GetValueOrDefault(departmentId) ?? new List<BllEmployee>();
            var substituteDepartment = substituteEmployeesByDepartment.GetValueOrDefault(departmentId) ?? new List<BllEmployee>();

            // Determine the target slot count, respecting the budget if set
            int targetCount;
            if (totalBudget == null || shiftDuration <= 0)
            {
                // No budget: fill up to MaxEmployees (standard greedy behaviour)
                targetCount = shift.MaxEmployees;
            }
            else
            {
                double remaining = totalBudget.Value - usedBudget;
                int canAfford    = (int)(remaining / shiftDuration);

                if (hardTotalHours)
                {
                    // Hard: never exceed budget
                    targetCount = Math.Min(shift.MaxEmployees, Math.Max(0, canAfford));
                    if (targetCount <= 0)
                    {
                        budgetExhausted = true;
                        shiftsUnderMinimum++;
                        continue; // Cannot assign anyone to this shift
                    }
                }
                else
                {
                    // Soft: always try to meet MinEmployees even if over budget
                    targetCount = Math.Min(shift.MaxEmployees, Math.Max(shift.MinEmployees, canAfford));
                    if (canAfford < shift.MinEmployees)
                        budgetExhausted = true;
                }
            }

            // Step 1: primary candidates (employees whose home department is this one)
            var candidates = primaryDepartment
                .Where(e => CanAssignToShift(e, shift, workloads[e.Id], shiftDuration, departmentPattern, timeOffs))
                .OrderBy(e => workloads[e.Id].TotalHours)
                .Take(targetCount)
                .ToList();

            // Step 2: fill remaining slots up to targetCount from substitutes
            if (candidates.Count < targetCount)
            {
                var substituteCandidates = substituteDepartment
                    .Where(e => !candidates.Contains(e) &&
                                CanAssignToShift(e, shift, workloads[e.Id], shiftDuration, departmentPattern, timeOffs))
                    .OrderBy(e => workloads[e.Id].TotalHours)
                    .Take(targetCount - candidates.Count)
                    .ToList();
                candidates.AddRange(substituteCandidates);
            }

            // Step 3: if still below minimum — relax soft constraints (time-off remains hard)
            if (candidates.Count < shift.MinEmployees)
            {
                int relaxTarget = Math.Min(shift.MinEmployees, targetCount) - candidates.Count;
                if (relaxTarget > 0)
                {
                    var allDepartmentEmployees = primaryDepartment.Concat(substituteDepartment).Distinct().ToList();
                    var additionalCandidates = allDepartmentEmployees
                        .Where(e => !candidates.Contains(e) && !IsEmployeeOnTimeOff(e.Id, shift.Date, timeOffs))
                        .OrderBy(e => workloads[e.Id].TotalHours)
                        .Take(relaxTarget)
                        .ToList();

                    if (additionalCandidates.Any())
                        shiftsWithConstraintViolations++;

                    candidates.AddRange(additionalCandidates);
                }

                if (candidates.Count < shift.MinEmployees)
                    shiftsUnderMinimum++;
            }

            foreach (var employee in candidates)
            {
                shift.Employees.Add(new BllEmployeeMinData
                {
                    Id              = employee.Id,
                    Name            = $"{employee.FirstName} {employee.LastName}",
                    DepartmentNames = employee.DepartmentNames ?? new List<string>()
                });
                UpdateWorkload(workloads[employee.Id], shift, shiftDuration);
            }

            if (totalBudget != null)
                usedBudget += candidates.Count * shiftDuration;
        }

        var employeeRates  = employees.ToDictionary(e => e.Id, e => (double)e.EmploymentRate);
        var hasHighWorkload = workloads.Values.Any(w =>
            w.TotalHours > 160 * employeeRates.GetValueOrDefault(w.EmployeeId, 1.0));

        return new AssignmentResult
        {
            HasShiftsUnderMinimum   = shiftsUnderMinimum > 0,
            HasConstraintViolations = shiftsWithConstraintViolations > 0,
            HasHighWorkload         = hasHighWorkload,
            BudgetExhausted         = budgetExhausted
        };
    }

    private class AssignmentResult
    {
        public bool HasShiftsUnderMinimum   { get; init; }
        public bool HasConstraintViolations { get; init; }
        public bool HasHighWorkload         { get; init; }
        public bool BudgetExhausted         { get; init; }
    }

    private bool CanAssignToShift(
        BllEmployee employee,
        BllShift shift,
        EmployeeWorkload workload,
        double shiftHours,
        SchedulePattern departmentPattern,
        Dictionary<int, List<DateRange>> timeOffs)
    {
        const double MAX_MONTHLY_HOURS = 200;
        const double MAX_WEEKLY_HOURS  = 48;
        var rate = (double)employee.EmploymentRate;

        if (IsEmployeeOnTimeOff(employee.Id, shift.Date, timeOffs))
            return false;

        if (workload.TotalHours + shiftHours > MAX_MONTHLY_HOURS * rate)
            return false;

        if (IsInSameWeek(shift.Date, workload.LastShiftDate) &&
            workload.WeeklyHours + shiftHours > MAX_WEEKLY_HOURS * rate)
            return false;

        if (departmentPattern != SchedulePattern.Flexible &&
            !MatchesSchedulePattern(workload, shift.Date, departmentPattern))
            return false;

        if (workload.LastShiftDate.HasValue &&
            workload.LastShiftDate.Value.AddDays(1) == shift.Date)
        {
            var restHours = CalculateRestHours(workload.LastShiftEndTime, shift.StartTime);
            if (restHours < 11)
                return false;
        }

        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Workload helpers
    // ─────────────────────────────────────────────────────────────────────────

    private Dictionary<int, EmployeeWorkload> InitializeWorkloads(List<BllEmployee> employees)
    {
        return employees.ToDictionary(
            e => e.Id,
            e => new EmployeeWorkload
            {
                EmployeeId         = e.Id,
                TotalHours         = 0,
                ShiftsThisWeek     = 0,
                LastShiftDate      = null,
                ConsecutiveShiftDays = 0,
                WorkDaysThisWeek   = 0
            });
    }

    private void UpdateWorkload(EmployeeWorkload workload, BllShift shift, double shiftHours)
    {
        workload.TotalHours += shiftHours;

        if (IsInSameWeek(shift.Date, workload.LastShiftDate))
        {
            workload.WeeklyHours += shiftHours;
            workload.WorkDaysThisWeek++;
        }
        else
        {
            workload.WeeklyHours      = shiftHours;
            workload.WorkDaysThisWeek = 1;
        }

        if (workload.LastShiftDate.HasValue &&
            workload.LastShiftDate.Value.AddDays(1) == shift.Date)
        {
            workload.ConsecutiveShiftDays++;
        }
        else
        {
            workload.ConsecutiveShiftDays = 1;
        }

        workload.LastShiftDate    = shift.Date;
        workload.LastShiftEndTime = shift.EndTime;
    }

    private double CalculateShiftDuration(BllShift shift)
    {
        var duration = shift.EndTime - shift.StartTime;
        if (duration.TotalHours < 0)
            duration = duration.Add(TimeSpan.FromHours(24));

        if (shift.BreakDuration.HasValue)
            duration = duration.Subtract(shift.BreakDuration.Value);

        return duration.TotalHours;
    }

    private double CalculateRestHours(TimeSpan? lastEndTime, TimeSpan nextStartTime)
    {
        if (!lastEndTime.HasValue) return 24;
        var rest = nextStartTime - lastEndTime.Value;
        if (rest.TotalHours < 0)
            rest = rest.Add(TimeSpan.FromHours(24));
        return rest.TotalHours;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Calendar helpers
    // ─────────────────────────────────────────────────────────────────────────

    private (TimeSpan Start, TimeSpan End) GetWorkingHours(
        BllWorkDay? workDay, BllDepartment departmentData, BllHoliday? holiday = null)
    {
        if (holiday != null && holiday.IsShortenedDay && holiday.StartTime.HasValue && holiday.EndTime.HasValue)
            return (holiday.StartTime.Value, holiday.EndTime.Value);
        if (departmentData.StartTime.HasValue && departmentData.EndTime.HasValue)
            return (departmentData.StartTime.Value, departmentData.EndTime.Value);
        if (workDay != null)
            return (TimeSpan.Parse(workDay.StartTime), TimeSpan.Parse(workDay.EndTime));
        return (TimeSpan.Zero, TimeSpan.Zero);
    }

    private bool IsWorkingHoursFullyCovered(List<BllShift> shifts, TimeSpan workStart, TimeSpan workEnd)
    {
        if (shifts.Count == 0) return false;
        var sortedShifts = shifts.OrderBy(s => s.StartTime).ToList();
        if (sortedShifts.First().StartTime > workStart) return false;
        var currentEnd = sortedShifts.First().EndTime;
        foreach (var shift in sortedShifts.Skip(1))
        {
            if (shift.StartTime > currentEnd) return false;
            if (shift.EndTime > currentEnd)   currentEnd = shift.EndTime;
        }
        return currentEnd >= workEnd;
    }

    private BllWorkDay? GetWorkDayForDate(DateOnly date, List<BllWorkDay> workDays) =>
        workDays.FirstOrDefault(wd => wd.DayOfWeek == date.DayOfWeek);

    private bool IsShiftWithinDepartmentTime(
        BllShiftTemplate shiftTemplate, BllWorkDay? workDay,
        BllDepartment departmentData, BllHoliday? holiday = null)
    {
        var (workStart, workEnd) = GetWorkingHours(workDay, departmentData, holiday);
        return shiftTemplate.StartTime >= workStart && shiftTemplate.EndTime <= workEnd;
    }

    private bool IsInSameWeek(DateOnly date1, DateOnly? date2)
    {
        if (!date2.HasValue) return false;
        return GetWeekNumber(date1) == GetWeekNumber(date2.Value);
    }

    private int GetWeekNumber(DateOnly date)
    {
        var startOfYear = new DateOnly(date.Year, 1, 1);
        return (date.DayNumber - startOfYear.DayNumber) / 7;
    }

    private bool IsHoliday(DateOnly date, List<BllHoliday> holidays) =>
        holidays.Any(h => h.Month == date.Month && h.Day == date.Day && !h.IsShortenedDay);

    private bool MatchesSchedulePattern(EmployeeWorkload workload, DateOnly shiftDate, SchedulePattern pattern)
    {
        if (workload.LastShiftDate == null) return true;
        var daysSinceLastShift = shiftDate.DayNumber - workload.LastShiftDate.Value.DayNumber;
        return pattern switch
        {
            SchedulePattern.TwoOnTwoOff     => workload.ConsecutiveShiftDays < 2 || daysSinceLastShift >= 2,
            SchedulePattern.ThreeOnThreeOff => workload.ConsecutiveShiftDays < 3 || daysSinceLastShift >= 3,
            SchedulePattern.FourOnFourOff   => workload.ConsecutiveShiftDays < 4 || daysSinceLastShift >= 4,
            SchedulePattern.FiveOnTwoOff    => workload.ConsecutiveShiftDays < 5 || daysSinceLastShift >= 2,
            _                               => true
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Time-off helpers
    // ─────────────────────────────────────────────────────────────────────────

    private bool IsEmployeeFullyOnTimeOff(
        int employeeId, DateOnly startDate, DateOnly endDate,
        Dictionary<int, List<DateRange>> timeOffs)
    {
        if (!timeOffs.TryGetValue(employeeId, out var ranges) || ranges.Count == 0)
            return false;

        var currentDate = startDate;
        while (currentDate <= endDate)
        {
            if (!ranges.Any(r => currentDate >= r.StartDate && currentDate <= r.EndDate))
                return false;
            currentDate = currentDate.AddDays(1);
        }
        return true;
    }

    private bool IsEmployeeOnTimeOff(int employeeId, DateOnly date, Dictionary<int, List<DateRange>> timeOffs)
    {
        if (!timeOffs.ContainsKey(employeeId)) return false;
        return timeOffs[employeeId].Any(range => date >= range.StartDate && date <= range.EndDate);
    }

    private async Task<Dictionary<int, List<DateRange>>> LoadTimeOffsAsync(
        List<int> employeeIds, DateOnly startDate, DateOnly endDate)
    {
        var result = new Dictionary<int, List<DateRange>>();

        var vacations = await _context.Vacations
            .Where(v => employeeIds.Contains(v.EmployeeId) &&
                        v.StartDate <= endDate && v.EndDate >= startDate)
            .ToListAsync();

        var sickLeaves = await _context.SickLeaves
            .Where(sl => employeeIds.Contains(sl.EmployeeId) &&
                         sl.StartDate <= endDate && sl.EndDate >= startDate)
            .ToListAsync();

        var personalDays = await _context.PersonalDays
            .Where(pd => employeeIds.Contains(pd.EmployeeId) &&
                         pd.StartDate <= endDate && pd.EndDate >= startDate)
            .ToListAsync();

        foreach (var empId in employeeIds)
        {
            var ranges = new List<DateRange>();

            ranges.AddRange(vacations
                .Where(v => v.EmployeeId == empId)
                .Select(v => new DateRange { StartDate = v.StartDate, EndDate = v.EndDate }));

            ranges.AddRange(sickLeaves
                .Where(sl => sl.EmployeeId == empId)
                .Select(sl => new DateRange { StartDate = sl.StartDate, EndDate = sl.EndDate }));

            ranges.AddRange(personalDays
                .Where(pd => pd.EmployeeId == empId)
                .Select(pd => new DateRange { StartDate = pd.StartDate, EndDate = pd.EndDate }));

            result[empId] = ranges;
        }

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private types
    // ─────────────────────────────────────────────────────────────────────────

    private class DateRange
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate   { get; set; }
    }

    private class EmployeeWorkload
    {
        public int      EmployeeId           { get; set; }
        public double   TotalHours           { get; set; }
        public double   WeeklyHours          { get; set; }
        public int      ShiftsThisWeek       { get; set; }
        public DateOnly? LastShiftDate       { get; set; }
        public TimeSpan? LastShiftEndTime    { get; set; }
        public int      ConsecutiveShiftDays { get; set; }
        public int      WorkDaysThisWeek     { get; set; }
    }
}
