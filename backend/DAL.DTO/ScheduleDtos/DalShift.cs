using DAL.DTO.EmployeeDtos;

namespace DAL.DTO.ScheduleDtos;

public class DalShift
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int ShiftTypeId { get; set; }
    public string ShiftTypeName { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Color { get; set; }
    public int MinEmployees { get; set; }
    public int MaxEmployees { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public List<DalEmployeeMinData> Employees { get; set; }
}
