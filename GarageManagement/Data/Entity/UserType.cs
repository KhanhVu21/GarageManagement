namespace GarageManagement.Data.Entity
{
    public class UserType
    {
        public Guid Id { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public int? Status { get; set; }
        public Guid? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string TypeCode { get; set; } = string.Empty;
    }
}
