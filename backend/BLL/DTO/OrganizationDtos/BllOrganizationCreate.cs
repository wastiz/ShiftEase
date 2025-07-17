using System.ComponentModel.DataAnnotations;

namespace DTOs.OrganizationDtos;

public record BllOrganizationCreate
{
    public string Name { get; set; }
    public string? Description { get; set; } = null;
    public string? Address { get; set; } = null;
    public string? Phone { get; set; } = null;
    public string? Website { get; set; } = null;
    public string OrganizationType { get; set; }
    public bool IsOpen24_7 { get; set; } = false;
    public float NightShiftBonus { get; set; } = 0;
    public float HolidayBonus { get; set; } = 0;
    public string? PhotoUrl { get; set; } = null;
    public int EmployeeCount { get; set; } = 0;
    public int EmployerId { get; set; } = 0;
    public List<BllHoliday> OrganizationHolidays { get; set; } = new List<BllHoliday>();
    public List<BllWorkDay> OrganizationWorkDays { get; set; }
}
