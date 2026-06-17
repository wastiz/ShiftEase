using System.ComponentModel.DataAnnotations;

namespace DTOs.EmployeeOptionsDtos;

public record BllSickLeaveDtoBase
{
    [Required] public DateOnly StartDate { get; set; }
    [Required] public DateOnly EndDate { get; set; }
}

public record BllBllSickLeaveDto : BllSickLeaveDtoBase
{
    public int Id { get; init; }
    public int EmployeeId { get; set; }
}

public record BllSickLeaveCreateDto : BllSickLeaveDtoBase
{
}

public record SickLeaveRequestDtoBase
{
    [Required] public DateOnly StartDate { get; set; }
    [Required] public DateOnly EndDate { get; set; }
}

public record SickLeaveRequestDto : SickLeaveRequestDtoBase
{
    public int Id { get; init; }
    public int EmployeeId { get; set; }
    public bool Accepted { get; set; }
    public bool Rejected { get; set; }
}

public record SickLeaveRequestCreateDto : SickLeaveRequestDtoBase
{
}
