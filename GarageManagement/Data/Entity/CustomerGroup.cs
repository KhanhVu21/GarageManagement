namespace GarageManagement.Data.Entity
{
    public class CustomerGroup
    {
        public Guid Id { get; set; }
        public string? GroupName { get; set; }
        public string? GroupCode { get; set; }
        public int? Status { get; set; }
        public bool? IsHide { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
