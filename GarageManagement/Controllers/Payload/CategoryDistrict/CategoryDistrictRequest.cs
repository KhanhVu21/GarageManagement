namespace GarageManagement.Controllers.Payload.CategoryDistrict
{
    public class CategoryDistrictRequest
    {
        public Guid? Id { get; set; }
        public string? DistrictName { get; set; }
        public string? DistrictCode { get; set; }
        public string? CityCode { get; set; }
    }
}
