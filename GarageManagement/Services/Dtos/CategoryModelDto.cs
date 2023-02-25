namespace GarageManagement.Services.Dtos
{
    public class CategoryModelDto
    {
        public Guid Id { get; set; }
        public string? ModelName { get; set; }
        public Guid? IdCategoryBrandVehicle { get; set; }
        public int? Status { get; set; }
        public bool? IsHide { get; set; }
        public DateTime? CreatedDate { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
