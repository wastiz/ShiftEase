namespace DTOs.EmployeeOptionsDtos;

public record SickLeaveDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}