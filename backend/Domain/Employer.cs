using System.ComponentModel.DataAnnotations;
using Domain;

public class Employer
{
    [Key] public int Id { get; set; }
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required] public string Email { get; set; }
    public string? Password { get; set; }
    public string? GoogleId { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public ICollection<Organization> Organizations { get; set; }
}