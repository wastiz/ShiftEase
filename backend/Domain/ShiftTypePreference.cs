using Domain.Models;

namespace Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ShiftTypePreference
{
    [Key] public int Id { get; set; }
    [ForeignKey("Employee")] public int EmployeeId { get; set; }
    [ForeignKey("ShiftType")] public int ShiftTypeId { get; set; }
}