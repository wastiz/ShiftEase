namespace DAL.DTO.GroupDtos;

public class DalGroupCreate
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }
    public bool AutorenewSchedules { get; set; } = false;
    public int OrganizationId { get; set; }
}