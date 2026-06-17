namespace BLL.DTO;

public record NotificationDto(
    int Id,
    string Type,
    string? Title,
    string Message,
    string? Data,
    bool IsRead,
    DateTime CreatedAt
);
