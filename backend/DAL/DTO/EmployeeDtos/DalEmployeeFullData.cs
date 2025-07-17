namespace DAL.DTO.EmployeeDtos;

public class DalEmployeeFullData
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Position { get; set; } = default!;
    public float Workload { get; set; }
    public float Salary { get; set; }
    public float SalaryInHour { get; set; }
    public string Priority { get; set; }
    public bool OnVacation { get; set; }
    public bool OnSickLeave { get; set; }
    public bool OnWork { get; set; }
    public int GroupId { get; set; }
    public string? GroupName { get; set; }
}
