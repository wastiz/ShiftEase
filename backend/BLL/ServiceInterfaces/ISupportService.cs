using Domain;
using DTOs.SupportDtos;

namespace BLL.ServiceInterfaces;

public interface ISupportService
{
    Task SendMessageAsync(DalSupportMessage dto);
    Task<List<SupportMessage>> GetAllMessagesAsync();
    Task<int> GetUnreadCountAsync();
    Task<List<SupportMessage>> GetRecentMessagesAsync(int count);
    Task<List<SupportMessage>> GetOrganizationsWithIssuesAsync();
    Task<SupportMessage?> GetByIdAsync(int id);
    Task MarkAsReadAsync(int messageId);
    Task<bool> MarkAsResolvedAsync(int messageId);
    Task<bool> ReplyToMessageAsync(DalSupportReply dto);
}