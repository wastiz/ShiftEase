namespace DTOs;

public record BllShiftType
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int EmployeeNeeded { get; set; }
    public string Color { get; set; }
}