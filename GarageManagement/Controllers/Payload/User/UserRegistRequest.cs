using System;
using System.ComponentModel.DataAnnotations;

namespace GarageManagement.Controllers.Payload.User
{
    public class UserRegistRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        public string? Fullname { get; set; }
        [Required]
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
