namespace GarageManagement.Services.Dtos
{
    public class DebtDto
    {
        public Guid Id { get; set; }
        public string? DebtContent { get; set; }
        public float? Deposit { get; set; }
        public float? TotalPay { get; set; }
        public float? DebtNumber { get; set; }
        public bool? LastPay { get; set; }
        public Guid? IdRepairOrders { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Status { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
