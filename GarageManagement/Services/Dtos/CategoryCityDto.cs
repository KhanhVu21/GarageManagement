namespace GarageManagement.Services.Dtos
{
    public class CategoryCityDto
    {
        public Guid Id { get; set; }
        public string? CityName { get; set; }
        public string? CityCode { get; set; }
        public int? Status { get; set; }
        public bool? IsHide { get; set; }
        public DateTime? CreateDate { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
