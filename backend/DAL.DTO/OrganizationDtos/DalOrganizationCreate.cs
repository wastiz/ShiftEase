using System.ComponentModel.DataAnnotations;

namespace DAL.DTO.OrganizationDtos;

public class DalOrganizationCreate
{
    [Required] public string Name { get; set; }
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
    [Required] public int EmployerId { get; set; } = 0;
    public List<DalHoliday> OrganizationHolidays { get; set; } = default!;
    [Required] public List<DalWorkDay> OrganizationWorkDays { get; set; }
}