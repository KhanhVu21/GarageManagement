namespace GarageManagement.Data.Entity
{
    public class RO_RepairOders
    {
        public Guid Id { get; set; }
        public string? RO_RepairOdersCode { get; set; }
        public Guid? IdCustomer { get; set; }
        public Guid? IdVehicle { get; set; }
        public int? Kilometer { get; set; }
        public int? TaxPercent { get; set; }
        public float? TotalMoney { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Status { get; set; }
    }
}
