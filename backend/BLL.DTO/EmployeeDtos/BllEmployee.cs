using System.ComponentModel.DataAnnotations;

namespace DTOs.EmployeeDtos;

public record BllEmployeeBase
{
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required] [EmailAddress] public string Email { get; set; }
    [Required] public string Position { get; set; }
    public string? Phone { get; set; }
    public float? HourlyRate { get; set; }
    public float EmploymentRate { get; set; } = 1.0f;
    public string? Priority { get; set; }
    public List<int>? DepartmentIds { get; set; } = new List<int>();
    public int? PrimaryDepartmentId { get; set; }
}

public record BllEmployee : BllEmployeeBase
{
    public int Id { get; set; }
    public List<string>? DepartmentNames { get; set; } = new List<string>();
    public bool OnVacation { get; set; }
    public bool OnSickLeave { get; set; }
    public bool OnWork { get; set; }
}

public record BllEmployeeCreate : BllEmployeeBase
{
    [Required] public int OrganizationId { get; set; }
}

public record BllEmployeeUpdate : BllEmployeeBase
{
    [Required] public int Id { get; set; }
}


