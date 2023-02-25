using GarageManagement.Attribute;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using GarageManagement.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mapster;
using GarageManagement.Controllers.Payload.Debt;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebtController: Controller
    {
        #region Variables
        private readonly IDebtRepository _DebtRepository;
        private readonly ILogger<DebtController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public DebtController(IDebtRepository DebtRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<DebtController> logger)
        {
            _DebtRepository = DebtRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region METHOD
        // GET: api/Debt/GetListDebt
        [HttpGet("GetListDebt")]
        public async Task<IActionResult> GetListDebt(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _DebtRepository.GetAllDebt(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Debt/GetDebtById
        [HttpGet("GetDebtById")]
        public async Task<IActionResult> GetDebtById(Guid IdDebt)
        {
            TemplateApi templateApi = await _DebtRepository.GetDebtById(IdDebt);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/Debt/InsertDebt
        [HttpPost("InsertDebt")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertDebt(DebtModel DebtRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var DebtDto = DebtRequest.Adapt<DebtDto>();

            // define some col with data concrete
            DebtDto.Id = Guid.NewGuid();
            DebtDto.IdUserCurrent = idUserCurrent;
            DebtDto.CreatedDate = DateTime.Now;
            DebtDto.Status = 0;

            TemplateApi result = await _DebtRepository.InsertDebt(DebtDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/Debt/UpdateDebt
        [HttpPut("UpdateDebt")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateDebt(DebtModel DebtRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var DebtDto = DebtRequest.Adapt<DebtDto>();
            DebtDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _DebtRepository.UpdateDebt(DebtDto);
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
        // HttpDelete: /api/Debt/DeleteDebt
        [HttpDelete("DeleteDebt")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteDebt(Guid IdDebt)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _DebtRepository.DeleteDebt(IdDebt, idUserCurrent);

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
        // HttpDelete: /api/Debt/DeleteDebtByList
        [HttpDelete("DeleteDebtByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteDebtByList(List<Guid> IdDebt)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _DebtRepository.DeleteDebtByList(IdDebt, idUserCurrent);

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
