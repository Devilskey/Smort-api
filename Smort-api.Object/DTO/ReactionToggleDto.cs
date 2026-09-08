namespace Smort_api.Object.DTO;

public class ReactionToggleDto
{
    public string TypeOfLike { get; set; } = string.Empty;
    public ReactionOwnerDto? Owner { get; set; }
}

public class ReactionOwnerDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
}
