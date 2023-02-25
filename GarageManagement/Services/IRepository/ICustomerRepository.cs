using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface ICustomerRepository
    {
        #region CRUD TABLE Customer
        Task<TemplateApi> UpdateCustomer(CustomerDto CustomerDto);
        Task<TemplateApi> InsertCustomer(CustomerDto CustomerDto);
        Task<TemplateApi> InsertCustomerNotToken(CustomerDto CustomerDto);
        Task<TemplateApi> DeleteCustomer(Guid IdCustomer, Guid IdUserCurrent);
        Task<TemplateApi> DeleteCustomerByList(List<Guid> IdCustomer, Guid IdUserCurrent);
        Task<TemplateApi> GetAllCustomer(int pageNumber, int pageSize);
        Task<TemplateApi> GetCustomerById(Guid IdCustomer);
        Task<TemplateApi> GetCustomerByPhoneOrByEmail(String filter, int pageNumber, int pageSize);
        Task<TemplateApi> GetAllCustomerByIdGroup(int pageNumber, int pageSize, Guid IdGroup);
        Task<TemplateApi> VerifyOtp(String cusCode, String email, String otp);
        Task<TemplateApi> RefreshOtp(String email, String random, String cuscode);
        Task<TemplateApi> GetAllRepairOrderByCustomer(String cusCode, int pageNumber, int pageSize);
        Task<TemplateApi> GetInforRepairOrderById(Guid IdRepairOrder, String cusCode);
        #endregion
    }
}
