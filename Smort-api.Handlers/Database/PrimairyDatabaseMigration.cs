using FluentMigrator;

namespace Smort_api.Handlers.Database
{
    [Migration(20250815)]
    public class PrimairyDatabaseMigration : Migration
    {
        public override void Up()
        {
            Create.Table("Username_Counter")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Username").AsString(32)
                .WithColumn("Amount").AsInt32()
                .WithColumn("Created_At").AsDateTime()
                .WithColumn("Updated_At").AsDateTime().Nullable();

            Create.Table("File_Type")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Type").AsString(32);

            Create.Table("File_Content")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("File_Name").AsString(128)
                .WithColumn("File_location").AsString(256)
                .WithColumn("file_type_Id").AsInt32()
                .WithColumn("Content_Id").AsInt32()
                .WithColumn("Created_At").AsDateTime()
                .WithColumn("Deleted_At").AsDateTime().Nullable();

            Create.Table("File_Image")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("File_Name").AsString(128)
                .WithColumn("File_location").AsString(256)
                .WithColumn("file_type_Id").AsInt32()
                .WithColumn("Created_At").AsDateTime()
                .WithColumn("Deleted_At").AsDateTime().Nullable();

            Create.Table("Role")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(16)
                .WithColumn("Description").AsString(128);

            Create.Table("Users_Private")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Role_Id").AsInt32()
                .WithColumn("Email").AsString(64)
                .WithColumn("Password").AsString(128)
                .WithColumn("Salt").AsString(64);

            Create.Table("Users_Public")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Person_Id").AsInt32()
                .WithColumn("Username").AsString(32)
                .WithColumn("Profile_Picture").AsInt32()
                .WithColumn("Created_At").AsDateTime()
                .WithColumn("Updated_At").AsDateTime().Nullable()
                .WithColumn("Deleted_At").AsDateTime().Nullable()
                .WithColumn("AllowedUser").AsBoolean().WithDefaultValue(false);

            Create.Table("Following")
                .WithColumn("User_Id_Followed").AsInt32()
                .WithColumn("User_Id_Follower").AsInt32()
                .WithColumn("Followed_At").AsDateTime();

            Create.Table("Content")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("User_Id").AsInt32()
                .WithColumn("Type").AsString(3)
                .WithColumn("Description").AsCustom("TEXT")
                .WithColumn("Thumbnail").AsInt32().Nullable()
                .WithColumn("Created_At").AsDateTime()
                .WithColumn("Updated_At").AsDateTime().Nullable()
                .WithColumn("Deleted_At").AsDateTime().Nullable();

            Create.Table("Reaction")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("User_Id").AsInt32()
                .WithColumn("Content_Id").AsInt32()
                .WithColumn("Content_Type").AsString(10)
                .WithColumn("Reaction").AsString(int.MaxValue)
                .WithColumn("Created_At").AsDateTime()
                .WithColumn("Updated_At").AsDateTime().Nullable();

            Create.Table("Report_User")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("User_Reported_Id").AsInt32()
                .WithColumn("User_Reporter_Id").AsInt32()
                .WithColumn("Reason").AsString(512)
                .WithColumn("Reported_At").AsDateTime().Nullable();

            Create.Table("Content_Answer")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("User_Id").AsInt32()
                .WithColumn("Content_Id").AsInt32()
                .WithColumn("Answer").AsString()
                .WithColumn("Created_At").AsDateTime()
                .WithColumn("Updated_At").AsDateTime().Nullable();

            Create.ForeignKey("FK_File_Image_FileType")
                .FromTable("File_Image").ForeignColumn("file_type_Id")
                .ToTable("File_Type").PrimaryColumn("Id");

            Create.ForeignKey("FK_File_Content_FileType")
                .FromTable("File_Content").ForeignColumn("file_type_Id")
                .ToTable("File_Type").PrimaryColumn("Id");

            Create.ForeignKey("FK_UsersPrivate_Role")
                .FromTable("Users_Private").ForeignColumn("Role_Id")
                .ToTable("Role").PrimaryColumn("Id");

            Create.ForeignKey("FK_UsersPublic_UsersPrivate")
                .FromTable("Users_Public").ForeignColumn("Person_Id")
                .ToTable("Users_Private").PrimaryColumn("Id");

            Create.ForeignKey("FK_UsersPublic_ProfilePicture")
                .FromTable("Users_Public").ForeignColumn("Profile_Picture")
                .ToTable("File_Image").PrimaryColumn("Id");

            Create.ForeignKey("FK_Following_Followed")
                .FromTable("Following").ForeignColumn("User_Id_Followed")
                .ToTable("Users_Public").PrimaryColumn("Id");

            Create.ForeignKey("FK_Following_Follower")
                .FromTable("Following").ForeignColumn("User_Id_Follower")
                .ToTable("Users_Public").PrimaryColumn("Id");

            Create.ForeignKey("FK_Content_User")
                .FromTable("Content").ForeignColumn("User_Id")
                .ToTable("Users_Public").PrimaryColumn("Id");

            Create.ForeignKey("FK_Content_File")
                .FromTable("File_Content").ForeignColumn("Content_Id")
                .ToTable("Content").PrimaryColumn("Id");

            Create.ForeignKey("FK_Content_Thumbnail")
                .FromTable("Content").ForeignColumn("Thumbnail")
                .ToTable("File_Image").PrimaryColumn("Id");

            Create.ForeignKey("FK_Reaction_User")
                .FromTable("Reaction").ForeignColumn("User_Id")
                .ToTable("Users_Public").PrimaryColumn("Id");

            Create.ForeignKey("FK_ReportUser_Reported")
                .FromTable("Report_User").ForeignColumn("User_Reported_Id")
                .ToTable("Users_Public").PrimaryColumn("Id");

            Create.ForeignKey("FK_ReportUser_Reporter")
                .FromTable("Report_User").ForeignColumn("User_Reporter_Id")
                .ToTable("Users_Public").PrimaryColumn("Id");

            Create.ForeignKey("FK_Content_Answer")
                .FromTable("Content_Answer").ForeignColumn("Content_Id")
                .ToTable("Content").PrimaryColumn("Id");



            Insert.IntoTable("Role").Row(new { Id = 1, Name = "User", Description = "Someone who has an account on the site" });
            Insert.IntoTable("Role").Row(new { Id = 2, Name = "Creator", Description = "Someone who Creates content" });
            Insert.IntoTable("Role").Row(new { Id = 3, Name = "Admin", Description = "Someone who has unlimited power" });

            Insert.IntoTable("File_Type").Row(new { Id = 1, Type = "Post image" });
            Insert.IntoTable("File_Type").Row(new { Id = 2, Type = "Thumbnail" });
            Insert.IntoTable("File_Type").Row(new { Id = 3, Type = "Video" });
            Insert.IntoTable("File_Type").Row(new { Id = 4, Type = "Profile picture" });

        }

        public override void Down()
        {
            Delete.ForeignKey("FK_File_FileType").OnTable("File");
            Delete.ForeignKey("FK_UsersPrivate_Role").OnTable("Users_Private");
            Delete.ForeignKey("FK_UsersPublic_UsersPrivate").OnTable("Users_Public");
            Delete.ForeignKey("FK_UsersPublic_ProfilePicture").OnTable("Users_Public");
            Delete.ForeignKey("FK_Following_Followed").OnTable("Following");
            Delete.ForeignKey("FK_Following_Follower").OnTable("Following");
            Delete.ForeignKey("FK_Content_User").OnTable("Content");
            Delete.ForeignKey("FK_Content_File").OnTable("Content");
            Delete.ForeignKey("FK_Content_Thumbnail").OnTable("Content");
            Delete.ForeignKey("FK_Reaction_User").OnTable("Reaction");
            Delete.ForeignKey("FK_ReportUser_Reported").OnTable("Report_User");
            Delete.ForeignKey("FK_ReportUser_Reporter").OnTable("Report_User");

            Delete.Table("Report_User");
            Delete.Table("Reaction");
            Delete.Table("Content");
            Delete.Table("Following");
            Delete.Table("Users_Public");
            Delete.Table("Users_Private");
            Delete.Table("Role");
            Delete.Table("File_Content");
            Delete.Table("File_Image");
            Delete.Table("File_Type");
            Delete.Table("Username_Counter");
        }
    }
}