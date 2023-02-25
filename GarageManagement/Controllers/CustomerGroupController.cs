using GarageManagement.Attribute;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using Microsoft.AspNetCore.Mvc;
using Mapster;
using GarageManagement.Controllers.Payload.CustomerGroup;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.Repository;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerGroupController:Controller
    {
        #region Variables
        private readonly ICustomerGroupRepository _CustomerGroupRepository;
        private readonly ILogger<CustomerGroupController> _logger;
        #endregion

        #region Contructor
        public CustomerGroupController(ICustomerGroupRepository CustomerGroupRepository,
        ILogger<CustomerGroupController> logger)
        {
            _CustomerGroupRepository = CustomerGroupRepository;
            _logger = logger;
        }
        #endregion

        #region METHOD
        // GET: api/CustomerGroup/GetListCustomerGroupAndCustomers
        [HttpGet("GetListCustomerGroupAndCustomers")]
        public async Task<IActionResult> GetListCustomerGroupAndCustomers(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CustomerGroupRepository.GetAllCustomerGroupAndCustomer(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/CustomerGroup/GetListCustomerGroupAvailable
        [HttpGet("GetListCustomerGroupAvailable")]
        public async Task<IActionResult> GetListCustomerGroupAvailable(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CustomerGroupRepository.GetAllCustomerGroupAvailable(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPut: /api/CustomerGroup/HideCustomerGroupByList
        [HttpPut("HideCustomerGroupByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCustomerGroupByList(List<Guid> IdCustomerGroup, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CustomerGroupRepository.HideCustomerGroupByList(IdCustomerGroup, idUserCurrent, IsHide);
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
        // HttpPut: /api/CustomerGroup/HideCustomerGroup
        [HttpPut("HideCustomerGroup")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCustomerGroup(Guid IdCustomerGroup, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CustomerGroupRepository.HideCustomerGroup(IdCustomerGroup, idUserCurrent, IsHide);
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
        // GET: api/CustomerGroup/GetListCustomerGroup
        [HttpGet("GetListCustomerGroup")]
        public async Task<IActionResult> GetListCustomerGroup(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CustomerGroupRepository.GetAllCustomerGroup(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/CustomerGroup/GetCustomerGroupById
        [HttpGet("GetCustomerGroupById")]
        public async Task<IActionResult> GetCustomerGroupById(Guid IdCustomerGroup)
        {
            TemplateApi templateApi = await _CustomerGroupRepository.GetCustomerGroupById(IdCustomerGroup);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/CustomerGroup/InsertCustomerGroup
        [HttpPost("InsertCustomerGroup")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertCustomerGroup(CustomerGroupRequest CustomerGroupRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CustomerGroupDto = CustomerGroupRequest.Adapt<CustomerGroupDto>();

            // define some col with data concrete
            CustomerGroupDto.Id = Guid.NewGuid();
            CustomerGroupDto.IdUserCurrent = idUserCurrent;
            CustomerGroupDto.CreateDate = DateTime.Now;
            CustomerGroupDto.Status = 0;
            CustomerGroupDto.IsHide = false;
            CustomerGroupDto.GroupCode = "GR - " + DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();


            TemplateApi result = await _CustomerGroupRepository.InsertCustomerGroup(CustomerGroupDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/CustomerGroup/UpdateCustomerGroup
        [HttpPut("UpdateCustomerGroup")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateCustomerGroup(CustomerGroupRequest CustomerGroupRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CustomerGroupDto = CustomerGroupRequest.Adapt<CustomerGroupDto>();
            CustomerGroupDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _CustomerGroupRepository.UpdateCustomerGroup(CustomerGroupDto);
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
        // HttpDelete: /api/CustomerGroup/DeleteCustomerGroup
        [HttpDelete("DeleteCustomerGroup")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCustomerGroup(Guid IdCustomerGroup)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CustomerGroupRepository.DeleteCustomerGroup(IdCustomerGroup, idUserCurrent);

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
        // HttpDelete: /api/CustomerGroup/DeleteCustomerGroupByList
        [HttpDelete("DeleteCustomerGroupByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCustomerGroupByList(List<Guid> IdCustomerGroup)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CustomerGroupRepository.DeleteCustomerGroupByList(IdCustomerGroup, idUserCurrent);

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
