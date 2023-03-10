namespace GarageManagement.Data.Entity
{
    public class RepairSchedule
    {
        public Guid Id { get; set; }
        public DateTime? DaySchedule { get; set; }
        public DateTime CreatedDate { get; set; }
        public Boolean? IsAccepted { get; set; }
        public Boolean? IsCancel { get; set; }
        public Boolean? IsWaiting { get; set; }
        public int? Status { get; set; }
        public Guid? IdCustomer { get; set; }
        public string? Note { get; set; }
        public string? NoteGarage { get; set; }
    }
}
