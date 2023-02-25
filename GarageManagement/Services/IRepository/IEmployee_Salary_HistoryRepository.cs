using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IEmployee_Salary_HistoryRepository
    {
        #region CRUD TABLE Employee_Salary_History
        Task<TemplateApi> GetAllEmployee_Salary_History(int pageNumber, int pageSize, Guid IdEmployee);
        Task<TemplateApi> UpdateEmployee_Salary_History(Employee_Salary_HistoryDto Employee_Salary_HistoryDto);
        Task<TemplateApi> InsertEmployee_Salary_History(Employee_Salary_HistoryDto Employee_Salary_HistoryDto);
        Task<TemplateApi> GetEmployee_Salary_HistoryById(Guid IdEmployee_Salary_History);
        Task<Employee_Salary_History> Employee_Salary_HistoryByIdEmployeeAndMonth(Guid idEmployee, int month);
        Task<IEnumerable<Employee_Salary_History>> Employee_Salary_HistoryByIdEmployeeAndMonth(int month);
        #endregion
    }
}
