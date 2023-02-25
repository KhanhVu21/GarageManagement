using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface ICategoryModelRepository
    {
        #region CRUD TABLE CategoryModel
        Task<TemplateApi> UpdateCategoryModel(CategoryModelDto CategoryModelDto);
        Task<TemplateApi> InsertCategoryModel(CategoryModelDto CategoryModelDto);
        Task<TemplateApi> DeleteCategoryModel(Guid IdCategoryModel, Guid IdUserCurrent);
        Task<TemplateApi> DeleteCategoryModelByList(List<Guid> IdCategoryModel, Guid IdUserCurrent);
        Task<TemplateApi> GetAllCategoryModel(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllCategoryModelAvailable(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllCategoryModelByIdIdCategoryBrandVehicle(int pageNumber, int pageSize, Guid IdCategoryBrandVehicle);
        Task<TemplateApi> GetCategoryModelById(Guid IdCategoryModel);
        Task<TemplateApi> HideCategoryModel(Guid IdCategoryModel, Guid IdUserCurrent, bool IsHide);
        Task<TemplateApi> HideCategoryModelByList(List<Guid> IdCategoryModel, Guid IdUserCurrent, bool IsHide);
        #endregion
    }
}
