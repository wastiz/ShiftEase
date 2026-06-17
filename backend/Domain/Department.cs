using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain;

public class Department
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#3b82f6";
    public bool AutorenewSchedules { get; set; } = false;
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public List<DayOfWeek> WorkingDays { get; set; } = new();
    public SchedulePattern DefaultSchedulePattern { get; set; } = SchedulePattern.Flexible;
        
    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public ICollection<EmployeeInDepartment> EmployeeDepartments { get; set; } = new List<EmployeeInDepartment>();
    public ICollection<ShiftTemplate> ShiftTemplates { get; set; } = new List<ShiftTemplate>();
}