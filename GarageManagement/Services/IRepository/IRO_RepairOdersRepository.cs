using GarageManagement.Controllers.Payload.RO_RepairOders;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IRO_RepairOdersRepository
    {
        #region CRUD TABLE RO_RepairOders
        Task<TemplateApi> UpdateRO_RepairOders(RepairOrderPayload repairOrderPayload, Guid IdUserCurrent);
        Task<TemplateApi> DeleteRO_RepairOders(Guid IdRO_RepairOders, Guid IdUserCurrent);
        Task<TemplateApi> DeleteRO_RepairOdersByList(List<Guid> IdRO_RepairOders, Guid IdUserCurrent);
        Task<TemplateApi> GetAllRO_RepairOders(int pageNumber, int pageSize, bool IsPaid);
        Task<TemplateApi> GetAllRO_RepairOdersByIdCustomer(int pageNumber, int pageSize, Guid CustomerId);
        Task<TemplateApi> GetRO_RepairOdersById(Guid IdRO_RepairOders);
        Task<TemplateApi> InsertRO_RepairOders(RepairOrderPayload repairOrderPayload, Guid IdUserCurrent);
        Task<TemplateApi> PayRepairOders(float totalMoneys, Guid IdRepairOrder, Guid IdUserCurrent, bool continueDeposit);
        #endregion
    }
}
