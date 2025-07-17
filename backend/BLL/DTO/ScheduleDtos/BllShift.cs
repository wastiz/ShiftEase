using System.Runtime.InteropServices.JavaScript;
using DTOs.EmployeeDtos;

namespace DTOs.Shifts;

public record BllShift
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int ShiftTypeId { get; set; }
    public List<BllEmployeeMinData> Employees { get; set; }
}