namespace GarageManagement.Controllers.Payload.RO_RepairOders
{
    public class RepairOrderPayload
    {
        public Guid? Id { get; set; }
        public Guid? IdCustomer { get; set; }
        public Guid? IdVehicle { get; set; }
        public int? Kilometer { get; set; }
        public int? TaxPercent { get; set; }
        public float? TotalMoney { get; set; }
        public float? Deposit { get; set; }

        public List<RequestListPayload>? RequestList { get; set; }
        public List<RepairOrderEmployeePayload>? RepairOrderEmployee { get; set; }
        public List<AccessaryOrderPayload>? AccessaryOrder { get; set; }
        public List<OrtherCostPayload>? OrtherCost { get; set; }
    }
    public record RequestListPayload
    {
        public Guid? Id { get; set; }
        public Guid? Id_RO_RepairOrders { get; set; }
        public string? RequestContent { get; set; }
        public string? Note { get; set; }
    }
    public record RepairOrderEmployeePayload
    {
        public Guid? Id { get; set; }
        public Guid? IdEmployee { get; set; }
        public Guid? IdRepairOrder { get; set; }
        public string? Note { get; set; }
    }
    public record AccessaryOrderPayload
    {
        public Guid? Id { get; set; }
        public Guid? IdCategoryAccessary { get; set; }
        public Guid? IdRepairOrders { get; set; }
        public float? Quantity { get; set; }
        public float? Price { get; set; }
        public int? DiscountPercent { get; set; }
        public float? DiscountPrice { get; set; }
        public float? TotalMoney { get; set; }
    }
    public record OrtherCostPayload
    {
        public Guid? Id { get; set; }
        public string? CostContent { get; set; }
        public float? Quantity { get; set; }
        public float? Price { get; set; }
        public float? TotalMoney { get; set; }
        public Guid? IdRepairOders { get; set; }
    }
}
