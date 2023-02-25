using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.Allowance;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using GarageManagement.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mapster;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AllowanceController: Controller
    {
        #region Variables
        private readonly IAllowanceRepository _AllowanceRepository;
        private readonly ILogger<AllowanceController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public AllowanceController(IAllowanceRepository AllowanceRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<AllowanceController> logger)
        {
            _AllowanceRepository = AllowanceRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region METHOD
        // HttpDelete: /api/Allowance/DeleteAllowanceByList
        [HttpDelete("DeleteAllowanceByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteAllowanceByList(List<Guid> IdAllowances)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _AllowanceRepository.DeleteAllowanceByList(IdAllowances, idUserCurrent);

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
        // GET: api/Allowance/GetListAllowance
        [HttpGet("GetListAllowance")]
        public async Task<IActionResult> GetListAllowance(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _AllowanceRepository.GetAllAllowance(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Allowance/GetAllAllowanceByIdEmployee
        [HttpGet("GetAllAllowanceByIdEmployee")]
        public async Task<IActionResult> GetAllAllowanceByIdEmployee(int pageNumber, int pageSize, Guid IdEmployee)
        {
            TemplateApi templateApi = await _AllowanceRepository.GetAllAllowanceByIdEmployee(pageNumber, pageSize, IdEmployee);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Allowance/GetAllowanceById
        [HttpGet("GetAllowanceById")]
        public async Task<IActionResult> GetAllowanceById(Guid IdAllowance)
        {
            TemplateApi templateApi = await _AllowanceRepository.GetAllowanceById(IdAllowance);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/Allowance/InsertAllowance
        [HttpPost("InsertAllowance")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertAllowance(AllowanceRequest AllowanceRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var AllowanceDto = AllowanceRequest.Adapt<AllowanceDto>();

            // define some col with data concrete
            AllowanceDto.Id = Guid.NewGuid();
            AllowanceDto.IdUserCurrent = idUserCurrent;
            AllowanceDto.CreatedDate = DateTime.Now;
            AllowanceDto.Status = 0;

            TemplateApi result = await _AllowanceRepository.InsertAllowance(AllowanceDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/Allowance/UpdateAllowance
        [HttpPut("UpdateAllowance")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateAllowance(AllowanceRequest AllowanceRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var AllowanceDto = AllowanceRequest.Adapt<AllowanceDto>();
            AllowanceDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _AllowanceRepository.UpdateAllowance(AllowanceDto);
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
        // HttpPut: api/Allowance/InsertAllowanceForEmployee
        [HttpPut("InsertAllowanceForEmployee")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertAllowanceForEmployee(Guid IdEmployee, List<Guid> IdAllowances)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _AllowanceRepository.InsertAllowanceForEmployee(IdEmployee, IdAllowances, idUserCurrent);
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
