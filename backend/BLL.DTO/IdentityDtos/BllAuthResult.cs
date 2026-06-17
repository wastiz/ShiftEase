namespace BLL.DTO.IdentityDtos;

public class BllAuthResult
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class BllEmployeeAuthResult : BllAuthResult
{
    public int EmployeeId { get; set; }
    public List<int>? DepartmentIds { get; set; } = new List<int>();
    public int OrganizationId { get; set; }
}