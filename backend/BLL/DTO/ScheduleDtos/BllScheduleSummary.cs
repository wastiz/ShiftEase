namespace DTOs.ScheduleDtos;

public record BllScheduleSummary
{
    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public List<string> ConfirmedMonths { get; set; } = new();
    public List<string> UnconfirmedMonths { get; set; } = new();
    public bool Autorenewal { get; set; }
}
