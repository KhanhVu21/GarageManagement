using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IAccessaryGroupRepository
    {
        #region CRUD TABLE AccessaryGroup
        Task<TemplateApi> DeleteAccessaryGroup(Guid IdAccessaryGroup, Guid IdUserCurrent);
        Task<TemplateApi> DeleteAccessaryGroupByList(List<Guid> IdAccessaryGroup, Guid IdUserCurrent);
        Task<TemplateApi> UpdateAccessaryGroup(AccessaryGroupDto AccessaryGroupDto);
        Task<TemplateApi> InsertAccessaryGroup(AccessaryGroupDto AccessaryGroupDto);
        Task<TemplateApi> GetAccessaryGroupById(Guid IdAccessaryGroup);
        Task<TemplateApi> GetAllAccessaryGroup(int pageNumber, int pageSize);
        #endregion
    }
}
