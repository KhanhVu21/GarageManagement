namespace GarageManagement.Controllers.Payload.CategoryModel
{
    public class CategoryModelRequest
    {
        public Guid? Id { get; set; }
        public string? ModelName { get; set; }
        public Guid? IdCategoryBrandVehicle { get; set; }
    }
}
