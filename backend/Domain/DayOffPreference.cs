using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;

namespace Domain;

public class DayOffPreference
{
    [Key]
    public int Id { get; set; }
    [ForeignKey("EmployeeId")]
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
}