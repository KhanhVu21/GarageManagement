using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.EmployeeDayOff;
using GarageManagement.Controllers.Payload.Holiday;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using GarageManagement.Utility;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidayController: Controller
    {
        #region Variables
        private readonly IHolidayRepository _HolidayRepository;
        private readonly ILogger<HolidayController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public HolidayController(IHolidayRepository HolidayRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<HolidayController> logger)
        {
            _HolidayRepository = HolidayRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region METHOD
        // HttpDelete: /api/Holiday/DeleteHolidayByList
        [HttpDelete("DeleteHolidayByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteHolidayByList(List<Guid> IdHolidays)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _HolidayRepository.DeleteHolidayByList(IdHolidays, idUserCurrent);

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
        // GET: api/Holiday/GetListHoliday
        [HttpGet("GetListHoliday")]
        public async Task<IActionResult> GetListHoliday(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _HolidayRepository.GetAllHoliday(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Holiday/GetHolidayById
        [HttpGet("GetHolidayById")]
        public async Task<IActionResult> GetHolidayById(Guid IdHoliday)
        {
            TemplateApi templateApi = await _HolidayRepository.GetHolidayById(IdHoliday);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/Holiday/InsertHoliday
        [HttpPost("InsertHoliday")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertHoliday(HolidayRequest HolidayRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var HolidayDto = HolidayRequest.Adapt<HolidayDto>();

            // define some col with data concrete
            HolidayDto.Id = Guid.NewGuid();
            HolidayDto.IdUserCurrent = idUserCurrent;
            HolidayDto.CreatedDate = DateTime.Now;
            HolidayDto.Status = 0;
            HolidayDto.DateHoliday = new DateTime(HolidayRequest.DateHoliday.Value.Year, HolidayRequest.DateHoliday.Value.Month, HolidayRequest.DateHoliday.Value.Day);

            TemplateApi result = await _HolidayRepository.InsertHoliday(HolidayDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/Holiday/UpdateHoliday
        [HttpPut("UpdateHoliday")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateHoliday(HolidayRequest HolidayRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var HolidayDto = HolidayRequest.Adapt<HolidayDto>();
            HolidayDto.IdUserCurrent = idUserCurrent;
            HolidayDto.DateHoliday = new DateTime(HolidayRequest.DateHoliday.Value.Year, HolidayRequest.DateHoliday.Value.Month, HolidayRequest.DateHoliday.Value.Day);

            TemplateApi result = await _HolidayRepository.UpdateHoliday(HolidayDto);
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
