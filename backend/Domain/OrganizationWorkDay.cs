namespace Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class OrganizationWorkDay
{
    [Key] public int Id { get; set; }
    [ForeignKey("WeekDay")] public int WeekDayId { get; set; }
    public WeekDay WeekDay { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    
    [ForeignKey("Organization")] public int OrganizationId { get; set; }
}
