
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

        public static MySqlCommand GetContentAlgorithmQuery(string search, int page = 0)
        {
            MySqlCommand GetContentCmd = new MySqlCommand();

            GetContentCmd.CommandText = $@"
                SELECT Content.Id, Content.Title, Content.Description, Content.User_Id, Content.File_Id, Content.Created_At, Users_Public.Username, Content.Type,
                (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like') AS Likes,
                null AS AlreadyLiked
                FROM Content 
                INNER JOIN Users_Public On Content.User_Id = Users_Public.Id 
                {(search != "" ? "WHERE Content.Title LIKE @asked OR Content.Description LIKE @asked" : "")}
                ORDER BY Created_At DESC LIMIT @max OFFSET @offset;
                ";

            GetContentCmd.Parameters.AddWithValue("@asked", search);

            GetContentCmd.Parameters.AddWithValue("@max", MaxContent);
            GetContentCmd.Parameters.AddWithValue("@offset", page * MaxContent);
            Console.WriteLine(GetContentCmd.CommandText);

            return GetContentCmd;
        }

        public static MySqlCommand GetContentAlgorithmQueryLoggedIn(string id, string search, int page = 0)
        {
            MySqlCommand GetContentCmd = new MySqlCommand();

            GetContentCmd.CommandText = $@"
                SELECT Content.Id, Content.Title, Content.Description, Content.User_Id, Content.File_Id, Content.Created_At, Users_Public.Username, Content.Type,
                (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like') AS Likes,
                (SELECT EXISTS(SELECT Id FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like' AND User_Id=@user)) AS AlreadyLiked
                FROM Content
                INNER JOIN Users_Public On Content.User_Id = Users_Public.Id
                {(search != "" ? "WHERE Content.Title LIKE @asked OR Content.Description LIKE @asked" : "")}
                ORDER BY Created_At DESC LIMIT @max OFFSET @offset;
                ";

            GetContentCmd.Parameters.AddWithValue("@asked", search);
            GetContentCmd.Parameters.AddWithValue("@user", id);
            GetContentCmd.Parameters.AddWithValue("@max", MaxContent);
            GetContentCmd.Parameters.AddWithValue("@offset", page * MaxContent);
            Console.WriteLine(GetContentCmd.CommandText);

            return GetContentCmd;
        }
    }
}