namespace GarageManagement.Data.Entity
{
    public class CategoryGearBox
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int? Status { get; set; }
        public bool? IsHide { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
