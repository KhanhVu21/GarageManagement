using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IRequestListRepository
    {
        #region CRUD TABLE RequestList
        Task<TemplateApi> DeleteRequestList(Guid IdRequestList, Guid IdUserCurrent);
        Task<TemplateApi> DeleteRequestListByList(List<Guid> IdRequestList, Guid IdUserCurrent);
        Task<TemplateApi> UpdateRequestList(RequestListDto RequestListDto);
        Task<TemplateApi> InsertRequestList(RequestListDto RequestListDto);
        Task<TemplateApi> GetAllRequestListById_RO_RepairOrders(int pageNumber, int pageSize, Guid Id_RO_RepairOrders);
        Task<TemplateApi> GetAllRequestList(int pageNumber, int pageSize);
        Task<TemplateApi> GetRequestListById(Guid IdRequestList);
        Task<TemplateApi> ProcessRequestList(Guid IdRequestList, Guid IdUserCurrent, bool IsProcessing);
        Task<TemplateApi> ProcessRequestListByList(List<Guid> IdRequestList, Guid IdUserCurrent, bool IsProcessing);
        Task<TemplateApi> CompleteRequestList(Guid IdRequestList, Guid IdUserCurrent, bool IsCompleted);
        Task<TemplateApi> CompleteRequestListByList(List<Guid> IdRequestList, Guid IdUserCurrent, bool IsCompleted);
        Task<TemplateApi> CancelRequestList(Guid IdRequestList, Guid IdUserCurrent, bool IsCanceled);
        Task<TemplateApi> CancelRequestListByList(List<Guid> IdRequestList, Guid IdUserCurrent, bool IsCanceled);
        #endregion
    }
}
