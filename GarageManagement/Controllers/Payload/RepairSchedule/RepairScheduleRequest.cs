namespace GarageManagement.Controllers.Payload.RepairSchedule
{
    public class RepairScheduleRequest
    {
        public Guid? Id { get; set; }
        public String? DaySchedule { get; set; }
        public Guid? IdCustomer { get; set; }
        public string? Note { get; set; }
    }
}
