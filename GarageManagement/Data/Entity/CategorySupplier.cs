namespace GarageManagement.Data.Entity
{
    public class CategorySupplier
    {
        public Guid Id { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierCode { get; set; }
        public string? TaxCode { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public  Guid? IdCategory { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public bool? IsHide { get; set; }
        public Guid? IdCity { get; set; }
        public Guid? IdDistrict { get; set; }
        public Guid? IdWard { get; set; }
    }
}
