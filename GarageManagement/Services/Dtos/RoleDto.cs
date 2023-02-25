namespace GarageManagement.Services.Dtos
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int Status { get; set; }
        public bool IsDeleted { get; set; }
        public int NumberRole { get; set; }
        public bool IsAdmin { get; set; }
    }
}
