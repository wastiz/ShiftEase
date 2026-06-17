using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.IdentityDtos;

public class BllForgotPassword
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}

public class BllResetPassword
{
    [Required]
    public string Token { get; set; }

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; }
}
