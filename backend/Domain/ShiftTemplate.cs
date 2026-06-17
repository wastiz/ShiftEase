namespace Domain;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ShiftTemplate
{
    [Key] public int Id { get; set; }
    public string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int MinEmployees { get; set; }
    public int MaxEmployees { get; set; }
    public string Color { get; set; }
    public TimeSpan? BreakDuration { get; set; }

    [ForeignKey("Department")]
    public int DepartmentId { get; set; }
    public Department Department { get; set; }

    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; }
}
