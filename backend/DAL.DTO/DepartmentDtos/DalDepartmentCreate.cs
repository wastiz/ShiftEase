using Domain.Enums;

namespace DAL.DTO.DepartmentDtos;

public class DalDepartmentCreate
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#3b82f6";
    public bool AutorenewSchedules { get; set; } = false;
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public List<DayOfWeek> WorkingDays { get; set; } = new();
    public SchedulePattern DefaultSchedulePattern { get; set; } = SchedulePattern.Flexible;
    public int OrganizationId { get; set; }
}