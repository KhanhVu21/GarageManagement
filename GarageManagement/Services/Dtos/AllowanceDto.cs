namespace GarageManagement.Services.Dtos
{
    public class AllowanceDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public float? Amount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Status { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
