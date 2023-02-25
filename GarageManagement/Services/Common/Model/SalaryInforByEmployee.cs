namespace GarageManagement.Services.Common.Model
{
    public class SalaryInforByEmployee
    {
        public float? totalDayWork { get; set; }
        public float? totalDayOff { get; set; }
        public float? totalDayOverTime { get; set; }
        public float? SalaryBase { get; set; }
        public float? totalSocialInsurance { get; set; }
        public float? totalTaxOfEmployee { get; set; }
        public float? totalSalaryReality { get; set; }
        public List<AlowanceOfEmployee>? AllowanceOfEmployees { get; set; }
        public Guid IdEmployee { get; set; }
    }
    public record AlowanceOfEmployee
    {
        public string? Name { get; set; }
        public float? Amount { get; set; }
    }
}
