using System.Runtime.InteropServices.JavaScript;

namespace DTOs.ScheduleDtos;

public record BllSchedulePost
{
    public int GroupId { get; init; }
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
    public bool Autorenewal { get; init; }
    public bool IsConfirmed { get; init; }
    public List<ShiftCreateDto> Shifts { get; init; } = new();
        
    public record ShiftCreateDto
    {
        public int ShiftTypeId { get; init; }
        public DateOnly Date { get; init; }
        public List<int> EmployeeIds { get; init; } = new();
    }
}