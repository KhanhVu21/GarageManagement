using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IDebtRepository
    {
        #region CRUD TABLE Debt
        Task<TemplateApi> DeleteDebt(Guid IdDebt, Guid IdUserCurrent);
        Task<TemplateApi> DeleteDebtByList(List<Guid> IdDebt, Guid IdUserCurrent);
        Task<TemplateApi> UpdateDebt(DebtDto DebtDto);
        Task<TemplateApi> InsertDebt(DebtDto DebtDto);
        Task<TemplateApi> GetAllDebt(int pageNumber, int pageSize);
        Task<TemplateApi> GetDebtById(Guid IdDebt);
        #endregion
    }
}
