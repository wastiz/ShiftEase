namespace Domain;

public class SupportMessage
{
    public int Id { get; set; }
    public string SenderEmail { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; } = false;
    public bool IsRead { get; set; } = false;
}
