using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs.SupportDtos;

namespace DAL.Repositories
{
    public interface ISupportRepository
    {
        // Getting count of unread messages
        Task<int> GetUnreadCountAsync();

        // Get count of recent messages
        Task<List<SupportMessage>> GetRecentMessagesAsync(int count);

        // Get all unresolved issues
        Task<List<SupportMessage>> GetOrganizationsWithIssuesAsync();

        // Adding new message
        Task AddMessageAsync(DalSupportMessage dto);

        // Get all messages
        Task<List<SupportMessage>> GetAllMessagesAsync();

        // Mark issue as resolved
        Task<bool> MarkAsResolvedAsync(int messageId);

        // Get message by id
        Task<SupportMessage?> GetByIdAsync(int id);

        // Mark message as read
        Task MarkAsReadAsync(int messageId);

        // Reply to sent message
        Task<bool> ReplyToMessageAsync(DalSupportReply dto);
    }
}