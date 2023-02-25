namespace GarageManagement.Controllers.Payload.EmployeeDayOff
{
    public class EmployeeDayOffRequest
    {
        public Guid? Id { get; set; }
        public Guid? IdEmployee { get; set; }
        public DateTime? Dayoff { get; set; }
        public int? TypeOfDayOff { get; set; }
        public int? Onleave { get; set; }
/*        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }*/
    }
}
