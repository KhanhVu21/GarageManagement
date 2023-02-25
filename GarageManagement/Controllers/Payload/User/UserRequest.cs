using System;
using System.ComponentModel.DataAnnotations;

namespace GarageManagement.Controllers.Payload.User
{
    public class UserRequest
    {
        public Guid? Id { get; set; }
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public string? Fullname { get; set; }
        public string? Description { get; set; }
        public string? Phone { get; set; }
        [Required]
        public Guid UserTypeId { get; set; }
        public string? Address { get; set; }
        public int? Status { get; set; }
        [Required]
        public Guid UnitId { get; set; }
        public bool IsActive { get; set; }

    }
}
