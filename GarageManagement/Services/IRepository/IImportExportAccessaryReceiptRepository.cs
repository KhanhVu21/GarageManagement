using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IImportExportAccessaryReceiptRepository
    {
        #region CRUD TABLE ImportExportAccessaryReceipt
        Task<TemplateApi> DeleteImportExportAccessaryReceipt(Guid IdImportExportAccessaryReceipt, Guid IdUserCurrent);
        Task<TemplateApi> DeleteImportExportAccessaryReceiptByList(List<Guid> IdImportExportAccessaryReceipt, Guid IdUserCurrent);
        Task<TemplateApi> UpdateImportExportAccessaryReceipt(ImportExportAccessaryReceiptDto ImportExportAccessaryReceiptDto);
        Task<TemplateApi> InsertImportExportAccessaryReceipt(ImportExportAccessaryReceiptDto ImportExportAccessaryReceiptDto);
        Task<TemplateApi> GetAllImportExportAccessaryReceiptByIdAccessary(int pageNumber, int pageSize, Guid IdAccessary);
        Task<TemplateApi> GetAllImportExportAccessaryReceiptByIdEmployee(int pageNumber, int pageSize, Guid IdEmployee);
        Task<TemplateApi> GetAllImportExportAccessaryReceipt(int pageNumber, int pageSize);
        Task<TemplateApi> GetImportExportAccessaryReceiptById(Guid IdImportExportAccessaryReceipt);
        #endregion
    }
}
