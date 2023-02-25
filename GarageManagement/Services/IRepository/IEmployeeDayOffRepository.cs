using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IEmployeeDayOffRepository
    {
        #region CRUD TABLE EmployeeDayOff
        Task<TemplateApi> DeleteEmployeeDayOffByList(List<Guid> IdEmployeeDayOff, Guid IdUserCurrent);
        Task<TemplateApi> UpdateEmployeeDayOff(EmployeeDayOffDto EmployeeDayOffDto);
        Task<TemplateApi> InsertEmployeeDayOff(EmployeeDayOffDto EmployeeDayOffDto);
        Task<TemplateApi> GetAllEmployeeDayOff(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllEmployeeOverTime(int pageNumber, int pageSize);
        Task<TemplateApi> GetEmployeeDayOffById(Guid IdEmployeeDayOff);
        #endregion
    }
}
