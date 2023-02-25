using GarageManagement.Data.Entity;
using Newtonsoft.Json;

namespace GarageManagement.Services.Dtos
{
    public class VehicleDto
    {
        public Guid Id { get; set; }
        public string? NameVehicle { get; set; }
        public string? ChassisNumber { get; set; }
        public string? EngineNumber { get; set; }
        public string? LicensePlates { get; set; }
        public Guid? IdBrandVehicleCategory { get; set; }
        public Guid? IdModel { get; set; }
        public Guid? IdType { get; set; }
        public Guid? IdGearBox { get; set; }
        public string? EngineCapacity { get; set; }
        public string? Color { get; set; }
        public int? Status { get; set; }
        public bool? IsHide { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? YearOfManufacture { get; set; }
        public string? Avatar { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
        public string? ListIdCustomer { get; set; }
        public List<Customer>? ListCustomers { get; set; }
        public Guid? idFile { get; set; }

    }
}
