namespace BLL.DTO.IdentityDtos;

public class BllEmployeeAuthResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int? EmployeeId { get; set; }
    public int? OrganizationId { get; set; }
    public int? GroupId { get; set; }
}