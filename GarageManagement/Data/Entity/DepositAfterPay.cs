namespace GarageManagement.Data.Entity
{
    public class DepositAfterPay
    {
        public Guid Id { get; set; }
        public Guid IdRepairOders { get; set; }
        public Guid IdCustomer { get; set; }
        public Guid IdDebt { get; set; }
        public float TotalMoney { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }
    }
}
