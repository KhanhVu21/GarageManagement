namespace GarageManagement.Services.Dtos
{
    public class HolidayDto
    {
        public Guid Id { get; set; }
        public string? NameHoliday { get; set; }
        public DateTime? DateHoliday { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Status { get; set; }
        public float? PercentBonusForWork { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
