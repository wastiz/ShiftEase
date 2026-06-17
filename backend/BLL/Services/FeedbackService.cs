using BLL.Contracts;
using DTOs.FeedbackDtos;

namespace BLL.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IMailService _mailService;

    public FeedbackService(IMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task SubmitFeedbackAsync(BllFeedbackResponse dto)
    {
        await _mailService.SendFeedbackNotificationAsync(dto);
    }
}
