using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;

namespace Domain;

public class Schedule
{
    [Key]
    public int Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsConfirmed { get; set; } = false;
        
    [ForeignKey("Organization")] [Required] public int OrganizationId { get; set; }
    public Organization Organization { get; set; }
    
    [ForeignKey("Group")] [Required] public int GroupId { get; set; }
    public Group Group { get; set; }
    
    public List<Shift> Shifts { get; set; } = new();
}
