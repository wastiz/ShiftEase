namespace Domain;

using System.ComponentModel.DataAnnotations;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Token { get; set; } = default!;

    [Required]
    public string UserId { get; set; } = default!;

    [Required]
    public string UserRole { get; set; } = default!;

    public DateTime Expires { get; set; }

    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
}
