using System;

namespace Smort_api.Object.DTO;

public class VideoDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public DateTime? Created_At { get; set; }
    public int? User_Id { get; set; }
    public int? Likes { get; set; }
    public bool? AlreadyLiked { get; set; }
}
