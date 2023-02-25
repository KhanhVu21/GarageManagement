using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.RepairSchedule;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using GarageManagement.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mapster;
using GarageManagement.Services.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Globalization;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepairScheduleController: Controller
    {
        #region Variables
        private readonly IRepairScheduleRepository _repairScheduleRepository;
        private readonly ILogger<RepairScheduleController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public RepairScheduleController(IRepairScheduleRepository repairScheduleRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<RepairScheduleController> logger)
        {
            _repairScheduleRepository = repairScheduleRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region Method
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        #endregion

        #region METHOD
        // HttpPut: /api/RepairSchedule/CancelRepairScheduleByList
        [HttpPut("CancelRepairScheduleByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> CancelRepairScheduleByList(List<Guid> IdRepairSchedule, bool IsCanceled, string reason)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _repairScheduleRepository.CancelRepairScheduleByList(IdRepairSchedule, idUserCurrent, IsCanceled, reason);
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
        // HttpPut: /api/RepairSchedule/AcceptRepairScheduleByList
        [HttpPut("AcceptRepairScheduleByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> AcceptRepairScheduleByList(List<Guid> IdRepairSchedule, bool IsAccepted)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _repairScheduleRepository.AcceptRepairScheduleByList(IdRepairSchedule, idUserCurrent, IsAccepted);
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
        // HttpPut: /api/RepairSchedule/WaitRepairScheduleByList
        [HttpPut("WaitRepairScheduleByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> WaitRepairScheduleByList(List<Guid> IdRepairSchedule, bool IsWaiting)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _repairScheduleRepository.WaitingRepairScheduleByList(IdRepairSchedule, idUserCurrent, IsWaiting);
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
        // GET: api/RepairSchedule/GetListRepairSchedule
        [HttpGet("GetListRepairSchedule")]
        public async Task<IActionResult> GetListRepairSchedule(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _repairScheduleRepository.GetAllRepairSchedule(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/RepairSchedule/GetRepairScheduleById
        [HttpGet("GetRepairScheduleById")]
        public async Task<IActionResult> GetRepairScheduleById(Guid IdRepairSchedule)
        {
            TemplateApi templateApi = await _repairScheduleRepository.GetRepairScheduleById(IdRepairSchedule);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/RepairSchedule/InsertRepairSchedule
        [HttpPost("InsertRepairSchedule")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertRepairSchedule(RepairScheduleRequest RepairScheduleRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var RepairScheduleDto = RepairScheduleRequest.Adapt<RepairScheduleDto>();

            // define some col with data concrete
            RepairScheduleDto.Id = Guid.NewGuid();
            RepairScheduleDto.IdUserCurrent = idUserCurrent;
            RepairScheduleDto.CreatedDate = DateTime.Now;
            RepairScheduleDto.Status = 0;
            RepairScheduleDto.IsWaiting = true;
            RepairScheduleDto.IsCancel = false;
            RepairScheduleDto.IsAccepted = false;
            RepairScheduleDto.NoteGarage = null;

            if (RepairScheduleRequest.DaySchedule is not null)
            {
                string format = "dd/MM/yyyy HH:mm";
                DateTime convertTime = DateTime.ParseExact(RepairScheduleRequest.DaySchedule, format, CultureInfo.InvariantCulture);
                RepairScheduleDto.DaySchedule = convertTime;
            }

            TemplateApi result = await _repairScheduleRepository.InsertRepairSchedule(RepairScheduleDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/RepairSchedule/UpdateRepairSchedule
        [HttpPut("UpdateRepairSchedule")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateRepairSchedule(RepairScheduleRequest RepairScheduleRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;


            var RepairScheduleDto = RepairScheduleRequest.Adapt<RepairScheduleDto>();
            if (RepairScheduleRequest.DaySchedule is not null)
            {
                string format = "dd/MM/yyyy HH:mm";
                DateTime convertTime = DateTime.ParseExact(RepairScheduleRequest.DaySchedule, format, CultureInfo.InvariantCulture);
                RepairScheduleDto.DaySchedule = convertTime;
            }
            RepairScheduleDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _repairScheduleRepository.UpdateRepairSchedule(RepairScheduleDto);
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
        // GET: api/RepairSchedule/GetListRepairScheduleByIdCustomer
        [HttpGet("GetListRepairScheduleByIdCustomer")]
        public async Task<IActionResult> GetListRepairScheduleByIdCustomer(int pageNumber, int pageSize)
        {
            var jwt = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (jwt is null)
            {
                return Ok(new { Message = "Không tìm thấy token !", Success = false, Fail = true });
            }

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? tokenS = handler.ReadToken(jwt) as JwtSecurityToken;
            string expire = tokenS.Claims.First(claim => claim.Type == "exp").Value;

            double doubleVal = Convert.ToDouble(expire);
            DateTime DateAfterConvert = UnixTimeStampToDateTime(doubleVal);

            if (DateAfterConvert < DateTime.Now)
            {
                return Ok(new
                {
                    Message = "Token đã hết hạn !",
                    Fail = true,
                    Success = false
                });
            }

            string cusCode = tokenS.Claims.First(claim => claim.Type == "sub").Value;

            TemplateApi templateApi = await _repairScheduleRepository.GetAllRepairScheduleByIdCustomer(cusCode, pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/RepairSchedule/InsertRepairScheduleClient
        [HttpPost("InsertRepairScheduleClient")]
        public async Task<IActionResult> InsertRepairScheduleClient(RepairScheduleRequest RepairScheduleRequest)
        {
            var jwt = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (jwt is null)
            {
                return Ok(new { Message = "Không tìm thấy token !", Success = false, Fail = true });
            }

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? tokenS = handler.ReadToken(jwt) as JwtSecurityToken;
            string expire = tokenS.Claims.First(claim => claim.Type == "exp").Value;

            double doubleVal = Convert.ToDouble(expire);
            DateTime DateAfterConvert = UnixTimeStampToDateTime(doubleVal);

            if (DateAfterConvert < DateTime.Now)
            {
                return Ok(new
                {
                    Message = "Token đã hết hạn !",
                    Fail = true,
                    Success = false
                });
            }

            string cusCode = tokenS.Claims.First(claim => claim.Type == "sub").Value;

            var RepairScheduleDto = RepairScheduleRequest.Adapt<RepairScheduleDto>();

            // define some col with data concrete
            RepairScheduleDto.Id = Guid.NewGuid();
            RepairScheduleDto.CreatedDate = DateTime.Now;
            RepairScheduleDto.Status = 0;
            RepairScheduleDto.IsWaiting = true;
            RepairScheduleDto.IsCancel = false;
            RepairScheduleDto.IsAccepted = false;
            RepairScheduleDto.IdCustomer = await _repairScheduleRepository.GetIdCustomerByCode(cusCode);
            if (RepairScheduleRequest.DaySchedule is not null)
            {
                string format = "dd/MM/yyyy HH:mm";
                DateTime convertTime = DateTime.ParseExact(RepairScheduleRequest.DaySchedule, format, CultureInfo.InvariantCulture);
                RepairScheduleDto.DaySchedule = convertTime;
            }

            TemplateApi result = await _repairScheduleRepository.InsertRepairScheduleClient(RepairScheduleDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/RepairSchedule/UpdateRepairScheduleClient
        [HttpPut("UpdateRepairScheduleClient")]
        public async Task<IActionResult> UpdateRepairScheduleClient(RepairScheduleRequest RepairScheduleRequest)
        {
            var jwt = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (jwt is null)
            {
                return Ok(new { Message = "Không tìm thấy token !", Success = false, Fail = true });
            }

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? tokenS = handler.ReadToken(jwt) as JwtSecurityToken;
            string expire = tokenS.Claims.First(claim => claim.Type == "exp").Value;

            double doubleVal = Convert.ToDouble(expire);
            DateTime DateAfterConvert = UnixTimeStampToDateTime(doubleVal);

            if (DateAfterConvert < DateTime.Now)
            {
                return Ok(new
                {
                    Message = "Token đã hết hạn !",
                    Fail = true,
                    Success = false
                });
            }

            string cusCode = tokenS.Claims.First(claim => claim.Type == "sub").Value;

            var RepairScheduleDto = RepairScheduleRequest.Adapt<RepairScheduleDto>();
            if (RepairScheduleRequest.DaySchedule is not null)
            {
                string format = "dd/MM/yyyy HH:mm";
                DateTime convertTime = DateTime.ParseExact(RepairScheduleRequest.DaySchedule, format, CultureInfo.InvariantCulture);
                RepairScheduleDto.DaySchedule = convertTime;
            }

            TemplateApi result = await _repairScheduleRepository.UpdateRepairScheduleClient(RepairScheduleDto, cusCode);
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
