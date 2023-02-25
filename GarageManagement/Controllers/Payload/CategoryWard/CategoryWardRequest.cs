namespace GarageManagement.Controllers.Payload.CategoryWard
{
    public class CategoryWardRequest
    {
        public Guid? Id { get; set; }
        public string? WardName { get; set; }
        public string? WardCode { get; set; }
        public string? DistrictCode { get; set; }
    }
}
