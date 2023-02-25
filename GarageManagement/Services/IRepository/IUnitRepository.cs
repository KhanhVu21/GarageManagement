using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IUnitRepository
    {
        #region CRUD TABLE UNIT
        Task<TemplateApi> UpdateUnit(UnitDto unitDto);
        Task<TemplateApi> InsertUnit(UnitDto unitDto);
        Task<TemplateApi> DeleteUnit(Guid IdUnit, Guid IdUserCurrent);
        Task<TemplateApi> GetAllUnit(int pageNumber, int pageSize);
        Task<TemplateApi> GetUnitById(Guid IdUnit);
        Task<TemplateApi> HideUnit(Guid IdUnit, bool IsHide, Guid IdUserCurrent);
        Task<TemplateApi> GetUnitNotHide(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllUnitByIdParent(Guid IdUnit, int pageNumber, int pageSize);
        Task<UnitDto> GetUnitByUnitCode(string UnitCode);
        Task<TemplateApi> GetAllUnitAndUser(int pageNumber, int pageSize);
        #endregion
    }
}
