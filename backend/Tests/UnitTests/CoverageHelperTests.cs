using BLL.DTO.ScheduleDtos;
using BLL.Helpers;
using DTOs;
using DTOs.DepartmentDtos;
using DTOs.OrganizationDtos;
using Domain.Enums;
using Xunit;

public class CoverageHelperTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static List<BllWorkDay> Weekdays() =>
        new()
        {
            new() { DayOfWeek = DayOfWeek.Monday,    StartTime = "08:00", EndTime = "17:00" },
            new() { DayOfWeek = DayOfWeek.Tuesday,   StartTime = "08:00", EndTime = "17:00" },
            new() { DayOfWeek = DayOfWeek.Wednesday, StartTime = "08:00", EndTime = "17:00" },
            new() { DayOfWeek = DayOfWeek.Thursday,  StartTime = "08:00", EndTime = "17:00" },
            new() { DayOfWeek = DayOfWeek.Friday,    StartTime = "08:00", EndTime = "17:00" }
        };

    private static BllShiftTemplate ShiftType(int id, int deptId, int min = 1, int max = 3) =>
        new()
        {
            Id           = id,
            Name         = $"Shift{id}",
            StartTime    = TimeSpan.FromHours(8),
            EndTime      = TimeSpan.FromHours(16),
            MinEmployees = min,
            MaxEmployees = max,
            Color        = "#fff",
            DepartmentId = deptId,
            OrganizationId = 1
        };

    private static BllDepartment Dept(int id, string name = "Dept") =>
        new() { Id = id, Name = name, Color = "#fff", DefaultSchedulePattern = SchedulePattern.Flexible };

    // ── All covered ───────────────────────────────────────────────────────────

    [Fact]
    public void Check_AllDaysCovered_ReturnsOkStatus()
    {
        // One working Monday
        var start     = new DateOnly(2025, 1, 6); // Monday
        var end       = new DateOnly(2025, 1, 6);
        var shiftType = ShiftType(1, 1, min: 1, max: 3);

        var shifts = new List<BllShiftForCoverage>
        {
            new() { Date = start, ShiftTypeId = 1, EmployeeCount = 2 }
        };

        var result = CoverageHelper.Check(start, end, shifts,
            new List<BllShiftTemplate> { shiftType },
            new List<BllDepartment> { Dept(1) },
            new List<BllHoliday>(),
            Weekdays());

        Assert.Equal("ok", result.CoverageStatus);
        Assert.Empty(result.Warnings);
    }

    // ── Understaffed ──────────────────────────────────────────────────────────

    [Fact]
    public void Check_BelowMinEmployees_ReportsUnderstaffed()
    {
        var start     = new DateOnly(2025, 1, 6);
        var end       = new DateOnly(2025, 1, 6);
        var shiftType = ShiftType(1, 1, min: 3, max: 5);

        var shifts = new List<BllShiftForCoverage>
        {
            new() { Date = start, ShiftTypeId = 1, EmployeeCount = 1 } // below min=3
        };

        var result = CoverageHelper.Check(start, end, shifts,
            new List<BllShiftTemplate> { shiftType },
            new List<BllDepartment> { Dept(1) },
            new List<BllHoliday>(),
            Weekdays());

        Assert.NotEqual("ok", result.CoverageStatus);
        var issue = Assert.Single(result.Warnings);
        Assert.Equal("understaffed", issue.Severity);
        Assert.Equal(1, issue.Assigned);
        Assert.Equal(3, issue.Required);
    }

    [Fact]
    public void Check_NoShiftOnWorkingDay_ReportsUnderstaffed()
    {
        var start     = new DateOnly(2025, 1, 6);
        var end       = new DateOnly(2025, 1, 6);
        var shiftType = ShiftType(1, 1, min: 1, max: 3);

        // No shifts at all — counts as 0 employees assigned
        var result = CoverageHelper.Check(start, end, new List<BllShiftForCoverage>(),
            new List<BllShiftTemplate> { shiftType },
            new List<BllDepartment> { Dept(1) },
            new List<BllHoliday>(),
            Weekdays());

        Assert.NotEqual("ok", result.CoverageStatus);
        Assert.Contains(result.Warnings, w => w.Severity == "understaffed");
    }

    // ── Overstaffed ───────────────────────────────────────────────────────────

    [Fact]
    public void Check_AboveMaxEmployees_ReportsOverstaffed()
    {
        var start     = new DateOnly(2025, 1, 6);
        var end       = new DateOnly(2025, 1, 6);
        var shiftType = ShiftType(1, 1, min: 1, max: 2);

        var shifts = new List<BllShiftForCoverage>
        {
            new() { Date = start, ShiftTypeId = 1, EmployeeCount = 5 } // above max=2
        };

        var result = CoverageHelper.Check(start, end, shifts,
            new List<BllShiftTemplate> { shiftType },
            new List<BllDepartment> { Dept(1) },
            new List<BllHoliday>(),
            Weekdays());

        var issue = Assert.Single(result.Warnings);
        Assert.Equal("overstaffed", issue.Severity);
        Assert.Equal(5, issue.Assigned);
        Assert.Equal(2, issue.Required);
    }

    // ── Coverage status formula ───────────────────────────────────────────────

    [Fact]
    public void Check_PartialCoverage_ReturnsFractionStatus()
    {
        // Jan 6-10 2025 = Mon-Fri = 5 working days
        var start     = new DateOnly(2025, 1, 6);
        var end       = new DateOnly(2025, 1, 10);
        var shiftType = ShiftType(1, 1, min: 1, max: 3);

        // Only Jan 6 (Monday) is covered; Jan 7-10 have 0 employees
        var shifts = new List<BllShiftForCoverage>
        {
            new() { Date = new DateOnly(2025, 1, 6), ShiftTypeId = 1, EmployeeCount = 2 }
        };

        var result = CoverageHelper.Check(start, end, shifts,
            new List<BllShiftTemplate> { shiftType },
            new List<BllDepartment> { Dept(1) },
            new List<BllHoliday>(),
            Weekdays());

        Assert.Equal("1/5", result.CoverageStatus);
    }

    // ── Holidays ──────────────────────────────────────────────────────────────

    [Fact]
    public void Check_FullHoliday_ExcludedFromWorkingDays()
    {
        // Jan 6 (Mon) is a full holiday → not a working day → no coverage required
        var start     = new DateOnly(2025, 1, 6);
        var end       = new DateOnly(2025, 1, 6);
        var shiftType = ShiftType(1, 1, min: 1, max: 3);

        var holidays = new List<BllHoliday>
        {
            new() { Month = 1, Day = 6, IsShortenedDay = false, HolidayName = "Holiday" }
        };

        var result = CoverageHelper.Check(start, end, new List<BllShiftForCoverage>(),
            new List<BllShiftTemplate> { shiftType },
            new List<BllDepartment> { Dept(1) },
            holidays,
            Weekdays());

        // 0 working days → no issues → ok
        Assert.Equal("ok", result.CoverageStatus);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Check_ShortenedHoliday_CountsAsWorkingDay()
    {
        // Jan 6 (Mon) is a shortened holiday — still a working day
        var start     = new DateOnly(2025, 1, 6);
        var end       = new DateOnly(2025, 1, 6);
        var shiftType = ShiftType(1, 1, min: 1, max: 3);

        var holidays = new List<BllHoliday>
        {
            new() { Month = 1, Day = 6, IsShortenedDay = true, HolidayName = "Short" }
        };

        // Provide enough staff
        var shifts = new List<BllShiftForCoverage>
        {
            new() { Date = start, ShiftTypeId = 1, EmployeeCount = 2 }
        };

        var result = CoverageHelper.Check(start, end, shifts,
            new List<BllShiftTemplate> { shiftType },
            new List<BllDepartment> { Dept(1) },
            holidays,
            Weekdays());

        Assert.Equal("ok", result.CoverageStatus);
    }

    // ── Weekends excluded ─────────────────────────────────────────────────────

    [Fact]
    public void Check_WeekendDays_NotCountedAsWorkingDays()
    {
        // Jan 11-12 2025 = Sat-Sun
        var start     = new DateOnly(2025, 1, 11);
        var end       = new DateOnly(2025, 1, 12);
        var shiftType = ShiftType(1, 1, min: 1, max: 3);

        var result = CoverageHelper.Check(start, end, new List<BllShiftForCoverage>(),
            new List<BllShiftTemplate> { shiftType },
            new List<BllDepartment> { Dept(1) },
            new List<BllHoliday>(),
            Weekdays()); // only Mon-Fri configured

        Assert.Equal("ok", result.CoverageStatus);
        Assert.Empty(result.Warnings);
    }

    // ── Multiple shift types ──────────────────────────────────────────────────

    [Fact]
    public void Check_MultipleShiftTypes_EachCheckedIndependently()
    {
        var start = new DateOnly(2025, 1, 6);
        var end   = new DateOnly(2025, 1, 6);

        var shiftTypes = new List<BllShiftTemplate>
        {
            ShiftType(1, 1, min: 2, max: 4), // Morning
            ShiftType(2, 1, min: 1, max: 2)  // Evening
        };

        var shifts = new List<BllShiftForCoverage>
        {
            new() { Date = start, ShiftTypeId = 1, EmployeeCount = 2 }, // ok
            new() { Date = start, ShiftTypeId = 2, EmployeeCount = 0 }  // understaffed
        };

        var result = CoverageHelper.Check(start, end, shifts,
            shiftTypes,
            new List<BllDepartment> { Dept(1) },
            new List<BllHoliday>(),
            Weekdays());

        Assert.NotEqual("ok", result.CoverageStatus);
        Assert.Contains(result.Warnings, w => w.Severity == "understaffed");

        var shiftType2Coverage = result.ShiftTypeCoverages.Single(c => c.ShiftTypeId == 2);
        Assert.Single(shiftType2Coverage.Issues);
    }
}
