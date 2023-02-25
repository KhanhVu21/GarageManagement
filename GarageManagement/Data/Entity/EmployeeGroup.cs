namespace GarageManagement.Data.Entity
{
    public class EmployeeGroup
    {
        public Guid Id { get; set; }
        public string? GroupName { get; set; }
        public string? GroupCode { get; set; }
        public int? Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public bool? IsHide { get; set; }
    }
}
