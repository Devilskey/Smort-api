namespace Smort_api.Object.DTO;

public class SearchPostsDto
{
    public int Id { get; set; }
    public int FileId { get; set; }
    
    public string Description { get; set; }  = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public int UserId { get; set; }
    public string Username { get; set; }  = string.Empty;
    
    public int Likes { get; set; }
    public bool AlreadyLiked { get; set; }
}