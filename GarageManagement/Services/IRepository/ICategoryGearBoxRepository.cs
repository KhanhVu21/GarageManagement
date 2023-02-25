using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface ICategoryGearBoxRepository
    {
        #region CRUD TABLE CategoryGearBox
        Task<TemplateApi> UpdateCategoryGearBox(CategoryGearBoxDto CategoryGearBoxDto);
        Task<TemplateApi> InsertCategoryGearBox(CategoryGearBoxDto CategoryGearBoxDto);
        Task<TemplateApi> DeleteCategoryGearBox(Guid IdCategoryGearBox, Guid IdUserCurrent);
        Task<TemplateApi> DeleteCategoryGearBoxByList(List<Guid> IdCategoryGearBox, Guid IdUserCurrent);
        Task<TemplateApi> GetAllCategoryGearBox(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllCategoryAvailable(int pageNumber, int pageSize);
        Task<TemplateApi> GetCategoryGearBoxById(Guid IdCategoryGearBox);
        Task<TemplateApi> HideCategoryGearBox(Guid IdCategoryGearBox, Guid IdUserCurrent, bool IsHide);
        Task<TemplateApi> HideCategoryGearBoxByList(List<Guid> IdCategoryGearBox, Guid IdUserCurrent, bool IsHide);
        #endregion
    }
}
