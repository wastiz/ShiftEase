namespace BLL.DTO.IdentityDtos;

public class BllEmployerAuthResult
{
    public bool Success { get; set; }
    public string? Message { get; set; } = String.Empty;
    public string AccessToken { get; set; } = String.Empty;
    public string RefreshToken { get; set; } = String.Empty;
}