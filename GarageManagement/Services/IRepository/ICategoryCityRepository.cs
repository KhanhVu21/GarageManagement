using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface ICategoryCityRepository
    {
        #region CRUD TABLE CategoryCity
        Task<TemplateApi> UpdateCategoryCity(CategoryCityDto CategoryCityDto);
        Task<TemplateApi> InsertCategoryCity(CategoryCityDto CategoryCityDto);
        Task<TemplateApi> DeleteCategoryCity(Guid IdCategoryCity, Guid IdUserCurrent);
        Task<TemplateApi> DeleteCategoryCityByList(List<Guid> IdCategoryCity, Guid IdUserCurrent);
        Task<TemplateApi> GetAllCategoryCity(int pageNumber, int pageSize);
        Task<TemplateApi> GetCategoryCityById(Guid IdCategoryCity);
        Task<TemplateApi> HideCategoryCity(Guid IdCategoryCity, Guid IdUserCurrent, bool IsHide);
        Task<TemplateApi> HideCategoryCityByList(List<Guid> IdCategoryCity, Guid IdUserCurrent, bool IsHide);
        #endregion
    }
}
