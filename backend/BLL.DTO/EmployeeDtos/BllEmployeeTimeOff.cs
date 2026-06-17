namespace BLL.DTO.ScheduleDtos;

public record BllEmployeeTimeOffRequest
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public List<int>? EmployeeIds { get; set; }
}

public record BllEmployeeTimeOff
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }   
    public TimeOffType Type { get; set; }
}

public enum TimeOffType
{
    Vacation,
    SickLeave,
    PersonalDay
}
