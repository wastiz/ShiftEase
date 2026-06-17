using System.ComponentModel.DataAnnotations;

namespace DTOs.EmployeeOptionsDtos;

public record BllVacationDtoBase
{
    [Required] public DateOnly StartDate { get; set; }
    [Required] public DateOnly EndDate { get; set; }
}

public record BllVacationDto : BllVacationDtoBase
{
    public int Id { get; init; }
    public int EmployeeId { get; set; }
}

public record BllVacationCreateDto : BllVacationDtoBase
{
}

public record VacationRequestDtoBase
{
    [Required] public DateOnly StartDate { get; set; }
    [Required] public DateOnly EndDate { get; set; }
}

public record VacationRequestDto : VacationRequestDtoBase
{
    public int Id { get; init; }
    public int EmployeeId { get; set; }
    public bool Accepted { get; set; }
    public bool Rejected { get; set; }
}

public record VacationRequestCreateDto : VacationRequestDtoBase
{
}
