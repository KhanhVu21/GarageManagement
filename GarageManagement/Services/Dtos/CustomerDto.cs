namespace GarageManagement.Services.Dtos
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? Sex { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public Guid? IdCity { get; set; }
        public Guid? IdDistrict { get; set; }
        public Guid? IdWard { get; set; }
        public string? TaxNumber { get; set; }
        public string? AccountNumber { get; set; }
        public string? Note { get; set; }
        public string? Avatar { get; set; }
        public bool? TypeOfCustomer { get; set; }
        public Guid? IdGroup { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Otp { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
        public Guid? idFile { get; set; }
        public float? Deposit { get; set; }
    }
}
