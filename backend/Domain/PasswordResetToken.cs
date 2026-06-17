using System.ComponentModel.DataAnnotations;

namespace Domain;

public class PasswordResetToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
