namespace GarageManagement.Controllers.Payload.RequestList
{
    public class RequestListModel
    {
        public Guid? Id { get; set; }
        public Guid? Id_RO_RepairOrders { get; set; }
        public string? RequestContent { get; set; }
        public string? Note { get; set; }
    }
}
