using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IAccessaryRepository
    {
        #region CRUD TABLE Accessary
        Task<TemplateApi> DeleteAccessary(Guid IdAccessary, Guid IdUserCurrent);
        Task<TemplateApi> DeleteAccessaryByList(List<Guid> IdAccessary, Guid IdUserCurrent);
        Task<TemplateApi> UpdateAccessary(AccessaryDto AccessaryDto);
        Task<TemplateApi> InsertAccessary(AccessaryDto AccessaryDto);
        Task<TemplateApi> GetAllAccessaryByGroupID(int pageNumber, int pageSize, Guid GroupID);
        Task<TemplateApi> GetAllAccessary(int pageNumber, int pageSize);
        Task<TemplateApi> GetAccessaryById(Guid IdAccessary);
        public Task<List<Tuple<Guid, String, String>>> InventoryAlert();
        #endregion
    }
}
