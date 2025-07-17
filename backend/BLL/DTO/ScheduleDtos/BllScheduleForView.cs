using BLL.DTO.ScheduleDtos;
using DTOs.OrganizationDtos;
using DTOs.Shifts;

namespace DTOs.ScheduleDtos;

public class BllScheduleForView
{
    public List<BllWorkDay> WorkDays { get; set; }
    public List<BllHoliday> Holidays { get; set; }
    public BllScheduleInfoWithFullShifts ScheduleInfoWithShifts { get; set; }
}