using BLL.DTO.ScheduleDtos;
using DTOs.EmployeeDtos;
using DTOs.DepartmentDtos;
using DTOs.OrganizationDtos;

namespace DTOs.ScheduleDtos;

public class BllScheduleEditorData
{
    public List<BllEmployeeMinData> Employees { get; set; }
    public List<BllDepartment> Departments { get; set; }
    public List<BllShiftTemplate> ShiftTypes { get; set; }
    public List<BllHoliday> OrganizationHolidays { get; set; } = default!;
    public List<BllWorkDay> OrganizationSchedule { get; set; } = default!;
    public List<BllEmployeeTimeOff> EmployeeTimeOffs { get; set; } = new();
    public BllSchedule Schedule { get; set; }
}