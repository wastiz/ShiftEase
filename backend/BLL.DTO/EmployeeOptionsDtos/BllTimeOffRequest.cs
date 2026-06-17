namespace DTOs.EmployeeOptionsDtos;

public class BllTimeOffRequest
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOffType Type { get; set; }
    public bool IsApproved { get; set; }
}

public enum TimeOffType { Vacation, SickLeave, PersonalDay }
