namespace GarageManagement.Data.Entity
{
    public class OrtherCost
    {
        public Guid Id { get; set; }
        public string? CostContent { get; set; }
        public float? Quantity { get; set; }
        public float? Price { get; set; }
        public float? TotalMoney { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Status { get; set; }
        public Guid? IdRepairOders { get; set; }
    }
}
