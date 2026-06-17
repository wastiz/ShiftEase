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

internal static class ScheduleGeneratorShared
{
    // ── Shared types ──────────────────────────────────────────────────────────

    internal class ScheduleWorkload
    {
        public double    TotalHours       { get; set; }
        public double    WeeklyHours      { get; set; }
        public DateOnly? LastShiftDate    { get; set; }
        public TimeSpan? LastShiftEndTime { get; set; }
        public int       ConsecutiveDays  { get; set; }
    }

    internal record ScheduleDateRange(DateOnly Start, DateOnly End);

    // ── Pure math helpers ─────────────────────────────────────────────────────

    internal static double CalcDuration(BllShift shift)
    {
        var d = shift.EndTime - shift.StartTime;
        if (d.TotalHours < 0) d = d.Add(TimeSpan.FromHours(24));
        if (shift.BreakDuration.HasValue) d -= shift.BreakDuration.Value;
        return d.TotalHours;
    }

    internal static double CalcRest(TimeSpan? lastEnd, TimeSpan nextStart)
    {
        if (!lastEnd.HasValue) return 24;
        var r = nextStart - lastEnd.Value;
        return r.TotalHours < 0 ? r.Add(TimeSpan.FromHours(24)).TotalHours : r.TotalHours;
    }

    internal static bool IsSameWeek(DateOnly a, DateOnly? b)
    {
        if (!b.HasValue) return false;
        static int WeekNum(DateOnly d) => (d.DayNumber - new DateOnly(d.Year, 1, 1).DayNumber) / 7;
        return WeekNum(a) == WeekNum(b.Value);
    }

    internal static (TimeSpan Start, TimeSpan End) GetWorkHours(
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

    internal static bool IsOnTimeOff(
        int empId, DateOnly date, Dictionary<int, List<ScheduleDateRange>> timeOffs) =>
        timeOffs.TryGetValue(empId, out var ranges)
        && ranges.Any(r => date >= r.Start && date <= r.End);

    internal static bool MatchesPattern(ScheduleWorkload wl, DateOnly date, SchedulePattern pattern)
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

    internal static void UpdateWorkload(ScheduleWorkload wl, BllShift shift, double hours)
    {
        wl.TotalHours += hours;

        if (IsSameWeek(shift.Date, wl.LastShiftDate))
            wl.WeeklyHours += hours;
        else
            wl.WeeklyHours = hours;

        wl.ConsecutiveDays = wl.LastShiftDate.HasValue && wl.LastShiftDate.Value.AddDays(1) == shift.Date
            ? wl.ConsecutiveDays + 1
            : 1;

        wl.LastShiftDate    = shift.Date;
        wl.LastShiftEndTime = shift.EndTime;
    }

    internal static int CalcSlotCount(
        BllShift shift, double shiftDuration,
        double? totalBudget, double usedBudget, bool hardTotalHours)
    {
        if (!totalBudget.HasValue || shiftDuration <= 0)
            return shift.MaxEmployees;
        double rem    = totalBudget.Value - usedBudget;
        int canAfford = Math.Max(0, (int)(rem / shiftDuration));
        return hardTotalHours
            ? Math.Min(shift.MaxEmployees, canAfford)
            : Math.Min(shift.MaxEmployees, Math.Max(shift.MinEmployees, canAfford));
    }

    // ── Data loading ──────────────────────────────────────────────────────────

    internal static async Task<Dictionary<int, List<ScheduleDateRange>>> LoadTimeOffsAsync(
        List<int> empIds, DateOnly start, DateOnly end, AppDbContext context)
    {
        var result = empIds.ToDictionary(id => id, _ => new List<ScheduleDateRange>());

        var vacations = await context.Vacations
            .Where(v => empIds.Contains(v.EmployeeId) && v.StartDate <= end && v.EndDate >= start)
            .ToListAsync();

        var sickLeaves = await context.SickLeaves
            .Where(s => empIds.Contains(s.EmployeeId) && s.StartDate <= end && s.EndDate >= start)
            .ToListAsync();

        var personalDays = await context.PersonalDays
            .Where(p => empIds.Contains(p.EmployeeId) && p.StartDate <= end && p.EndDate >= start)
            .ToListAsync();

        foreach (var v in vacations)
            result[v.EmployeeId].Add(new ScheduleDateRange(v.StartDate, v.EndDate));
        foreach (var s in sickLeaves)
            result[s.EmployeeId].Add(new ScheduleDateRange(s.StartDate, s.EndDate));
        foreach (var p in personalDays)
            result[p.EmployeeId].Add(new ScheduleDateRange(p.StartDate, p.EndDate));

        return result;
    }

    // ── Employee pool builders ────────────────────────────────────────────────

    internal static (Dictionary<int, int> ShiftTypeToDeptId,
                     Dictionary<int, SchedulePattern> ShiftTypeToPattern)
        BuildShiftTypeMaps(List<BllDepartment> depts)
    {
        var shiftTypeToDeptId = depts
            .SelectMany(d => d.ShiftTypes.Select(st => (st.Id, DeptId: d.Id)))
            .ToDictionary(x => x.Id, x => x.DeptId);

        var shiftTypeToPattern = depts
            .SelectMany(d => d.ShiftTypes.Select(st => (st.Id, d.DefaultSchedulePattern)))
            .ToDictionary(x => x.Id, x => x.DefaultSchedulePattern);

        return (shiftTypeToDeptId, shiftTypeToPattern);
    }

    internal static (Dictionary<int, List<BllEmployee>> PrimaryByDept,
                     Dictionary<int, List<BllEmployee>> SubstituteByDept)
        BuildEmployeePools(List<BllDepartment> depts, List<BllEmployee> employees)
    {
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

        return (primaryByDept, substituteByDept);
    }

    // ── Shift skeleton builder ────────────────────────────────────────────────

    internal static List<BllShift> BuildEmptyShifts(
        DateOnly start, DateOnly end, BllOrganization org, List<BllDepartment> depts)
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
                                      && holiday.StartTime.HasValue && holiday.EndTime.HasValue;
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

    // ── Heuristic η(employee, shift) ─────────────────────────────────────────

    internal static double ComputeHeuristic(
        BllEmployee                              emp,
        BllShift                                 shift,
        double                                   shiftDuration,
        ScheduleWorkload                         wl,
        double                                   rate,
        SchedulePattern                          pattern,
        Dictionary<int, List<ScheduleDateRange>> timeOffs,
        MaxLimitsRules                           maxLimits,
        RestPeriodsRules                         restPeriods,
        int                                      monthlyNorm,
        bool                                     isPrimary)
    {
        const double PrimaryEta    = 10.0;
        const double SubstituteEta = 3.5;

        if (IsOnTimeOff(emp.Id, shift.Date, timeOffs))
            return 0;

        if (wl.LastShiftDate.HasValue && wl.LastShiftDate.Value.AddDays(1) == shift.Date)
        {
            if (CalcRest(wl.LastShiftEndTime, shift.StartTime) < restPeriods.MinDailyRestHours)
                return 0;
        }

        double weeklyMax = maxLimits.MaxHoursPerWeekAverage * rate;
        if (IsSameWeek(shift.Date, wl.LastShiftDate) && wl.WeeklyHours + shiftDuration > weeklyMax)
            return 0;

        double normCap = monthlyNorm * rate;
        if (wl.TotalHours + shiftDuration > normCap * 1.2)
            return 0;

        if (pattern != SchedulePattern.Flexible && !MatchesPattern(wl, shift.Date, pattern))
            return 0;

        double eta = isPrimary ? PrimaryEta : SubstituteEta;

        double workloadRatio = normCap > 0 ? wl.TotalHours / normCap : 0;
        eta *= Math.Max(0.1, 1.0 - workloadRatio * 0.75);

        if (wl.TotalHours + shiftDuration > normCap)
            eta *= 0.2;

        if (IsSameWeek(shift.Date, wl.LastShiftDate))
        {
            double weeklyRatio = weeklyMax > 0 ? (wl.WeeklyHours + shiftDuration) / weeklyMax : 0;
            if (weeklyRatio > 0.85) eta *= 0.4;
        }

        return eta;
    }

    // ── Fitness function ──────────────────────────────────────────────────────

    internal static double EvaluateFitness(
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

        if (empHours.Count > 1)
        {
            double mean     = empHours.Values.Average();
            double variance = empHours.Values.Select(h => Math.Pow(h - mean, 2)).Average();
            score -= Math.Sqrt(variance) * 1.5;
        }

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

    internal static BllScheduleGenerateResult ApplySolutionAndBuildResult(
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
}
