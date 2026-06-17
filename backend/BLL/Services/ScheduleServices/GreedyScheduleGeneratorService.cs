using BLL.Contracts;
using BLL.DTO.ScheduleDtos;
using DAL;
using Domain.Enums;
using DTOs.DepartmentDtos;
using DTOs.EmployeeDtos;
using DTOs.OrganizationDtos;
using static BLL.Services.ScheduleGeneratorShared;

namespace BLL.Services;

public class GreedyScheduleGeneratorService : IScheduleGeneratorService
{
    private readonly IOrganizationService  _organizationService;
    private readonly IShiftTemplateService _shiftTemplateService;
    private readonly IEmployeeService      _employeeService;
    private readonly IDepartmentService    _departmentService;
    private readonly AppDbContext          _context;
    private readonly IAnalyticsService     _analytics;

    public GreedyScheduleGeneratorService(
        IOrganizationService  organizationService,
        IShiftTemplateService shiftTemplateService,
        IEmployeeService      employeeService,
        IDepartmentService    departmentService,
        AppDbContext          context,
        IAnalyticsService     analytics)
    {
        _organizationService  = organizationService;
        _shiftTemplateService = shiftTemplateService;
        _employeeService      = employeeService;
        _departmentService    = departmentService;
        _context              = context;
        _analytics            = analytics;
    }

    public async Task<BllScheduleGenerateResult> GenerateGreedyScheduleAsync(
        int orgId,
        BllScheduleGenerateRequest request)
    {
        _analytics.Track(AnalyticsEventTypes.ScheduleGenerationRequested, organizationId: orgId,
            metadata: new() { ["algorithm"] = "greedy" });

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await GenerateGreedyCoreAsync(orgId, request);
        sw.Stop();

        if (result.Status == GenerateStatus.Error)
            _analytics.Track(AnalyticsEventTypes.ScheduleGenerationFailed, organizationId: orgId,
                metadata: new() { ["algorithm"] = "greedy", ["duration_ms"] = (object?)sw.ElapsedMilliseconds, ["error"] = result.Error?.ToString() });
        else
            _analytics.Track(AnalyticsEventTypes.ScheduleGenerationSuccess, organizationId: orgId,
                metadata: new() { ["algorithm"] = "greedy", ["duration_ms"] = (object?)sw.ElapsedMilliseconds, ["shift_count"] = result.Shifts.Count, ["employee_count"] = result.Shifts.SelectMany(s => s.Employees).Select(e => e.Id).Distinct().Count() });

        return result;
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

        var departments          = await _departmentService.GetAllByOrganizationIdAsync(orgId);
        var departmentsWithShifts = departments.Where(g => g.ShiftTypes.Any()).ToList();
        if (!departmentsWithShifts.Any())
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoShiftTypes);

        var (shiftTypeToDepartmentId, shiftTypeToDepartmentPattern) = BuildShiftTypeMaps(departmentsWithShifts);

        var employees = await _employeeService.GetFullDataByOrganizationIdAsync(orgId);
        if (!employees.Any())
            return BllScheduleGenerateResult.Fail(GenerateErrorCode.NoEmployeesInDepartment);

        var (primaryEmployeesByDepartment, substituteEmployeesByDepartment) =
            BuildEmployeePools(departmentsWithShifts, employees);

        var employeeIds = employees.Select(e => e.Id).ToList();
        var timeOffs    = await LoadTimeOffsAsync(employeeIds, request.StartDate, request.EndDate, _context);

        var availableEmployeesCount = employees.Count(e =>
            !IsEmployeeFullyOnTimeOff(e.Id, request.StartDate, request.EndDate, timeOffs));
        if (availableEmployeesCount == 0)
            warnings.Add(GenerateWarningCode.AllEmployeesOnTimeOff);

        var workloads = employees.ToDictionary(e => e.Id, _ => new ScheduleWorkload());

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

            var workDay = organizationData.WorkDays.FirstOrDefault(wd => wd.DayOfWeek == currentDate.DayOfWeek);
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

                var (workStart, workEnd) = GetWorkHours(workDay, department, holiday);
                var dayDepartmentShifts  = new List<BllShift>();

                foreach (var shiftType in department.ShiftTypes)
                {
                    if (shiftType.StartTime >= workStart && shiftType.EndTime <= workEnd)
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

        if (assignmentResult.BudgetExhausted)            warnings.Add(GenerateWarningCode.BudgetExhausted);
        if (assignmentResult.HasShiftsUnderMinimum)      warnings.Add(GenerateWarningCode.NotEnoughEmployeesForMinimum);
        if (assignmentResult.HasConstraintViolations)    warnings.Add(GenerateWarningCode.EmployeesAssignedWithConstraintViolations);
        if (assignmentResult.HasHighWorkload)            warnings.Add(GenerateWarningCode.HighWorkloadDetected);

        return warnings.Any()
            ? BllScheduleGenerateResult.WithWarnings(allShifts, warnings)
            : BllScheduleGenerateResult.Success(allShifts);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Assignment
    // ─────────────────────────────────────────────────────────────────────────

    private AssignmentResult AssignEmployeesToShifts(
        List<BllShift>                      shifts,
        List<BllEmployee>                   employees,
        Dictionary<int, List<BllEmployee>>  primaryEmployeesByDepartment,
        Dictionary<int, List<BllEmployee>>  substituteEmployeesByDepartment,
        Dictionary<int, int>                shiftTypeToDepartmentId,
        Dictionary<int, SchedulePattern>    shiftTypeToDepartmentPattern,
        Dictionary<int, ScheduleWorkload>   workloads,
        Dictionary<int, List<ScheduleDateRange>> timeOffs,
        double?                             totalBudget,
        bool                                hardTotalHours)
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
            var shiftDuration     = CalcDuration(shift);
            var departmentId      = shiftTypeToDepartmentId[shift.ShiftTypeId];
            var departmentPattern = shiftTypeToDepartmentPattern.GetValueOrDefault(shift.ShiftTypeId, SchedulePattern.Flexible);
            var primaryDepartment    = primaryEmployeesByDepartment.GetValueOrDefault(departmentId)    ?? new List<BllEmployee>();
            var substituteDepartment = substituteEmployeesByDepartment.GetValueOrDefault(departmentId) ?? new List<BllEmployee>();

            int targetCount;
            if (totalBudget == null || shiftDuration <= 0)
            {
                targetCount = shift.MaxEmployees;
            }
            else
            {
                double remaining = totalBudget.Value - usedBudget;
                int canAfford    = (int)(remaining / shiftDuration);

                if (hardTotalHours)
                {
                    targetCount = Math.Min(shift.MaxEmployees, Math.Max(0, canAfford));
                    if (targetCount <= 0)
                    {
                        budgetExhausted = true;
                        shiftsUnderMinimum++;
                        continue;
                    }
                }
                else
                {
                    targetCount = Math.Min(shift.MaxEmployees, Math.Max(shift.MinEmployees, canAfford));
                    if (canAfford < shift.MinEmployees)
                        budgetExhausted = true;
                }
            }

            var candidates = primaryDepartment
                .Where(e => CanAssignToShift(e, shift, workloads[e.Id], shiftDuration, departmentPattern, timeOffs))
                .OrderBy(e => workloads[e.Id].TotalHours)
                .Take(targetCount)
                .ToList();

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

            if (candidates.Count < shift.MinEmployees)
            {
                int relaxTarget = Math.Min(shift.MinEmployees, targetCount) - candidates.Count;
                if (relaxTarget > 0)
                {
                    var allDepartmentEmployees = primaryDepartment.Concat(substituteDepartment).Distinct().ToList();
                    var additionalCandidates = allDepartmentEmployees
                        .Where(e => !candidates.Contains(e) && !IsOnTimeOff(e.Id, shift.Date, timeOffs))
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

        bool hasHighWorkload = employees.Any(e =>
            workloads[e.Id].TotalHours > 160 * (double)e.EmploymentRate);

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
        ScheduleWorkload workload,
        double shiftHours,
        SchedulePattern departmentPattern,
        Dictionary<int, List<ScheduleDateRange>> timeOffs)
    {
        const double MAX_MONTHLY_HOURS = 200;
        const double MAX_WEEKLY_HOURS  = 48;
        var rate = (double)employee.EmploymentRate;

        if (IsOnTimeOff(employee.Id, shift.Date, timeOffs))
            return false;

        if (workload.TotalHours + shiftHours > MAX_MONTHLY_HOURS * rate)
            return false;

        if (IsSameWeek(shift.Date, workload.LastShiftDate) &&
            workload.WeeklyHours + shiftHours > MAX_WEEKLY_HOURS * rate)
            return false;

        if (departmentPattern != SchedulePattern.Flexible &&
            !MatchesPattern(workload, shift.Date, departmentPattern))
            return false;

        if (workload.LastShiftDate.HasValue &&
            workload.LastShiftDate.Value.AddDays(1) == shift.Date)
        {
            if (CalcRest(workload.LastShiftEndTime, shift.StartTime) < 11)
                return false;
        }

        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Greedy-specific calendar helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static bool IsWorkingHoursFullyCovered(List<BllShift> shifts, TimeSpan workStart, TimeSpan workEnd)
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

    // ─────────────────────────────────────────────────────────────────────────
    // Greedy-specific time-off helper
    // ─────────────────────────────────────────────────────────────────────────

    private static bool IsEmployeeFullyOnTimeOff(
        int employeeId, DateOnly startDate, DateOnly endDate,
        Dictionary<int, List<ScheduleDateRange>> timeOffs)
    {
        if (!timeOffs.TryGetValue(employeeId, out var ranges) || ranges.Count == 0)
            return false;

        var currentDate = startDate;
        while (currentDate <= endDate)
        {
            if (!ranges.Any(r => currentDate >= r.Start && currentDate <= r.End))
                return false;
            currentDate = currentDate.AddDays(1);
        }
        return true;
    }
}
