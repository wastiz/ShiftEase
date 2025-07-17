namespace DAL.DTO.ScheduleDtos;

public class DalSchedulePost
{
    public int GroupId { get; init; }
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
    public bool Autorenewal { get; init; } = false;
    public bool IsConfirmed { get; init; } = false;
    public List<ShiftCreateDto> Shifts { get; init; } = new();
        
    public record ShiftCreateDto
    {
        public int ShiftTypeId { get; init; }
        public DateOnly Date { get; init; }
        public List<int> EmployeeIds { get; init; } = new();
    }
}