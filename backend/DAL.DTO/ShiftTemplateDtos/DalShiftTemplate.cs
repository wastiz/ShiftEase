namespace DAL.DTO.ShiftTypeDtos;

public class DalShiftTemplateBase
{
    public string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int MinEmployees { get; set; }
    public int MaxEmployees { get; set; }
    public string Color { get; set; }
    public TimeSpan? BreakDuration { get; set; }
}

public class DalShiftTemplate : DalShiftTemplateBase
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public int OrganizationId { get; set; }
}

public class DalShiftTemplateCreate : DalShiftTemplateBase
{
    public int DepartmentId { get; set; }
    public int OrganizationId { get; set; }
}

public class DalShiftTemplateUpdate : DalShiftTemplateBase
{
    public int Id { get; set; }
}
