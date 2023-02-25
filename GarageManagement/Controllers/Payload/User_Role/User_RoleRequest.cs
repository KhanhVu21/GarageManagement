using System.ComponentModel.DataAnnotations;

namespace GarageManagement.Controllers.Payload.User_Role
{
    public class User_RoleRequest
    {
        [Required]
        public Guid IdRole { get; set; }
        [Required]
        public Guid IdUser { get; set; }
    }
}
