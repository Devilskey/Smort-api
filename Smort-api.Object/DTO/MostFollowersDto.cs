namespace Smort_api.Object.DTO;

public class MostFollowersDto
{
    public int UserIdFollowed { get; set; }
    public int Amount { get; set; }
    public int? ProfilePicture { get; set; }
    public string? Username { get; set; }
}
