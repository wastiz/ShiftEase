namespace DAL.DTO.ScheduleDtos;

public class DalSchedulePost
{
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
    public bool IsConfirmed { get; init; } = false;
    public List<ShiftCreateDto> Shifts { get; init; } = new();

    public record ShiftCreateDto
    {
        public int ShiftTypeId { get; init; }
        public DateOnly Date { get; init; }
        public List<EmployeeInShiftDto> Employees { get; set; } = new();

        // Backward compatibility: support old format with just employee IDs
        public List<int>? EmployeeIds
        {
            get => Employees.Select(e => e.EmployeeId).ToList();
            init
            {
                if (value != null && value.Any())
                {
                    Employees = value.Select(id => new EmployeeInShiftDto
                    {
                        EmployeeId = id,
                        Note = null
                    }).ToList();
                }
            }
        }
    }

    public record EmployeeInShiftDto
    {
        public int EmployeeId { get; init; }
        public string? Note { get; init; }
    }
}