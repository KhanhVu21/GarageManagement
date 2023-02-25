namespace GarageManagement.Services.Dtos
{
    public class CategoryDistrictDto
    {
        public Guid Id { get; set; }
        public string? DistrictName { get; set; }
        public string? DistrictCode { get; set; }
        public string? CityCode { get; set; }
        public int? Status { get; set; }
        public bool? IsHide { get; set; }
        public DateTime? CreatedDate { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
