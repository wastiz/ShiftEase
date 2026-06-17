namespace DAL.DTO.EmployeeDtos;

public class DalEmployeeMinData
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public string? Note { get; set; }
}
