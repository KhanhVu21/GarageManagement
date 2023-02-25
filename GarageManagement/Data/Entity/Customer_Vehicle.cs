namespace GarageManagement.Data.Entity
{
    public class Customer_Vehicle
    {
        public Guid Id { get; set; }
        public Guid? IdCustomer { get; set; }
        public Guid? IdVehicle { get; set; }

    }
}
