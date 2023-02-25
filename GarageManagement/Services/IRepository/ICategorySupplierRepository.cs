using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface ICategorySupplierRepository
    {
        #region CRUD TABLE CategorySupplier
        Task<TemplateApi> DeleteCategorySupplier(Guid IdCategorySupplier, Guid IdUserCurrent);
        Task<TemplateApi> DeleteCategorySupplierByList(List<Guid> IdCategorySupplier, Guid IdUserCurrent);
        Task<TemplateApi> UpdateCategorySupplier(CategorySupplierDto CategorySupplierDto);
        Task<TemplateApi> InsertCategorySupplier(CategorySupplierDto CategorySupplierDto);
        Task<TemplateApi> GetAllCategorySupplier(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllCategorySupplierAvailable(int pageNumber, int pageSize);
        Task<TemplateApi> GetCategorySupplierById(Guid IdCategorySupplier);
        Task<TemplateApi> HideCategorySupplier(Guid IdCategorySupplier, Guid IdUserCurrent, bool IsHide);
        Task<TemplateApi> HideCategorySupplierByList(List<Guid> IdCategorySupplier, Guid IdUserCurrent, bool IsHide);
        #endregion
    }
}
