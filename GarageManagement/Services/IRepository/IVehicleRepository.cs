using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IVehicleRepository
    {
        #region CRUD TABLE Vehicle
        Task<TemplateApi> UpdateVehicle(VehicleDto VehicleDto);
        Task<TemplateApi> InsertVehicle(VehicleDto VehicleDto);
        Task<TemplateApi> DeleteVehicle(Guid IdVehicle, Guid IdUserCurrent);
        Task<TemplateApi> DeleteVehicleByList(List<Guid> IdVehicle, Guid IdUserCurrent);
        Task<TemplateApi> GetAllCustomerByIdVehicle(int pageNumber, int pageSize, Guid IdVehicle);
        Task<TemplateApi> GetAllVehicleByIdCustomer(int pageNumber, int pageSize, Guid IdCustomer);
        Task<TemplateApi> GetAllVehicle(int pageNumber, int pageSize);
        Task<TemplateApi> GetVehicleById(Guid IdVehicle);
        Task<TemplateApi> HideVehicle(Guid IdVehicle, Guid IdUserCurrent, bool IsHide);
        Task<TemplateApi> HideVehicleByList(List<Guid> IdVehicle, Guid IdUserCurrent, bool IsHide);
        #endregion
    }
}
