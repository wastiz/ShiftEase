using DTOs.EmployeeDtos;
using DTOs.GroupDtos;
using DTOs.OrganizationDtos;
using DTOs.Shifts;

namespace DTOs.ScheduleDtos;

public class BllScheduleDataForManaging
{
    public List<BllEmployeeMinData> Employees { get; set; }
    public List<BllShiftType> ShiftTypes { get; set; }
    public List<BllGroup> Groups { get; set; }
    public List<BllHoliday> OrganizationHolidays { get; set; }
    public List<BllWorkDay> OrganizationWorkSchedule { get; set; }
}