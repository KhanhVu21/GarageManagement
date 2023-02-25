namespace GarageManagement.Controllers.Payload.Holiday
{
    public class HolidayRequest
    {
        public Guid? Id { get; set; }
        public string? NameHoliday { get; set; }
        public DateTime? DateHoliday { get; set; }
        public float? PercentBonusForWork { get; set; }
    }
}
