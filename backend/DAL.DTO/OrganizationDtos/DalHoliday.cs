namespace DAL.DTO.OrganizationDtos;

public record DalHoliday
{
    public string Name { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public bool IsShortenedDay { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}