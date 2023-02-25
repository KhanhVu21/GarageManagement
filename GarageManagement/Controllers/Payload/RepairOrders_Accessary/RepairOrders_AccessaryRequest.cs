namespace GarageManagement.Controllers.Payload.RepairOrders_Accessary
{
    public class RepairOrders_AccessaryRequest
    {
        public Guid? IdCategoryAccessary { get; set; }
        public float? Quantity { get; set; }
        public float? Price { get; set; }
        public float? TotalMoneyRepairOrders_Accessary { get; set; }
        public int? DiscountPercent { get; set; }
        public float? DiscountPrice { get; set; }
    }
}
