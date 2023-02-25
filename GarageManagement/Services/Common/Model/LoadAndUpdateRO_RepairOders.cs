using GarageManagement.Data.Entity;

namespace GarageManagement.Services.Common.Model
{
    public class LoadAndUpdateRO_RepairOdersBy
    {
        public RO_RepairOders? RO_RepairOders { get; set; }
        public IEnumerable<RepairOrders_Accessary>? ListRepairOrders_Accessary { get; set; }
        public IEnumerable<RepairOrders_Employee>? ListEmployee { get; set; }
        public IEnumerable<Debt>? Debts { get; set; }
        public IEnumerable<RequestList>? RequestLists { get; set; }
        public IEnumerable<OrtherCost>? OrtherCosts { get; set; }
    }
}
