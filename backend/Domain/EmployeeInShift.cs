using System.ComponentModel.DataAnnotations;
using Domain.Models;

namespace Domain;
using System.ComponentModel.DataAnnotations.Schema;

public class EmployeeInShift
{
    [Key]   
    public int Id { get; set; }
    [ForeignKey("Employee")] [Required] public int EmployeeId { get; set; }
    public Employee Employee { get; set; }
    [ForeignKey("Shift")] [Required] public int ShiftId { get; set; }
    public Shift Shift { get; set; }
}
