using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.AccssesaryGroup;
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
    public class AccessaryGroupController: Controller
    {
        #region Variables
        private readonly IAccessaryGroupRepository _AccessaryGroupRepository;
        private readonly ILogger<AccessaryGroupController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public AccessaryGroupController(IAccessaryGroupRepository AccessaryGroupRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<AccessaryGroupController> logger)
        {
            _AccessaryGroupRepository = AccessaryGroupRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;

        }
        #endregion

        #region METHOD
        // GET: api/AccessaryGroup/GetListAccessaryGroup
        [HttpGet("GetListAccessaryGroup")]
        public async Task<IActionResult> GetListAccessaryGroup(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _AccessaryGroupRepository.GetAllAccessaryGroup(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/AccessaryGroup/GetAccessaryGroupById
        [HttpGet("GetAccessaryGroupById")]
        public async Task<IActionResult> GetAccessaryGroupById(Guid IdAccessaryGroup)
        {
            TemplateApi templateApi = await _AccessaryGroupRepository.GetAccessaryGroupById(IdAccessaryGroup);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/AccessaryGroup/InsertAccessaryGroup
        [HttpPost("InsertAccessaryGroup")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertAccessaryGroup(AccessaryGroupRequest AccessaryGroupRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var AccessaryGroupDto = AccessaryGroupRequest.Adapt<AccessaryGroupDto>();

            // define some col with data concrete
            AccessaryGroupDto.Id = Guid.NewGuid();
            AccessaryGroupDto.IdUserCurrent = idUserCurrent;
            AccessaryGroupDto.CreatedDate = DateTime.Now;
            AccessaryGroupDto.Status = 0;
            AccessaryGroupDto.Code = "AG - " + DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

            TemplateApi result = await _AccessaryGroupRepository.InsertAccessaryGroup(AccessaryGroupDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/AccessaryGroup/UpdateAccessaryGroup
        [HttpPut("UpdateAccessaryGroup")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateAccessaryGroup(AccessaryGroupRequest AccessaryGroupRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var AccessaryGroupDto = AccessaryGroupRequest.Adapt<AccessaryGroupDto>();
            AccessaryGroupDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _AccessaryGroupRepository.UpdateAccessaryGroup(AccessaryGroupDto);
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
        // HttpDelete: /api/AccessaryGroup/DeleteAccessaryGroup
        [HttpDelete("DeleteAccessaryGroup")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteAccessaryGroup(Guid IdAccessaryGroup)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _AccessaryGroupRepository.DeleteAccessaryGroup(IdAccessaryGroup, idUserCurrent);

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
        // HttpDelete: /api/AccessaryGroup/DeleteAccessaryGroupByList
        [HttpDelete("DeleteAccessaryGroupByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteAccessaryGroupByList(List<Guid> IdAccessaryGroup)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _AccessaryGroupRepository.DeleteAccessaryGroupByList(IdAccessaryGroup, idUserCurrent);

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
