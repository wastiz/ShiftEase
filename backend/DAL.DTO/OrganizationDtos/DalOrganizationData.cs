using DAL.DTO.ScheduleDtos;

namespace DAL.DTO.OrganizationDtos;

public class DalOrganizationData
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int DepartmentCount { get; set; }
    public int ShiftTypeCount { get; set; }
    public int EmployeeCount { get; set; }
    public int ScheduleCount { get; set; }
    public DalScheduleSummary ScheduleSummary { get; set; } = new();
}
