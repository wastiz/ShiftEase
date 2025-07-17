namespace DTOs.EmployeeOptionsDtos;

public record ShiftTypePreferenceDto
{
    public int Id { get; set; }
    public int ShiftTypeId { get; set; }
    public int EmployeeId { get; set; }
}