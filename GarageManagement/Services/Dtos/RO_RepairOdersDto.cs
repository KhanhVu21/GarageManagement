using GarageManagement.Controllers.Payload.RepairOrders_Accessary;

namespace GarageManagement.Services.Dtos
{
    public class RO_RepairOdersDto
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

        //extendtion
        public bool IsPaid { get; set; }
        public Guid IdUserCurrent { get; set; }
        public List<Guid>? IdEmployee { get; set; }
        public List<RepairOrders_AccessaryRequest>? RepairOrders_AccessaryRequests { get; set; }
    }
}
