using BLL.DTO.ScheduleDtos;

namespace DTOs.OrganizationDtos;

public class BllOrganizationDashboardData
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int DepartmentCount { get; set; }
    public int ShiftTypeCount { get; set; }
    public int EmployeeCount { get; set; }
    public int ScheduleCount { get; set; }
    public BllScheduleSummary ScheduleSummary { get; set; } = new();
}
