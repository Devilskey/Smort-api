
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
        public static MySqlCommand GetContentAlgorithmQueryLoggedIn(string id, int page = 0)
        {
            MySqlCommand GetContentCmd = new MySqlCommand();

            GetContentCmd.CommandText = $@"
                SELECT Video.Id, Video.Title, Video.Description, Video.User_Id, Video.File_Id, Video.Created_At, Users_Public.Username, 
                (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Video.Id AND Reaction = \""Like\"" AND Content_Type=\""vid\"") AS Likes,
                (SELECT EXISTS(SELECT Id FROM Reaction WHERE Content_Id = Video.Id AND Reaction = \""Like\"" AND Content_Type=\""vid\"" AND User_Id=@user)) AS AlreadyLiked
                FROM Video INNER JOIN Users_Public On Video.User_Id = Users_Public.Id 
                UNION 
                SELECT Image_Post.Id, Image_Post.Title, Image_Post.Description, Image_Post.User_Id, Image_Post.File_Id, Image_Post.Created_At, Users_Public.Username, 
                (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \""Like\"" AND Content_Type=\""img\"") AS Likes,
                (SELECT EXISTS(SELECT Id FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \""Like\"" AND Content_Type=\""img\"" AND User_Id=@user)) AS AlreadyLiked
                FROM Image_Post INNER JOIN Users_Public On Image_Post.User_Id = Users_Public.Id 
                ORDER BY Created_At DESC LIMIT @max OFFSET @offset;
                ";
            GetContentCmd.Parameters.AddWithValue("@user", id);
            GetContentCmd.Parameters.AddWithValue("@max", MaxContent);
            GetContentCmd.Parameters.AddWithValue("@offset", page * MaxContent);

            return new MySqlCommand();
        }
    }
}
//public Id: number = 0;
//public Title: string = "";
//public Description: string = "";
//public User_Id: number = 0;
//public File_Id: number = 0;
//public Username: string = "";
//public Created_At: string = "";

//public Likes: number = 0;
//public AlreadyLiked: number = 0;

//"SELECT Image_Post.Id, Image_Post.Title,  Image_Post.Description,  Image_Post.File_Id, Image_Post.User_Id, Users_Public.Username, " +
// "(SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \"Like\" AND Content_Type=\"img\") AS Likes, " +
//  "(SELECT EXISTS(SELECT Id FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \"Like\" AND Content_Type=\"img\" AND User_Id=@user)) AS AlreadyLiked " +
//"FROM Image_Post INNER JOIN Users_Public ON Image_Post.User_Id = Users_Public.Id ORDER BY RAND() LIMIT 10; ";
