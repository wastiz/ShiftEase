using DTOs.EmployeeDtos;

namespace BLL.DTO.ScheduleDtos;

public class BllFullShift
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string ShiftTypeName { get; set; }
    public TimeSpan From { get; set; }
    public TimeSpan To { get; set; }
    public string Color { get; set; }
    public int EmployeeNeeded { get; set; }
    public List<BllEmployeeMinData> Employees { get; set; }
}