using System.ComponentModel.DataAnnotations;

namespace DTOs.OrganizationDtos;

public class BllOrganizationBase
{
    [Required] public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    [Required] public string OrganizationType { get; set; } = default!;
    public bool IsOpen24_7 { get; set; }
    public float NightShiftBonus { get; set; }
    public float HolidayBonus { get; set; }
    public string? PhotoUrl { get; set; }
    public int EmployeeCount { get; set; }
    public List<BllHoliday> Holidays { get; set; }
    public List<BllWorkDay> WorkDays { get; set; }
}
public class BllOrganization : BllOrganizationBase
{
    [Required] public int Id { get; set; }
}

public class BllOrganizationCreate : BllOrganizationBase
{
    [Required] public int EmployerId { get; set; }
};

public class BllOrganizationUpdate : BllOrganizationBase
{
    [Required] public int Id { get; set; }
}

