using System;

namespace Smort_api.Object.DTO;

public class AllUserDto
{
    public int Id { get; set; }
    public int? Profile_Picture { get; set; }
    public string? Username { get; set; }
    public DateTime? Created_At { get; set; }
    public bool AllowedUser { get; set; }
}
