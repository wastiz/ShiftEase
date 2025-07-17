namespace DTOs.OrganizationDtos;

public record BllHoliday
{
    public string HolidayName { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
}