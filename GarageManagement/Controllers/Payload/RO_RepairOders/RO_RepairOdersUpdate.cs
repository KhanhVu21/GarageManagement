namespace GarageManagement.Controllers.Payload.RO_RepairOders
{
    public class RO_RepairOdersUpdate
    {
        public Guid? Id { get; set; }
        public Guid? IdCustomer { get; set; }
        public Guid? IdVehicle { get; set; }
        public int? Kilometer { get; set; }
        public int? TaxPercent { get; set; }
        public float? TotalMoney { get; set; }

        public List<RequestListUpdatePayload>? RequestList { get; set; }
        public List<RepairOrderEmployeeUpdatePayload>? RepairOrderEmployee { get; set; }
        public List<AccessaryOrderUpdatePayload>? AccessaryOrder { get; set; }
        public List<OrtherCostUpdatePayload>? OrtherCost { get; set; }
        public List<DebtUpdatePayload>? Debt { get; set; }
    }
    public record RequestListUpdatePayload
    {
        public Guid? Id { get; set; }
        public string? RequestContent { get; set; }
        public bool? IsProcessing { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsCanceled { get; set; }
        public string? Note { get; set; }
    }
    public record RepairOrderEmployeeUpdatePayload
    {
        public Guid? Id { get; set; }
        public Guid? IdEmployee { get; set; }
        public string? Note { get; set; }
    }
    public record AccessaryOrderUpdatePayload
    {
        public Guid? Id { get; set; }
        public Guid? IdCategoryAccessary { get; set; }
        public float? Quantity { get; set; }
        public float? Price { get; set; }
        public int? DiscountPercent { get; set; }
        public float? DiscountPrice { get; set; }
        public float? TotalMoney { get; set; }
    }
    public record OrtherCostUpdatePayload
    {
        public Guid? Id { get; set; }
        public string? CostContent { get; set; }
        public float? Quantity { get; set; }
        public float? Price { get; set; }
        public float? TotalMoney { get; set; }
    }
    public record DebtUpdatePayload
    {
        public Guid? Id { get; set; }
        public string? DebtContent { get; set; }
        public float? Deposit { get; set; }
        public float? TotalPay { get; set; }
        public float? DebtNumber { get; set; }
        public bool? LastPay { get; set; }
    }
}
