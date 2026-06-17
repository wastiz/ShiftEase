namespace DAL.DTO.ScheduleDtos;

public class DalSchedule
{
    public int Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsConfirmed { get; set; }
    public int OrganizationId { get; set; }
}