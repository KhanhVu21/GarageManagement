using GarageManagement.Data.Entity;

namespace GarageManagement.Services.Dtos
{
    public class CustomerGroupDto
    {
        public Guid Id { get; set; }
        public string? GroupName { get; set; }
        public string? GroupCode { get; set; }
        public int? Status { get; set; }
        public bool? IsHide { get; set; }
        public DateTime? CreateDate { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
        public List<Customer> customers { get; set; }
    }
}
