namespace Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class OrganizationWorkDay
{
    [Key] public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    
    [Column(TypeName = "time")]
    public TimeSpan StartTime { get; set; }
    
    [Column(TypeName = "time")]
    public TimeSpan EndTime { get; set; }
    
    [ForeignKey("Organization")] 
    public int OrganizationId { get; set; }
}