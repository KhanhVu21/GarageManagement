namespace GarageManagement.Services.Dtos
{
    public class CategoryGearBoxDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int? Status { get; set; }
        public bool? IsHide { get; set; }
        public DateTime? CreatedDate { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
