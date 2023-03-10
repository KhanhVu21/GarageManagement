namespace GarageManagement.Data.Entity
{
    public class RequestList
    {
        public Guid Id { get; set; }
        public Guid? Id_RO_RepairOrders { get; set; }
        public string? RequestContent { get; set; }
        public bool? IsProcessing { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsCanceled { get; set; }
        public string? Note { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
