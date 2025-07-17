using Domain;
using DTOs.SupportDtos;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class SupportRepository : ISupportRepository
    {
        private readonly AppDbContext _context;

        public SupportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _context.SupportMessages
                .CountAsync(m => !m.IsRead);
        }

        public async Task<List<SupportMessage>> GetRecentMessagesAsync(int count)
        {
            return await _context.SupportMessages
                .OrderByDescending(m => m.SentAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<SupportMessage>> GetOrganizationsWithIssuesAsync()
        {
            return await _context.SupportMessages
                .Where(sm => !sm.IsResolved)
                .ToListAsync();
        }

        public async Task AddMessageAsync(DalSupportMessage dto)
        {
            var message = new SupportMessage
            {
                SenderEmail = dto.SenderEmail,
                Subject = dto.Subject,
                Message = dto.Message
            };

            _context.SupportMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SupportMessage>> GetAllMessagesAsync()
        {
            return await _context.SupportMessages
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<bool> MarkAsResolvedAsync(int messageId)
        {
            var message = await _context.SupportMessages.FindAsync(messageId);
            if (message == null) return false;

            message.IsResolved = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SupportMessage?> GetByIdAsync(int id)
        {
            return await _context.SupportMessages
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task MarkAsReadAsync(int messageId)
        {
            var message = await _context.SupportMessages.FindAsync(messageId);
            if (message != null && !message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ReplyToMessageAsync(DalSupportReply dto)
        {
            var message = await _context.SupportMessages.FindAsync(dto.MessageId);
            if (message == null) return false;

            // Логика отправки ответа (если будет реализована) — добавить здесь

            message.IsResolved = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
