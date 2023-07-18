namespace Smort_api.Objects
{
    public class UsersPrivateData
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string Email { get; set;}
        public string Password { get; set;}
        public int Token { get; set;}

    }
}