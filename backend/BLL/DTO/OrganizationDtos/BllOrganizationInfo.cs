namespace DTOs.OrganizationDtos;

public class BllOrganizationInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string OrganizationType { get; set; } = default!;
    public bool IsOpen24_7 { get; set; }
    public float NightShiftBonus { get; set; }
    public float HolidayBonus { get; set; }
    public string? PhotoUrl { get; set; }
    public int EmployeeCount { get; set; }
}