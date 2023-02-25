using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface ICustomerGroupRepository
    {
        #region CRUD TABLE CustomerGroup
        Task<TemplateApi> UpdateCustomerGroup(CustomerGroupDto CustomerGroupDto);
        Task<TemplateApi> InsertCustomerGroup(CustomerGroupDto CustomerGroupDto);
        Task<TemplateApi> DeleteCustomerGroup(Guid IdCustomerGroup, Guid IdUserCurrent);
        Task<TemplateApi> DeleteCustomerGroupByList(List<Guid> IdCustomerGroup, Guid IdUserCurrent);
        Task<TemplateApi> GetAllCustomerGroupAndCustomer(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllCustomerGroup(int pageNumber, int pageSize);
        Task<TemplateApi> GetAllCustomerGroupAvailable(int pageNumber, int pageSize);
        Task<TemplateApi> GetCustomerGroupById(Guid IdCustomerGroup);
        Task<TemplateApi> HideCustomerGroup(Guid IdCustomerGroup, Guid IdUserCurrent, bool IsHide);
        Task<TemplateApi> HideCustomerGroupByList(List<Guid> IdCustomerGroup, Guid IdUserCurrent, bool IsHide);
        #endregion
    }
}
