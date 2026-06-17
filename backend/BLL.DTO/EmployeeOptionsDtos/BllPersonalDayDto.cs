using System.ComponentModel.DataAnnotations;

namespace DTOs.EmployeeOptionsDtos;

public record BllPersonalDayDtoBase
{
    [Required] public DateOnly StartDate { get; set; }
    [Required] public DateOnly EndDate { get; set; }
}

public record BllPersonalDayDto : BllPersonalDayDtoBase
{
    public int Id { get; init; }
    public int EmployeeId { get; set; }
}

public record BllPersonalDayCreateDto : BllPersonalDayDtoBase
{
}

public record PersonalDayRequestDtoBase
{
    [Required] public DateOnly StartDate { get; set; }
    [Required] public DateOnly EndDate { get; set; }
}

public record PersonalDayRequestDto : PersonalDayRequestDtoBase
{
    public int Id { get; init; }
    public int EmployeeId { get; set; }
    public bool Accepted { get; set; }
    public bool Rejected { get; set; }
}

public record PersonalDayRequestCreateDto : PersonalDayRequestDtoBase
{
}
