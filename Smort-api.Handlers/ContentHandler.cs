
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Smort_api.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Handlers
{
    /// <summary>
    /// Handles Complex queries that have to do with content
    /// </summary>
    public static class ContentHandler
    {
        private static readonly int MaxContent = 30;

        public static string GetContentAlgorithmQuery(string search) => $@"
                SELECT Content.Id, Content.Description, Content.User_Id,
                (SELECT Id FROM File_Content WHERE Content_Id=Content.Id) As File_Id,
                Content.Created_At, Users_Public.Username, Content.Type,
                (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like') AS Likes,
                null AS AlreadyLiked
                FROM Content
                INNER JOIN Users_Public On Content.User_Id = Users_Public.Id 
                {(search != "" ? "WHERE LOWER(Content.Description) LIKE @asked " : "")}
                ORDER BY Content.Created_At DESC LIMIT @max OFFSET @offset;
                ";
        

        public static string GetContentAlgorithmQueryLoggedIn(string search) => $@"
                SELECT Content.Id, Content.Description, Content.User_Id, Content.Created_At, Users_Public.Username, Content.Type,
                (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like') AS Likes,
                (SELECT EXISTS(SELECT Id FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like' AND User_Id=@user)) AS AlreadyLiked,
                (SELECT Id FROM File_Content WHERE Content_Id=Content.Id) As File_Id
                FROM Content
                INNER JOIN Users_Public On Content.User_Id=Users_Public.Id
                {(search != "" ? "WHERE LOWER(Content.Description) LIKE @asked " : "")}
                ORDER BY Content.Created_At DESC LIMIT @max OFFSET @offset;
                ";



        public static string GetContentItemAlgorithmQuery() => $@"
                SELECT Content.Id, Content.Description, Content.User_Id, Content.Created_At, Users_Public.Username, Content.Type,
                (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like') AS Likes,
                (SELECT Id FROM File_Content WHERE Content_Id=Content.Id) As File_Id,
                null AS AlreadyLiked
                FROM Content 
                INNER JOIN Users_Public On Content.User_Id = Users_Public.Id 
                WHERE Content.id = @Contentid;
                ";
        

        public static string GetContentItemAlgorithmQueryLoggedIn() => $@"
                SELECT Content.Id, Content.Description, Content.User_Id, Content.Created_At, Users_Public.Username, Content.Type,
                (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like') AS Likes,
                (SELECT Id FROM File_Content WHERE Content_Id=Content.Id) As File_Id,
                (SELECT EXISTS(SELECT Id FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like' AND User_Id=@user)) AS AlreadyLiked
                FROM Content
                INNER JOIN Users_Public On Content.User_Id = Users_Public.Id
                WHERE Content.id = @Contentid;
                ";
    }
}