using DAL.DTO.EmployeeDtos;

namespace DAL.DTO.ScheduleDtos;

public class DalShift
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int ShiftTypeId { get; set; }
    public List<DalEmployeeMinData> Employees { get; set; }
}