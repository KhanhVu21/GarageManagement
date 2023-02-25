namespace GarageManagement.Services.Dtos
{
    public class EmployeeDayOffDto
    {
        public Guid Id { get; set; }
        public Guid? IdEmployee { get; set; }
        public DateTime? Dayoff { get; set; }
        public int? TypeOfDayOff { get; set; }
        public int? Status { get; set; }
        public int? Onleave { get; set; }
        public DateTime? CreatedDate { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
