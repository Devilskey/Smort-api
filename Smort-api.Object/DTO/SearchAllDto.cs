namespace Smort_api.Object.DTO;

public class SearchAllDto
{
    public List<SearchPostsDto> PostsResults { get; set; } = [];
    public List<SearchUsersDto>  UserResults { get; set; } = [];
}