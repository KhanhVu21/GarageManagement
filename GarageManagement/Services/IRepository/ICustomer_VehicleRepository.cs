using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface ICustomer_VehicleRepository
    {
        #region CRUD TABLE Customer_Vehicle
        Task<TemplateApi> UpdateCustomer_Vehicle(Customer_VehicleDto Customer_VehicleDto);
        Task<TemplateApi> InsertCustomer_Vehicle(Customer_VehicleDto Customer_VehicleDto);
        Task<TemplateApi> DeleteCustomer_Vehicle(Guid IdCustomer_Vehicle, Guid IdUserCurrent);
        Task<TemplateApi> DeleteCustomer_VehicleByList(List<Guid> IdCustomer_Vehicle, Guid IdUserCurrent);
        Task<TemplateApi> GetAllCustomer_Vehicle(int pageNumber, int pageSize);
        Task<TemplateApi> GetCustomer_VehicleById(Guid IdCustomer_Vehicle);
        #endregion
    }
}
