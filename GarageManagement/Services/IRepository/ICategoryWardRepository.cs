using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface ICategoryWardRepository
    {
        #region CRUD TABLE CategoryWard
        Task<TemplateApi> UpdateCategoryWard(CategoryWardDto CategoryWardDto);
        Task<TemplateApi> InsertCategoryWard(CategoryWardDto CategoryWardDto);
        Task<TemplateApi> DeleteCategoryWard(Guid IdCategoryWard, Guid IdUserCurrent);
        Task<TemplateApi> DeleteCategoryWardByList(List<Guid> IdCategoryWard, Guid IdUserCurrent);
        Task<TemplateApi> GetAllCategoryWard(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllCategoryWardByIdDistrict(int pageNumber, int pageSize, string DistrictCode);
        Task<TemplateApi> GetCategoryWardById(Guid IdCategoryWard);
        Task<TemplateApi> HideCategoryWard(Guid IdCategoryWard, Guid IdUserCurrent, bool IsHide);
        Task<TemplateApi> HideCategoryWardByList(List<Guid> IdCategoryWard, Guid IdUserCurrent, bool IsHide);
        #endregion
    }
}
