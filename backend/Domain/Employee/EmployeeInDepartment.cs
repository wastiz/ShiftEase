using System.Text.RegularExpressions;

namespace Domain;
using Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class EmployeeInDepartment
{
    [Key] public int Id { get; set; }
    [ForeignKey("Employee")] public int EmployeeId { get; set; }
    public Employee Employee { get; set; }
    [ForeignKey("Department")] public int DepartmentId { get; set; }
    public Department Department { get; set; }
    public bool IsPrimary { get; set; } = false;
}
