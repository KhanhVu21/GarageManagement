using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.Accessary;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using GarageManagement.Services.Repository;
using GarageManagement.Utility;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;
using System.Globalization;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessaryController: Controller
    {
        #region Variables
        private readonly IAccessaryRepository _AccessaryRepository;
        private readonly ILogger<AccessaryController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public AccessaryController(IAccessaryRepository AccessaryRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<AccessaryController> logger)
        {
            _AccessaryRepository = AccessaryRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region METHOD
        // GET: api/Accessary/InventoryAlert
        [HttpGet("InventoryAlert")]
        public async Task<IActionResult> InventoryAlert()
        {
            var values = await _AccessaryRepository.InventoryAlert();
            return Ok(new
            {
                Success = true,
                Fail = false,
                Message = values
        });
        }
        // HttpDelete: /api/Accessary/DeleteAccessaryByList
        [HttpDelete("DeleteAccessaryByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteAccessaryByList(List<Guid> IdAccessarys)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _AccessaryRepository.DeleteAccessaryByList(IdAccessarys, idUserCurrent);

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
        // GET: api/Accessary/GetListAccessaryByGroupID
        [HttpGet("GetListAccessaryByGroupID")]
        public async Task<IActionResult> GetListAccessaryByGroupID(int pageNumber, int pageSize, Guid GroupID)
        {
            TemplateApi templateApi = await _AccessaryRepository.GetAllAccessaryByGroupID(pageNumber, pageSize, GroupID);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Accessary/GetListAccessary
        [HttpGet("GetListAccessary")]
        public async Task<IActionResult> GetListAccessary(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _AccessaryRepository.GetAllAccessary(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Accessary/GetAccessaryById
        [HttpGet("GetAccessaryById")]
        public async Task<IActionResult> GetAccessaryById(Guid IdAccessary)
        {
            TemplateApi templateApi = await _AccessaryRepository.GetAccessaryById(IdAccessary);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/Accessary/InsertAccessary
        [HttpPost("InsertAccessary")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertAccessary(AccessaryModel AccessaryRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var AccessaryDto = AccessaryRequest.Adapt<AccessaryDto>();

            // define some col with data concrete
            AccessaryDto.Id = Guid.NewGuid();
            AccessaryDto.IdUserCurrent = idUserCurrent;
            AccessaryDto.CreatedDate = DateTime.Now;
            AccessaryDto.Status = 0;
            AccessaryDto.Code = "AC - " + DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

            TemplateApi result = await _AccessaryRepository.InsertAccessary(AccessaryDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/Accessary/UpdateAccessary
        [HttpPut("UpdateAccessary")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateAccessary(AccessaryModel AccessaryRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var AccessaryDto = AccessaryRequest.Adapt<AccessaryDto>();
            AccessaryDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _AccessaryRepository.UpdateAccessary(AccessaryDto);
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
