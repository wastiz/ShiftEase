using BLL.DTO;
using BLL.Helpers;
using BLL.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications()
    {
        var userId = User.GetUserId();
        var role = User.GetRole();
        var notifications = await _notificationService.GetByUserAsync(userId, role);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var userId = User.GetUserId();
        var role = User.GetRole();
        var count = await _notificationService.GetUnreadCountAsync(userId, role);
        return Ok(count);
    }

    [HttpPost("{id}/read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var userId = User.GetUserId();
        var role = User.GetRole();
        await _notificationService.MarkAsReadAsync(id, userId, role);
        return Ok();
    }

    [HttpPost("read-all")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var userId = User.GetUserId();
        var role = User.GetRole();
        await _notificationService.MarkAllAsReadAsync(userId, role);
        return Ok();
    }
}
