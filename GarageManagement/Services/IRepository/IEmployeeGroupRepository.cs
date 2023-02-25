using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IEmployeeGroupRepository
    {
        #region CRUD TABLE EmployeeGroup
        Task<TemplateApi> UpdateEmployeeGroup(EmployeeGroupDto EmployeeGroupDto);
        Task<TemplateApi> InsertEmployeeGroup(EmployeeGroupDto EmployeeGroupDto);
        Task<TemplateApi> DeleteEmployeeGroup(Guid IdEmployeeGroup, Guid IdUserCurrent);
        Task<TemplateApi> DeleteEmployeeGroupByList(List<Guid> IdEmployeeGroup, Guid IdUserCurrent);
        Task<TemplateApi> GetAllEmployeeGroupAndEmployees(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllEmployeeGroup(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllEmployeeGroupAvailable(int pageNumber, int pageSize);
        Task<TemplateApi> GetEmployeeGroupById(Guid IdEmployeeGroup);
        Task<TemplateApi> HideEmployeeGroup(Guid IdEmployeeGroup, Guid IdUserCurrent, bool IsHide);
        Task<TemplateApi> HideEmployeeGroupByList(List<Guid> IdEmployeeGroup, Guid IdUserCurrent, bool IsHide);
        #endregion
    }
}
