using System.ComponentModel.DataAnnotations;

namespace DAL.DTO.EmployeeDtos;

public class DalEmployeeBase
{
    [Required] public string FirstName { get; set; } = default!;
    [Required] public string LastName { get; set; } = default!;
    [Required] public string Email { get; set; } = default!;
    [Required] public string Position { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public float? HourlyRate { get; set; }
    public float EmploymentRate { get; set; } = 1.0f;
    public string? Priority { get; set; } = "Low";
    public bool OnVacation { get; set; }
    public bool OnSickLeave { get; set; }
    public bool OnWork { get; set; }
    public List<int>? DepartmentIds { get; set; } = new List<int>();
    public int? PrimaryDepartmentId { get; set; }
    public int OrganizationId { get; set; }
}

public class DalEmployee : DalEmployeeBase
{
    public int Id { get; set; }
    public List<string>? DepartmentNames { get; set; } = new List<string>();
    public string? OrganizationName { get; set; }
}

public class DalEmployeeCreate : DalEmployeeBase
{
    public string Password { get; set; } = default!;
};

public class DalEmployeeUpdate : DalEmployeeBase
{
    public int Id { get; set; }
}
