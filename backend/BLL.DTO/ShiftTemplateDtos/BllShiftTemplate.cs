namespace DTOs;

public record BllShiftTemplateBase
{
    public string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int MinEmployees { get; set; }
    public int MaxEmployees { get; set; }
    public string Color { get; set; }
    public TimeSpan? BreakDuration { get; set; }
}

public record BllShiftTemplate : BllShiftTemplateBase
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public int OrganizationId { get; set; }
}

public record BllShiftTemplateCreate : BllShiftTemplateBase
{
    public int DepartmentId { get; set; }
    public int OrganizationId { get; set; }
}

public record BllShiftTemplateUpdate : BllShiftTemplateBase
{
    public int Id { get; set; }
}
