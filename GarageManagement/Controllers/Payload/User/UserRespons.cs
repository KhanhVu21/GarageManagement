using GarageManagement.Utility;

namespace GarageManagement.Controllers.Payload.User
{
    public class UserRespons
    {
        public Guid Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TokenModel Data { get; set; } = new TokenModel();
        public List<CustomApiRoleOfUser> RoleList { get; set; } = new List<CustomApiRoleOfUser>();
        public bool IsAdmin { get; set; }
    }

    public class CustomApiRoleOfUser
    {
        public Guid IdRole { get; set; }
        public string NameRole { get; set; } = string.Empty;
        public Guid IdUser { get; set; }
        public bool IsAdmin { get; set; }
    }
}
