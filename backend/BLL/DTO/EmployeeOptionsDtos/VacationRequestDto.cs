namespace DTOs.EmployeeOptionsDtos;

public record VacationRequestDto
{
    public int Id { get; init; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool Accepted { get; set; }
    public bool Rejected { get; set; }
}