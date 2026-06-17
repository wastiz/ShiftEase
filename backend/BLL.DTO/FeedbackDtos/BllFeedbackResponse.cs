using Domain.Enums;

namespace DTOs.FeedbackDtos;

public class BllFeedbackResponse
{
    public string SchedulingMethodBefore { get; set; } = default!;
    public string ReferralSource { get; set; }
    public string TodaysPurpose { get; set; } = default!;
    public string TaskCompleted { get; set; }
    public string? Obstacle { get; set; }
    public string BiggestChallenge { get; set; } = default!;
    public int Rating { get; set; }
    public string RatingReason { get; set; } = default!;
    public string? AdditionalComments { get; set; }
}
