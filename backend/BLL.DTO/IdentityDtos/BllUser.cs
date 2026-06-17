using Domain.Enums;

namespace BLL.DTO.IdentityDtos;

public interface BllUser
{
    bool Authenticated { get; set; }
    int Id { get; set; }
    UserRole Role { get; set; }
    string FullName { get; set; }
    string? Email { get; set; }
}

public class BllEmployerMeData : BllUser
{
    public bool Authenticated { get; set; }
    public int Id { get; set; }
    public UserRole Role { get; set; }
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
}

public class BllEmployeeMeData : BllUser
{
    public bool Authenticated { get; set; }
    public int Id { get; set; }
    public UserRole Role { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; }
    public List<int>? DepartmentIds { get; set; } = new List<int>();
    public List<string>? DepartmentNames { get; set; } = new List<string>();
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = "";
    public string Position { get; set; } = "";
}