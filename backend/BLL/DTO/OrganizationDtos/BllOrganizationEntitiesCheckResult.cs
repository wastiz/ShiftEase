namespace DTOs.OrganizationDtos;

public record BllOrganizationEntitiesCheckResult
{
    public bool Groups { get; set; }
    public bool Employees { get; set; }
    public bool ShiftTypes { get; set; }
    public bool Schedules { get; set; }
}