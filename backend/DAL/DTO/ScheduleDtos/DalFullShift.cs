using DAL.DTO.EmployeeDtos;

namespace DAL.DTO.ScheduleDtos;

public class DalFullShift
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string ShiftTypeName { get; set; }
    public TimeSpan From { get; set; }
    public TimeSpan To { get; set; }
    public string Color { get; set; }
    public int EmployeeNeeded { get; set; }
    public List<DalEmployeeMinData> Employees { get; set; }
}