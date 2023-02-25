using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IRepairScheduleRepository
    {
        #region CRUD TABLE RepairSchedule
        Task<TemplateApi> UpdateRepairSchedule(RepairScheduleDto RepairScheduleDto);
        Task<TemplateApi> InsertRepairSchedule(RepairScheduleDto RepairScheduleDto);
        Task<TemplateApi> UpdateRepairScheduleClient(RepairScheduleDto RepairScheduleDto, String cusCode);
        Task<TemplateApi> InsertRepairScheduleClient(RepairScheduleDto RepairScheduleDto);
        Task<TemplateApi> GetAllRepairSchedule(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllRepairScheduleByIdCustomer(String cusCode, int pageNumber, int pageSize);
        Task<TemplateApi> GetRepairScheduleById(Guid IdRepairSchedule);
        Task<TemplateApi> AcceptRepairScheduleByList(List<Guid> IdRepairSchedule, Guid IdUserCurrent, bool IsAccepted);
        Task<TemplateApi> CancelRepairScheduleByList(List<Guid> IdRepairSchedule, Guid IdUserCurrent, bool IsCancel, string reason);
        Task<TemplateApi> WaitingRepairScheduleByList(List<Guid> IdRepairSchedule, Guid IdUserCurrent, bool IsWaiting);
        Task<Guid> GetIdCustomerByCode(string Code);
        #endregion
    }
}
