using Domain.Enums;
using DTOs;

namespace DTOs.DepartmentDtos;

public record BllDepartmentBase
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Color { get; init; } = "#3b82f6";
    public TimeSpan? StartTime { get; init; }
    public TimeSpan? EndTime { get; init; }
    public List<DayOfWeek> WorkingDays { get; init; } = new();
    public SchedulePattern DefaultSchedulePattern { get; init; } = SchedulePattern.Flexible;
}

public record BllDepartment : BllDepartmentBase
{
    public int Id { get; set; }
    public bool AutorenewSchedules { get; set; } = false;
    public List<BllShiftTemplate> ShiftTypes { get; init; } = new();
}

public record BllDepartmentCreate : BllDepartmentBase;

public record BllDepartmentUpdate : BllDepartmentBase
{
    public int Id { get; set; }
}