namespace GarageManagement.Services.Dtos
{
    public class Employee_Salary_HistoryDto
    {
        public Guid Id { get; set; }
        public Guid? IdEmployee { get; set; }
        public float? Allowance { get; set; }
        public float? SocialInsurance { get; set; }
        public float? TaxPay { get; set; }
        public float? SalaryBase { get; set; }
        public float? TotalSalaryReality { get; set; }
        public DateTime? DateSalary { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Status { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
