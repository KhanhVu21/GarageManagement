namespace GarageManagement.Data.Entity
{
    public class RepairOrders_Employee
    {
        public Guid Id { get; set; }
        public Guid? IdRepairOrders { get; set; }
        public Guid? IdEmployee { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Note { get; set; }
    }
}
