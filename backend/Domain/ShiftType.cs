namespace Domain;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ShiftType
{
    [Key] public int Id { get; set; }
    public string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int EmployeeNeeded { get; set; }
    public string Color { get; set; }

    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }
}
