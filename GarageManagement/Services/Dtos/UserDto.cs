namespace GarageManagement.Services.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? Fullname { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public Guid UserTypeId { get; set; }
        public string? Address { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public string? UserCode { get; set; }
        public bool IsLocked { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public Guid UnitId { get; set; }
        public string ActiveCode { get; set; } = string.Empty;
        public string? Avatar { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
