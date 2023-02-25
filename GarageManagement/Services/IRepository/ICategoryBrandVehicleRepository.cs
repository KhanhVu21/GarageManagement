using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface ICategoryBrandVehicleRepository
    {
        #region CRUD TABLE CategoryBrandVehicle
        Task<TemplateApi> DeleteCategoryBrandVehicle(Guid IdCategoryBrandVehicle, Guid IdUserCurrent);
        Task<TemplateApi> DeleteCategoryBrandVehicleByList(List<Guid> IdCategoryBrandVehicle, Guid IdUserCurrent);
        Task<TemplateApi> UpdateCategoryBrandVehicle(CategoryBrandVehicleDto CategoryBrandVehicleDto);
        Task<TemplateApi> InsertCategoryBrandVehicle(CategoryBrandVehicleDto CategoryBrandVehicleDto);
        Task<TemplateApi> GetAllCategoryBrandVehicle(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllCategoryBrandVehicleAvailable(int pageNumber, int pageSize); 
        Task<TemplateApi> GetCategoryBrandVehicleById(Guid IdCategoryBrandVehicle);
        Task<TemplateApi> HideCategoryBrandVehicle(Guid IdCategoryBrandVehicle, Guid IdUserCurrent, bool IsHide);
        Task<TemplateApi> HideCategoryBrandVehicleByList(List<Guid> IdCategoryBrandVehicle, Guid IdUserCurrent, bool IsHide);
        #endregion
    }
}
