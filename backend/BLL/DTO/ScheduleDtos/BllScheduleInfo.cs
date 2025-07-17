namespace BLL.DTO.ScheduleDtos;

public class BllScheduleInfo
{
    public int Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate {get; set;}
    public int GroupId { get; set; }
    public bool IsConfirmed {get; set;}
}