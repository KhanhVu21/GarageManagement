using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.EmployeeDayOff;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using GarageManagement.Utility;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeDayOffController: Controller
    {
        #region Variables
        private readonly IEmployeeDayOffRepository _EmployeeDayOffRepository;
        private readonly ILogger<EmployeeDayOffController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public EmployeeDayOffController(IEmployeeDayOffRepository EmployeeDayOffRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<EmployeeDayOffController> logger)
        {
            _EmployeeDayOffRepository = EmployeeDayOffRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region METHOD
        // GET: api/EmployeeDayOff/GetListEmployeeOverTime
        [HttpGet("GetListEmployeeOverTime")]
        public async Task<IActionResult> GetListEmployeeOverTime(int pageNumber, int pageSize)
        {



            TemplateApi templateApi = await _EmployeeDayOffRepository.GetAllEmployeeOverTime(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpDelete: /api/EmployeeDayOff/DeleteEmployeeDayOffByList
        [HttpDelete("DeleteEmployeeDayOffByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteEmployeeDayOffByList(List<Guid> IdEmployeeDayOffs)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _EmployeeDayOffRepository.DeleteEmployeeDayOffByList(IdEmployeeDayOffs, idUserCurrent);

            if (result.Success)
            {
                _logger.LogInformation("Thành công : {message}", result.Message);
                return Ok(new
                {
                    Success = result.Success,
                    Fail = result.Fail,
                    Message = result.Message
                });
            }
            else
            {
                _logger.LogError("Xảy ra lỗi : {message}", result.Message);
                return Ok(new
                {
                    Success = result.Success,
                    Fail = result.Fail,
                    Message = result.Message
                });
            }
        }
        // GET: api/EmployeeDayOff/GetListEmployeeDayOff
        [HttpGet("GetListEmployeeDayOff")]
        public async Task<IActionResult> GetListEmployeeDayOff(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _EmployeeDayOffRepository.GetAllEmployeeDayOff(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/EmployeeDayOff/GetEmployeeDayOffById
        [HttpGet("GetEmployeeDayOffById")]
        public async Task<IActionResult> GetEmployeeDayOffById(Guid IdEmployeeDayOff)
        {
            TemplateApi templateApi = await _EmployeeDayOffRepository.GetEmployeeDayOffById(IdEmployeeDayOff);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/EmployeeDayOff/InsertEmployeeDayOff
        [HttpPost("InsertEmployeeDayOff")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertEmployeeDayOff(EmployeeDayOffRequest EmployeeDayOffRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var EmployeeDayOffDto = EmployeeDayOffRequest.Adapt<EmployeeDayOffDto>();

            // define some col with data concrete
            EmployeeDayOffDto.Id = Guid.NewGuid();
            EmployeeDayOffDto.IdUserCurrent = idUserCurrent;
            EmployeeDayOffDto.CreatedDate = DateTime.Now;
            EmployeeDayOffDto.Status = 0;
            EmployeeDayOffDto.Dayoff = new DateTime(EmployeeDayOffRequest.Dayoff.Value.Year, EmployeeDayOffRequest.Dayoff.Value.Month, EmployeeDayOffRequest.Dayoff.Value.Day);

            TemplateApi result = await _EmployeeDayOffRepository.InsertEmployeeDayOff(EmployeeDayOffDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/EmployeeDayOff/UpdateEmployeeDayOff
        [HttpPut("UpdateEmployeeDayOff")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateEmployeeDayOff(EmployeeDayOffRequest EmployeeDayOffRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var EmployeeDayOffDto = EmployeeDayOffRequest.Adapt<EmployeeDayOffDto>();
            EmployeeDayOffDto.IdUserCurrent = idUserCurrent;
            EmployeeDayOffDto.Dayoff = new DateTime(EmployeeDayOffRequest.Dayoff.Value.Year, EmployeeDayOffRequest.Dayoff.Value.Month, EmployeeDayOffRequest.Dayoff.Value.Day);

            TemplateApi result = await _EmployeeDayOffRepository.UpdateEmployeeDayOff(EmployeeDayOffDto);
            if (result.Success)
            {
                _logger.LogInformation("Thành công : {message}", result.Message);
                return Ok(new
                {
                    Success = result.Success,
                    Fail = result.Fail,
                    Message = result.Message
                });
            }
            else
            {
                _logger.LogError("Xảy ra lỗi : {message}", result.Message);
                return Ok(new
                {
                    Success = result.Success,
                    Fail = result.Fail,
                    Message = result.Message
                });
            }
        }
        #endregion
    }
}
