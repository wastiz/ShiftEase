using BLL.DTO.ScheduleDtos;

namespace DTOs.Shifts;

public record BllScheduleInfoWithShifts
{
    public BllScheduleInfo ScheduleInfo { get; set; } = default!;
    public List<BllShift> Shifts { get; set; } = default!;
}   