using System.ComponentModel.DataAnnotations;

namespace DAL.DTO.OrganizationDtos;

public class DalOrganizationUpdate
{
    [Required]
    public int OrganizationId { get; set; }
    public string Name { get; set; } = null!;
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
    public List<DalHoliday> OrganizationHolidays { get; set; } = new List<DalHoliday>();
    public List<DalWorkDay>? OrganizationWorkDays { get; set; } = new List<DalWorkDay>();
}