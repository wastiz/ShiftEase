using DTOs.EmployeeDtos;

namespace BLL.DTO.ScheduleDtos;

public record BllSchedule
{
    public int? Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsConfirmed { get; set; }
    public List<BllShift> Shifts { get; set; } = default!;
}   

public record BllShift
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int ShiftTypeId { get; set; }
    public string ShiftTypeName { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Color { get; set; }
    public int MinEmployees { get; set; }
    public int MaxEmployees { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public List<BllEmployeeMinData> Employees { get; set; }
}
