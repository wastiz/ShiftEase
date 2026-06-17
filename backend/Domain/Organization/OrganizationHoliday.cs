using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;

namespace Domain;

public class OrganizationHoliday
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public int Day { get; set; }
    public int Month { get; set; }
    
    public bool IsShortenedDay { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
        
    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; }
}