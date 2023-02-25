namespace GarageManagement.Data.Entity
{
    public class EmployeeDayOff
    {
        public Guid Id { get; set; }
        public Guid? IdEmployee { get; set; }
        public DateTime? Dayoff { get; set; }
        public int? TypeOfDayOff { get; set; }
        public int? Status { get; set; }
        public int? Onleave { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
