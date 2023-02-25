using GarageManagement.Controllers.Payload.Accessary;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using GarageManagement.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaticsController: Controller
    {
        #region Variables
        private readonly IStaticsRepository _staticsRepository;
        private readonly ILogger<StaticsController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public StaticsController(IStaticsRepository staticsRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<StaticsController> logger)
        {
            _staticsRepository = staticsRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region METHOD
        // GET: api/Statics/GetRevenueStatistical
        [HttpGet("GetRevenueStatistical")]
        public async Task<IActionResult> GetRevenueStatistical(string fromDate, string toDate)
        {
            TemplateApi templateApi = await _staticsRepository.GetRevenueStatistical(fromDate, toDate);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        #endregion
    }
}
