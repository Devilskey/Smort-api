using System;

namespace Smort_api.Object.DTO;

public class ContentItemDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public int? UserId { get; set; }
    public int? FileId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Username { get; set; }
    public string? Type { get; set; }
    public int? Likes { get; set; }
    public bool? AlreadyLiked { get; set; }
    public int? Thumbnail { get; set; }
}
