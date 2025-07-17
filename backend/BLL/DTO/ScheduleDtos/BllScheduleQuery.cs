namespace DTOs.ScheduleDtos;

public class BllScheduleQuery
{
    public int GroupId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public bool ShowOnlyConfirmed { get; set; }
}