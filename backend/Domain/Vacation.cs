using Domain.Models;

namespace Domain;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Vacation
{
    [Key] public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    [ForeignKey("Employee")] public int EmployeeId { get; set; }
    public Employee Employee { get; set; }
}
