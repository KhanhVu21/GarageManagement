using GarageManagement.Services.Common.Model;

namespace GarageManagement.Services.IRepository
{
    public interface IStaticsRepository
    {
        Task<TemplateApi> GetRevenueStatistical(string fromDate, string toDate);
    }
}
