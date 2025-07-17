namespace DAL.DTO.GroupDtos;

public class DalGroupMinData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public bool AutorenewSchedules { get; set; } = false;
}