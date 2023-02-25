using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface ICategoryDistrictRepository
    {
        #region CRUD TABLE CategoryDistrict
        Task<TemplateApi> UpdateCategoryDistrict(CategoryDistrictDto CategoryDistrictDto);
        Task<TemplateApi> InsertCategoryDistrict(CategoryDistrictDto CategoryDistrictDto);
        Task<TemplateApi> DeleteCategoryDistrict(Guid IdCategoryDistrict, Guid IdUserCurrent);
        Task<TemplateApi> DeleteCategoryDistrictByList(List<Guid> IdCategoryDistrict, Guid IdUserCurrent);
        Task<TemplateApi> GetAllCategoryDistrictByIdCity(int pageNumber, int pageSize, string CityCode);
        Task<TemplateApi> GetAllCategoryDistrict(int pageNumber, int pageSize);
        Task<TemplateApi> GetCategoryDistrictById(Guid IdCategoryDistrict);
        Task<TemplateApi> HideCategoryDistrict(Guid IdCategoryDistrict, Guid IdUserCurrent, bool IsHide);
        Task<TemplateApi> HideCategoryDistrictByList(List<Guid> IdCategoryDistrict, Guid IdUserCurrent, bool IsHide);
        #endregion
    }
}
