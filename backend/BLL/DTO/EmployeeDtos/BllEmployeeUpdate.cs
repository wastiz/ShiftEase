namespace DTOs.EmployeeDtos;

public class BllEmployeeUpdate
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Password { get; set; }
    public string Position { get; set; }
    public float Workload { get; set; }
    public float Salary { get; set; }
    public float SalaryInHour { get; set; }
    public string Priority { get; set; }
    public int GroupId { get; set; }
}