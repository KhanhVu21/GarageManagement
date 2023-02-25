namespace GarageManagement.Data.Entity
{
    public class Employee_Allowance
    {
        public Guid Id { get; set; }
        public Guid? IdAllowance { get; set; }
        public Guid? IdEmployee { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
