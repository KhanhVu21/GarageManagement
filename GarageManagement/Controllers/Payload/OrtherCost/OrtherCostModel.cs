namespace GarageManagement.Controllers.Payload.OrtherCost
{
    public class OrtherCostModel
    {
        public Guid? Id { get; set; }
        public string? CostContent { get; set; }
        public float? Quantity { get; set; }
        public float? Price { get; set; }
        public float? TotalMoney { get; set; }
        public Guid? IdRepairOders { get; set; }
    }
}
