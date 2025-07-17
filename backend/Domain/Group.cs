using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;

namespace Domain;

public class Group
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }
    public bool AutorenewSchedules { get; set; } = false;
        
    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; }
}