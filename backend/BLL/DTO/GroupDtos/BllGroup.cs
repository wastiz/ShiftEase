namespace DTOs.GroupDtos;

public record BllGroup
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }
    public bool AutorenewSchedules { get; set; } = false;
}