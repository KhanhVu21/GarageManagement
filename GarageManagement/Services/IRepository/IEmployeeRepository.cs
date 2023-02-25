using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IEmployeeRepository
    {
        #region CRUD TABLE Employee
        Task<TemplateApi> GetAllEmployeeByIdGroup(int pageNumber, int pageSize, Guid IdGroup);
        Task<TemplateApi> UpdateEmployee(EmployeeDto EmployeeDto);
        Task<TemplateApi> InsertEmployee(EmployeeDto EmployeeDto);
        Task<TemplateApi> DeleteEmployee(Guid IdEmployee, Guid IdUserCurrent);
        Task<TemplateApi> DeleteEmployeeByList(List<Guid> IdEmployee, Guid IdUserCurrent);
        Task<TemplateApi> GetAllEmployee(int pageNumber, int pageSize);
        Task<Employee> GetEmployeeExcel(Guid IdEmployee);
        Task<TemplateApi> GetEmployeeById(Guid IdEmployee);
        Task<TemplateApi> SalaryCalculateByMonth(Guid IdEmployees, DateTime date);
        Task<TemplateApi> SalaryCalculateAllEmployeeByMonth(DateTime date, int pageNumber, int pageSize);
        Task<TemplateApi> HideEmployee(Guid IdEmployee, Guid IdUserCurrent, bool IsHide);
        Task<TemplateApi> HideEmployeeByList(List<Guid> IdEmployee, Guid IdUserCurrent, bool IsHide);
        #endregion
    }
}
