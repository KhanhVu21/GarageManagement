using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface ICategoryTypeRepository
    {
        #region CRUD TABLE CategoryType
        Task<TemplateApi> UpdateCategoryType(CategoryTypeDto CategoryTypeDto);
        Task<TemplateApi> InsertCategoryType(CategoryTypeDto CategoryTypeDto);
        Task<TemplateApi> DeleteCategoryType(Guid IdCategoryType, Guid IdUserCurrent);
        Task<TemplateApi> DeleteCategoryTypeByList(List<Guid> IdCategoryType, Guid IdUserCurrent);
        Task<TemplateApi> GetAllCategoryType(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllCategoryTypeAvailable(int pageNumber, int pageSize);
        Task<TemplateApi> GetCategoryTypeById(Guid IdCategoryType);
        Task<TemplateApi> HideCategoryType(Guid IdCategoryType, Guid IdUserCurrent, bool IsHide);
        Task<TemplateApi> HideCategoryTypeByList(List<Guid> IdCategoryType, Guid IdUserCurrent, bool IsHide);
        #endregion
    }
}
