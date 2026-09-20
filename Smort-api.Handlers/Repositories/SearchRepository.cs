using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Smort_api.Object.DTO;

namespace Smort_api.Handlers.Repositories;

public class SearchRepository
    (ILogger<SearchRepository> _logger, IDbConnection _db): ISearchRepository
{
    public async Task<SearchAllDto> SearchAllAsync(string query, int userId)
    {
        //Searches Content <--- For content
        // Awnsers <--- for possible matches that questions contain
        //Searches user public <-- for profiles
        var searchQuery = """
                          select Id, Username, Profile_Picture 
                          from Users_Public 
                          where Username LIKE @Query;

                          select Content.Id, Content.User_Id, Content.Description, Content.Type, Content.Created_At, Users_Public.Username,
                                (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like') AS Likes,
                                (SELECT EXISTS(SELECT Id FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like' AND User_Id=@user)) AS AlreadyLiked,
                                (SELECT Id FROM File_Content WHERE Content_Id=Content.Id) As File_Id
                          from Content 
                              inner join Users_Public On Content.User_Id = Users_Public.Id 
                              where description LIKE @Query or Users_Public.Username LIKE @Query;
                          """;
        
       var searchResults = await _db.QueryMultipleAsync(searchQuery, new { Query=$"%{query}%", user=userId });
       var userResults = searchResults.Read<SearchUsersDto>().ToList();
       var postResults = searchResults.Read<SearchPostsDto>().ToList();
       
       return new SearchAllDto() { UserResults = userResults, PostsResults = postResults };
    }
}