using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain;

public class Notification
{
    [Key] public int Id { get; set; }
    [Required] public int UserId { get; set; }
    [Required] public UserRole UserRole { get; set; }

    [Required] public string Type { get; set; } = null!;
    public string? Title { get; set; }
    public string Message { get; set; } = null!;

    public string? Data { get; set; } // JSON
    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
