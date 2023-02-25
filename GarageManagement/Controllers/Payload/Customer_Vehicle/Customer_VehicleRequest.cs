namespace GarageManagement.Controllers.Payload.Customer_Vehicle
{
    public class Customer_VehicleRequest
    {
        public Guid? Id { get; set; }
        public Guid? IdCustomer { get; set; }
        public Guid? IdVehicle { get; set; }
    }
}
