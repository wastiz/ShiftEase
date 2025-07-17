namespace BLL.DTO.IdentityDtos;

public class BllEmployerRegister
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; } = String.Empty;
    public string Password { get; set; }
}