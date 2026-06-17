using DTOs.FeedbackDtos;

namespace BLL.Contracts;

public interface IFeedbackService
{
    Task SubmitFeedbackAsync(BllFeedbackResponse dto);
}
