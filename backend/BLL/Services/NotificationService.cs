using System.Text.Json;
using BLL.DTO;
using BLL.Contracts;
using DAL;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(int userId, UserRole role, string type, string message, string? title = null, object? data = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            UserRole = role,
            Type = type,
            Title = title,
            Message = message,
            Data = data != null ? JsonSerializer.Serialize(data) : null,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }

    public async Task<List<NotificationDto>> GetByUserAsync(int userId, UserRole role)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId && n.UserRole == role)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(
                n.Id,
                n.Type,
                n.Title,
                n.Message,
                n.Data,
                n.IsRead,
                n.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId, UserRole role)
    {
        return await _db.Notifications
            .CountAsync(n => n.UserId == userId && n.UserRole == role && !n.IsRead);
    }

    public async Task MarkAsReadAsync(int notificationId, int userId, UserRole role)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && n.UserRole == role);

        if (notification != null)
        {
            notification.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(int userId, UserRole role)
    {
        await _db.Notifications
            .Where(n => n.UserId == userId && n.UserRole == role && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}
