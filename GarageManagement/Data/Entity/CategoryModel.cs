namespace GarageManagement.Data.Entity
{
    public class CategoryModel
    {
        public Guid Id { get; set; }
        public string? ModelName { get; set; }
        public Guid? IdCategoryBrandVehicle { get; set; }
        public int? Status { get; set; }
        public bool? IsHide { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
