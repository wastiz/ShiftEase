using DTOs.SupportDtos;

namespace BLL.Contracts;

public interface ISupportService
{
    Task SendMessageAsync(BllSupportMessage dto);
}
