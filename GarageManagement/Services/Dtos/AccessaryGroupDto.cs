namespace GarageManagement.Services.Dtos
{
    public class AccessaryGroupDto
    {
        public Guid Id { get; set; }
        public string? GroupName { get; set; }
        public string? Code { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
