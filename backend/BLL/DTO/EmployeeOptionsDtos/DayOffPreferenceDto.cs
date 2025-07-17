namespace DTOs.EmployeeOptionsDtos;

public record DayOffPreferenceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
}