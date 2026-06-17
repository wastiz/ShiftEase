using BLL.DTO.ScheduleDtos;
using DTOs;
using DTOs.DepartmentDtos;
using DTOs.OrganizationDtos;

namespace BLL.Helpers;

public static class CoverageHelper
{
    /// <summary>
    /// Per-shift coverage penalty used inside GA/ACO fitness functions.
    /// Mirrors the frontend computeCoverage severity logic:
    ///   • understaffed (assigned &lt; min) → heavy penalty per missing employee
    ///   • overstaffed  (assigned &gt; max) → lighter penalty per excess employee
    /// </summary>
    public static double ShiftCoveragePenalty(int assigned, int min, int max)
    {
        if (assigned < min) return (min - assigned) * 200.0;
        if (assigned > max) return (assigned - max) * 50.0;
        return 0.0;
    }

    /// <summary>
    /// For each shift type checks which working days of the period are
    /// understaffed or overstaffed.
    ///
    /// Mirrors the frontend computeCoverage function:
    ///   • orgWorkingDays  = days that are not holidays AND match org schedule
    ///   • relevantDays    = orgWorkingDays filtered by the department's own
    ///                       WorkingDays (if empty the department works every
    ///                       org working day — same as the frontend's
    ///                       isDepartmentWorkingDay guard)
    /// </summary>
    public static CoverageResult Check(
        DateOnly start,
        DateOnly end,
        List<BllShiftForCoverage> shifts,
        List<BllShiftTemplate> shiftTypes,
        List<BllDepartment> departments,
        List<BllHoliday> holidays,
        List<BllWorkDay> orgSchedule)
    {
        // All org working days in the range (holidays excluded, shortened days kept)
        var orgWorkingDays = GetOrgWorkingDays(start, end, holidays, orgSchedule);

        var shiftTypeCoverages = shiftTypes.Select(st =>
        {
            var department = departments.FirstOrDefault(d => d.Id == st.DepartmentId);

            // Filter to the days this specific department actually works
            var relevantDays = orgWorkingDays
                .Where(d => IsDepartmentWorkingDay(d, department))
                .ToList();

            var issues = new List<BllDayIssue>();

            foreach (var day in relevantDays)
            {
                var shift    = shifts.FirstOrDefault(s => s.Date == day && s.ShiftTypeId == st.Id);
                var assigned = shift?.EmployeeCount ?? 0;

                if (assigned < st.MinEmployees)
                {
                    issues.Add(new BllDayIssue
                    {
                        IsoDate  = day.ToString("yyyy-MM-dd"),
                        Label    = day.ToString("dd MMM"),
                        Assigned = assigned,
                        Required = st.MinEmployees,
                        Severity = "understaffed"
                    });
                }
                else if (assigned > st.MaxEmployees)
                {
                    issues.Add(new BllDayIssue
                    {
                        IsoDate  = day.ToString("yyyy-MM-dd"),
                        Label    = day.ToString("dd MMM"),
                        Assigned = assigned,
                        Required = st.MaxEmployees,
                        Severity = "overstaffed"
                    });
                }
            }

            return new ShiftTypeCoverageResult
            {
                ShiftTypeId      = st.Id,
                DepartmentName   = department?.Name ?? "—",
                TotalWorkingDays = relevantDays.Count,
                OkDays           = relevantDays.Count - issues.Count(i => i.Severity == "understaffed"),
                Issues           = issues
            };
        }).ToList();

        var allUnderstaffedDates = shiftTypeCoverages
            .SelectMany(c => c.Issues.Where(i => i.Severity == "understaffed").Select(i => i.IsoDate))
            .ToHashSet();

        var totalWorkingDays = shiftTypeCoverages.FirstOrDefault()?.TotalWorkingDays ?? 0;
        var fullyCoveredDays = totalWorkingDays - allUnderstaffedDates.Count;
        var allOk            = allUnderstaffedDates.Count == 0;

        var coverageStatus = allOk ? "ok" : $"{fullyCoveredDays}/{totalWorkingDays}";

        var warnings = shiftTypeCoverages
            .SelectMany(c => c.Issues)
            .ToList();

        return new CoverageResult
        {
            CoverageStatus     = coverageStatus,
            Warnings           = warnings,
            ShiftTypeCoverages = shiftTypeCoverages
        };
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns all org-level working days in [start, end].
    /// Holidays are excluded unless IsShortenedDay == true (shortened days still count).
    /// </summary>
    private static List<DateOnly> GetOrgWorkingDays(
        DateOnly         start,
        DateOnly         end,
        List<BllHoliday> holidays,
        List<BllWorkDay> orgSchedule)
    {
        var days = new List<DateOnly>();
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            var holiday = holidays.FirstOrDefault(h => h.Month == d.Month && h.Day == d.Day);
            if (holiday != null)
            {
                if (holiday.IsShortenedDay)
                    days.Add(d);
                // full holiday → skip
            }
            else if (orgSchedule.Any(w => w.DayOfWeek == d.DayOfWeek))
            {
                days.Add(d);
            }
        }
        return days;
    }

    /// <summary>
    /// Mirrors the frontend isDepartmentWorkingDay:
    /// if the department has no WorkingDays configured it works on every org
    /// working day; otherwise the day must be in its WorkingDays list.
    /// </summary>
    private static bool IsDepartmentWorkingDay(DateOnly date, BllDepartment? department)
    {
        if (department == null || !department.WorkingDays.Any())
            return true;
        return department.WorkingDays.Contains(date.DayOfWeek);
    }
}

// ── Supporting DTOs ───────────────────────────────────────────────────────────

public class BllShiftForCoverage
{
    public DateOnly Date          { get; set; }
    public int      ShiftTypeId   { get; set; }
    public int      EmployeeCount { get; set; }
}

public class ShiftTypeCoverageResult
{
    public int              ShiftTypeId      { get; set; }
    public string           DepartmentName   { get; set; } = default!;
    public int              TotalWorkingDays { get; set; }
    public int              OkDays           { get; set; }
    public List<BllDayIssue> Issues          { get; set; } = new();
}

public class CoverageResult
{
    /// <summary>"ok" when fully covered, otherwise "X/Y" (covered days / working days).</summary>
    public string                    CoverageStatus     { get; set; } = default!;
    public List<BllDayIssue>         Warnings           { get; set; } = new();
    public List<ShiftTypeCoverageResult> ShiftTypeCoverages { get; set; } = new();
}
