using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.EngineerGroup;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeGroupController: Controller
    {
        #region Variables
        private readonly IEmployeeGroupRepository _EmployeeGroupRepository;
        private readonly ILogger<EmployeeGroupController> _logger;
        #endregion

        #region Contructor
        public EmployeeGroupController(IEmployeeGroupRepository EmployeeGroupRepository,
        ILogger<EmployeeGroupController> logger)
        {
            _EmployeeGroupRepository = EmployeeGroupRepository;
            _logger = logger;
        }
        #endregion

        #region METHOD
        // GET: api/EmployeeGroup/GetAllEmployeeGroupAndEmployees
        [HttpGet("GetAllEmployeeGroupAndEmployees")]
        public async Task<IActionResult> GetAllEmployeeGroupAndEmployees(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _EmployeeGroupRepository.GetAllEmployeeGroupAndEmployees(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/EmployeeGroup/GetListEmployeeGroupAvailable
        [HttpGet("GetListEmployeeGroupAvailable")]
        public async Task<IActionResult> GetListEmployeeGroupAvailable(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _EmployeeGroupRepository.GetAllEmployeeGroupAvailable(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPut: /api/EmployeeGroup/HideEmployeeGroupByList
        [HttpPut("HideEmployeeGroupByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideEmployeeGroupByList(List<Guid> IdEmployeeGroup, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _EmployeeGroupRepository.HideEmployeeGroupByList(IdEmployeeGroup, idUserCurrent, IsHide);
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
        // HttpPut: /api/EmployeeGroup/HideEmployeeGroup
        [HttpPut("HideEmployeeGroup")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideEmployeeGroup(Guid IdEmployeeGroup, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _EmployeeGroupRepository.HideEmployeeGroup(IdEmployeeGroup, idUserCurrent, IsHide);
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
        // GET: api/EmployeeGroup/GetListEmployeeGroup
        [HttpGet("GetListEmployeeGroup")]
        public async Task<IActionResult> GetListEmployeeGroup(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _EmployeeGroupRepository.GetAllEmployeeGroup(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/EmployeeGroup/GetEmployeeGroupById
        [HttpGet("GetEmployeeGroupById")]
        public async Task<IActionResult> GetEmployeeGroupById(Guid IdEmployeeGroup)
        {
            TemplateApi templateApi = await _EmployeeGroupRepository.GetEmployeeGroupById(IdEmployeeGroup);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/EmployeeGroup/InsertEmployeeGroup
        [HttpPost("InsertEmployeeGroup")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertEmployeeGroup(EmployeeGroupRequest EmployeeGroupRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var EmployeeGroupDto = EmployeeGroupRequest.Adapt<EmployeeGroupDto>();

            // define some col with data concrete
            EmployeeGroupDto.Id = Guid.NewGuid();
            EmployeeGroupDto.IdUserCurrent = idUserCurrent;
            EmployeeGroupDto.CreateDate = DateTime.Now;
            EmployeeGroupDto.Status = 0;
            EmployeeGroupDto.IsHide = false;

            TemplateApi result = await _EmployeeGroupRepository.InsertEmployeeGroup(EmployeeGroupDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/EmployeeGroup/UpdateEmployeeGroup
        [HttpPut("UpdateEmployeeGroup")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateEmployeeGroup(EmployeeGroupRequest EmployeeGroupRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var EmployeeGroupDto = EmployeeGroupRequest.Adapt<EmployeeGroupDto>();
            EmployeeGroupDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _EmployeeGroupRepository.UpdateEmployeeGroup(EmployeeGroupDto);
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
        // HttpDelete: /api/EmployeeGroup/DeleteEmployeeGroup
        [HttpDelete("DeleteEmployeeGroup")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteEmployeeGroup(Guid IdEmployeeGroup)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _EmployeeGroupRepository.DeleteEmployeeGroup(IdEmployeeGroup, idUserCurrent);

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
        // HttpDelete: /api/EmployeeGroup/DeleteEmployeeGroupByList
        [HttpDelete("DeleteEmployeeGroupByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteEmployeeGroupByList(List<Guid> IdEmployeeGroup)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _EmployeeGroupRepository.DeleteEmployeeGroupByList(IdEmployeeGroup, idUserCurrent);

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
