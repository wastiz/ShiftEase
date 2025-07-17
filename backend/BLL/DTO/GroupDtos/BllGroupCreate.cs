namespace DTOs.GroupDtos;

public record BllGroupCreate {
    public string Name { get; init; }
    public string Description { get; init; }
    public string Color { get; init; }
};