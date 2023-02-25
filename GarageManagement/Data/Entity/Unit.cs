namespace GarageManagement.Data.Entity
{
    public class Unit
    {
        public Guid Id { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public int? Status { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UnitCode { get; set; }
        public bool? IsHide { get; set; }
    }
}
