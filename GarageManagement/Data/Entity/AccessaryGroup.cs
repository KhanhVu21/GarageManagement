namespace GarageManagement.Data.Entity
{
    public class AccessaryGroup
    {
        public Guid Id { get; set; }
        public string? GroupName { get; set; }
        public string? Code { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
