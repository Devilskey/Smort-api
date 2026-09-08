using System;

namespace Smort_api.Object.DTO;

public class AllUserDto
{
    public int Id { get; set; }
    public int? ProfilePicture { get; set; }
    public string? Username { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool AllowedUser { get; set; }
}
