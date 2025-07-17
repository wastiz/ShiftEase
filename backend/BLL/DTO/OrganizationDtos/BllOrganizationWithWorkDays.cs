namespace DTOs.OrganizationDtos;

public class BllOrganizationWithWorkDays
{
    public BllOrganizationInfo OrganizationInfo { get; set; }
    public List<BllHoliday> OrganizationHolidays { get; set; }
    public List<BllWorkDay> OrganizationWorkDays { get; set; }
}