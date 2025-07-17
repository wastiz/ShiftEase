namespace Domain;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class VacationRequest
{
    [Key] public int Id { get; set; }
    [ForeignKey("Employee")] public int EmployeeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool Accepted { get; set; }
    public bool Rejected { get; set; }
}