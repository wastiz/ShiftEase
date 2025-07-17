namespace BLL.DTO.ScheduleDtos;

public class BllScheduleInfoWithFullShifts
{
    public BllScheduleInfo ScheduleInfo { get; set; } = default!;
    public List<BllFullShift> Shifts { get; set; } = default!;
}