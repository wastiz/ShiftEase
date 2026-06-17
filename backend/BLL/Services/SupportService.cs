using BLL.Contracts;
using DTOs.SupportDtos;

namespace BLL.Services;

public class SupportService : ISupportService
{
    private readonly IMailService _mailService;

    public SupportService(IMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task SendMessageAsync(BllSupportMessage dto)
    {
        await _mailService.SendSupportNotificationAsync(dto.SenderEmail, dto.Subject, dto.Message);
    }
}
