using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IOrtherCostRepository
    {
        #region CRUD TABLE OrtherCost
        Task<TemplateApi> DeleteOrtherCost(Guid IdOrtherCost, Guid IdUserCurrent);
        Task<TemplateApi> DeleteOrtherCostByList(List<Guid> IdOrtherCost, Guid IdUserCurrent);
        Task<TemplateApi> UpdateOrtherCost(OrtherCostDto OrtherCostDto);
        Task<TemplateApi> InsertOrtherCost(OrtherCostDto OrtherCostDto);
        Task<TemplateApi> GetAllOrtherCostByIdRepairOders(int pageNumber, int pageSize, Guid IdRepairOders);
        Task<TemplateApi> GetAllOrtherCost(int pageNumber, int pageSize);
        Task<TemplateApi> GetOrtherCostById(Guid IdOrtherCost);
        #endregion
    }
}
