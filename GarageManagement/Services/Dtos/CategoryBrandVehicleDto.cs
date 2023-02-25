namespace GarageManagement.Services.Dtos
{
    public class CategoryBrandVehicleDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? logo { get; set; }
        public bool? IsHide { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
        public Guid? idFile { get; set; }
    }
}
