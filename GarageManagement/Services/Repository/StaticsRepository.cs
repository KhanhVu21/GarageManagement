using Dapper;
using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Mapster;
using System.Data;

namespace GarageManagement.Services.Repository
{
    public class StaticsRepository: IStaticsRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public StaticsRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetRevenueStatistical(string fromDate, string toDate)
        {
            using var con = _DbContext.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@fromDate", fromDate);
            parameters.Add("@toDate", toDate);

            var revenueStatistical = await con.QueryFirstAsync<RevenueStatistical>(
                QueryStatics.queryGM_SP_RevenueStatistical,
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            revenueStatistical.Revenue = revenueStatistical.Repair + revenueStatistical.Sell;
            revenueStatistical.TotalVehicle = revenueStatistical.ProcessingVehicle + revenueStatistical.CompletedVehicle;

            var templateApi = revenueStatistical != null
                ? new TemplateApi()
                {
                    Payload = revenueStatistical,
                    Message = "Lấy thông tin thành công",
                    Success = true,
                    Fail = false,
                    TotalElement = 1,
                }
                : new TemplateApi()
                {
                    Message = "Không tìm thấy kết quả",
                    Success = false,
                    Fail = true,
                };
            return templateApi;
        }
        #endregion

        internal static class QueryStatics
        {
            public const string queryGM_SP_RevenueStatistical = "[dbo].[GM_SP_RevenueStatistical]";
        }
    }
}
