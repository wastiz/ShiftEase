namespace DTOs.EmployeeDtos;

public class BllEmployeeMinData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<string> DepartmentNames { get; set; }
    public string Position { get; set; }
    public string? Note { get; set; }
}