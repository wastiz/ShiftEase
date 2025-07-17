namespace DAL.DTO.OrganizationDtos;

public record DalHoliday
{
    public string HolidayName { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
}