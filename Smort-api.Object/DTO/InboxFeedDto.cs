namespace Smort_api.Object.DTO;

public class InboxFeedDto
{
    public int Id { get; set; }
    public int NotificationFromUser { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool HasSeen { get; set; }
    public DateTime CreatedAt { get; set; } 
}