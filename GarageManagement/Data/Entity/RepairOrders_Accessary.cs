namespace GarageManagement.Data.Entity
{
    public class RepairOrders_Accessary
    {
        public Guid Id { get; set; }
        public Guid? IdCategoryAccessary { get; set; }
        public Guid? IdRepairOrders { get; set; }
        public float? Quantity { get; set; }
        public float? Price { get; set; }
        public int? DiscountPercent { get; set; }
        public float? DiscountPrice { get; set; }
        public float? TotalMoney { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
