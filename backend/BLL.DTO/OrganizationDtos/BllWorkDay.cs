using System.Text.Json.Serialization;

namespace DTOs.OrganizationDtos;

public class BllWorkDay
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DayOfWeek DayOfWeek { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
}