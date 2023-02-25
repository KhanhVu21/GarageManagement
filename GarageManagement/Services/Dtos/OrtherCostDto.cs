namespace GarageManagement.Services.Dtos
{
    public class OrtherCostDto
    {
        public Guid Id { get; set; }
        public string? CostContent { get; set; }
        public float? Quantity { get; set; }
        public float? Price { get; set; }
        public float? TotalMoney { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Status { get; set; }
        public Guid? IdRepairOders { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
