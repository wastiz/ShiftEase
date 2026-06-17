using System.Text.Json.Serialization;

namespace DTOs.ScheduleDtos;

public record BllSchedulePost
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public bool IsConfirmed { get; init; }
    public List<ShiftPost> Shifts { get; init; } = new();

    public record ShiftPost
    {
        public int ShiftTypeId { get; init; }
        public DateOnly Date { get; init; }
        public List<EmployeeInShiftDto> Employees { get; set; } = new();
    }

    public record EmployeeInShiftDto
    {
        [JsonPropertyName("id")]
        public int EmployeeId { get; init; }
        public string? Note { get; init; }
    }
}
