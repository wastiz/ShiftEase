using BLL.DTO;
using Domain.Enums;

namespace BLL.Contracts;

public interface INotificationService
{
    Task CreateAsync(int userId, UserRole role, string type, string message, string? title = null, object? data = null);
    Task<List<NotificationDto>> GetByUserAsync(int userId, UserRole role);
    Task<int> GetUnreadCountAsync(int userId, UserRole role);
    Task MarkAsReadAsync(int notificationId, int userId, UserRole role);
    Task MarkAllAsReadAsync(int userId, UserRole role);
}
