namespace GarageManagement.Services.Dtos
{
    public class Customer_VehicleDto
    {
        public Guid Id { get; set; }
        public Guid? IdCustomer { get; set; }
        public Guid? IdVehicle { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
