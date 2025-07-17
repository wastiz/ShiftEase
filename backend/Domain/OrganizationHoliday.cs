using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;

namespace Domain;

public class OrganizationHoliday
{
    [Key]
    public int Id { get; set; }
    public string HolidayName { get; set; }
    public int Day { get; set; }
    public int Month { get; set; }
        
    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; }
}