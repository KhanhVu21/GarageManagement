using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IAllowanceRepository
    {
        #region CRUD TABLE Allowance
        Task<TemplateApi> DeleteAllowanceByList(List<Guid> IdAllowance, Guid IdUserCurrent);
        Task<TemplateApi> UpdateAllowance(AllowanceDto AllowanceDto);
        Task<TemplateApi> InsertAllowance(AllowanceDto AllowanceDto);
        Task<TemplateApi> GetAllAllowance(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllAllowanceByIdEmployee(int pageNumber, int pageSize, Guid IdEmployee);
        Task<TemplateApi> GetAllowanceById(Guid IdAllowance);
        Task<TemplateApi> InsertAllowanceForEmployee(Guid IdEmployee, List<Guid> IdAllowances, Guid IdUserCurrent);
        #endregion
    }
}
