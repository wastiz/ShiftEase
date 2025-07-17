using System.Text.RegularExpressions;

namespace Domain;
using Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class EmployeeInGroup
{
    [Key] public int Id { get; set; }
    [ForeignKey("Employee")] public int EmployeeId { get; set; }
    public Employee Employee { get; set; }
    [ForeignKey("Group")] public int GroupId { get; set; }
    public Group Group { get; set; }
}