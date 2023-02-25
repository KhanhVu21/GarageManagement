using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.OrtherCost;
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
    public class OrtherCostController: Controller
    {
        #region Variables
        private readonly IOrtherCostRepository _OrtherCostRepository;
        private readonly ILogger<OrtherCostController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public OrtherCostController(IOrtherCostRepository OrtherCostRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<OrtherCostController> logger)
        {
            _OrtherCostRepository = OrtherCostRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region METHOD
        // GET: api/OrtherCost/GetAllOrtherCostByIdRepairOders
        [HttpGet("GetAllOrtherCostByIdRepairOders")]
        public async Task<IActionResult> GetAllOrtherCostByIdRepairOders(int pageNumber, int pageSize, Guid IdRepairOders)
        {
            TemplateApi templateApi = await _OrtherCostRepository.GetAllOrtherCostByIdRepairOders(pageNumber, pageSize, IdRepairOders);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/OrtherCost/GetListOrtherCost
        [HttpGet("GetListOrtherCost")]
        public async Task<IActionResult> GetListOrtherCost(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _OrtherCostRepository.GetAllOrtherCost(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/OrtherCost/GetOrtherCostById
        [HttpGet("GetOrtherCostById")]
        public async Task<IActionResult> GetOrtherCostById(Guid IdOrtherCost)
        {
            TemplateApi templateApi = await _OrtherCostRepository.GetOrtherCostById(IdOrtherCost);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/OrtherCost/InsertOrtherCost
        [HttpPost("InsertOrtherCost")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertOrtherCost(OrtherCostModel OrtherCostRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var OrtherCostDto = OrtherCostRequest.Adapt<OrtherCostDto>();

            // define some col with data concrete
            OrtherCostDto.Id = Guid.NewGuid();
            OrtherCostDto.IdUserCurrent = idUserCurrent;
            OrtherCostDto.CreatedDate = DateTime.Now;
            OrtherCostDto.Status = 0;

            TemplateApi result = await _OrtherCostRepository.InsertOrtherCost(OrtherCostDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/OrtherCost/UpdateOrtherCost
        [HttpPut("UpdateOrtherCost")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateOrtherCost(OrtherCostModel OrtherCostRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var OrtherCostDto = OrtherCostRequest.Adapt<OrtherCostDto>();
            OrtherCostDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _OrtherCostRepository.UpdateOrtherCost(OrtherCostDto);
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
        // HttpDelete: /api/OrtherCost/DeleteOrtherCost
        [HttpDelete("DeleteOrtherCost")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteOrtherCost(Guid IdOrtherCost)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _OrtherCostRepository.DeleteOrtherCost(IdOrtherCost, idUserCurrent);

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
        // HttpDelete: /api/OrtherCost/DeleteOrtherCostByList
        [HttpDelete("DeleteOrtherCostByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteOrtherCostByList(List<Guid> IdOrtherCost)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _OrtherCostRepository.DeleteOrtherCostByList(IdOrtherCost, idUserCurrent);

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
