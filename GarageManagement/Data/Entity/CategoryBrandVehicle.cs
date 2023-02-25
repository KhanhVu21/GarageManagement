namespace GarageManagement.Data.Entity
{
    public class CategoryBrandVehicle
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsHide { get; set; }
        public string? logo { get; set; }

    }
}
