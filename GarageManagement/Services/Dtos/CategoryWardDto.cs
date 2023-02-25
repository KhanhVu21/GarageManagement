namespace GarageManagement.Services.Dtos
{
    public class CategoryWardDto
    {
        public Guid Id { get; set; }
        public string? WardName { get; set; }
        public string? WardCode { get; set; }
        public string? DistrictCode { get; set; }
        public int? Status { get; set; }
        public bool? IsHide { get; set; }
        public DateTime? CreatedDate { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
