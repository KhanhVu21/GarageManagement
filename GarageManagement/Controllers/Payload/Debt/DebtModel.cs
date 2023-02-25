namespace GarageManagement.Controllers.Payload.Debt
{
    public class DebtModel
    {
        public Guid? Id { get; set; }
        public string? DebtContent { get; set; }
        public float? Deposit { get; set; }
        public float? TotalPay { get; set; }
        public float? DebtNumber { get; set; }
        public bool? LastPay { get; set; }
        public Guid? IdRepairOrders { get; set; }
    }
}
