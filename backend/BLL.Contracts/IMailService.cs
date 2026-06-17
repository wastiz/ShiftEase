using DTOs.FeedbackDtos;

namespace BLL.Contracts;

public interface IMailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    Task SendSupportNotificationAsync(string senderEmail, string subject, string message);
    Task SendFeedbackNotificationAsync(BllFeedbackResponse feedback);
}
