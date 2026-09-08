using System;

namespace Smort_api.Object.DTO;

public class VideoDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? UserId { get; set; }
    public int? Likes { get; set; }
    public bool? AlreadyLiked { get; set; }
}
