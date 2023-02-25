using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IHolidayRepository
    {
        #region CRUD TABLE Holiday
        Task<TemplateApi> DeleteHolidayByList(List<Guid> IdHoliday, Guid IdUserCurrent);
        Task<TemplateApi> UpdateHoliday(HolidayDto HolidayDto);
        Task<TemplateApi> InsertHoliday(HolidayDto HolidayDto);
        Task<TemplateApi> GetAllHoliday(int pageNumber, int pageSize);
        Task<TemplateApi> GetHolidayById(Guid IdHoliday);
        #endregion
    }
}
