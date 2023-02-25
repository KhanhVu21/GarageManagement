using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.Customer_Vehicle;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using GarageManagement.Services.Repository;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Customer_VehicleController: Controller
    {
        #region Variables
        private readonly ICustomer_VehicleRepository _Customer_VehicleRepository;
        private readonly ILogger<Customer_VehicleController> _logger;
        #endregion

        #region Contructor
        public Customer_VehicleController(ICustomer_VehicleRepository Customer_VehicleRepository,
        ILogger<Customer_VehicleController> logger)
        {
            _Customer_VehicleRepository = Customer_VehicleRepository;
            _logger = logger;
        }
        #endregion

        #region METHOD
        // GET: api/Customer_Vehicle/GetListCustomer_Vehicle
        [HttpGet("GetListCustomer_Vehicle")]
        public async Task<IActionResult> GetListCustomer_Vehicle(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _Customer_VehicleRepository.GetAllCustomer_Vehicle(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Customer_Vehicle/GetCustomer_VehicleById
        [HttpGet("GetCustomer_VehicleById")]
        public async Task<IActionResult> GetCustomer_VehicleById(Guid IdCustomer_Vehicle)
        {
            TemplateApi templateApi = await _Customer_VehicleRepository.GetCustomer_VehicleById(IdCustomer_Vehicle);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/Customer_Vehicle/InsertCustomer_Vehicle
        [HttpPost("InsertCustomer_Vehicle")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertCustomer_Vehicle(Customer_VehicleRequest Customer_VehicleRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var Customer_VehicleDto = Customer_VehicleRequest.Adapt<Customer_VehicleDto>();

            // define some col with data concrete
            Customer_VehicleDto.Id = Guid.NewGuid();
            Customer_VehicleDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _Customer_VehicleRepository.InsertCustomer_Vehicle(Customer_VehicleDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/Customer_Vehicle/UpdateCustomer_Vehicle
        [HttpPut("UpdateCustomer_Vehicle")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateCustomer_Vehicle(Customer_VehicleRequest Customer_VehicleRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var Customer_VehicleDto = Customer_VehicleRequest.Adapt<Customer_VehicleDto>();
            Customer_VehicleDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _Customer_VehicleRepository.UpdateCustomer_Vehicle(Customer_VehicleDto);
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
        // HttpDelete: /api/Customer_Vehicle/DeleteCustomer_Vehicle
        [HttpDelete("DeleteCustomer_Vehicle")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCustomer_Vehicle(Guid IdCustomer_Vehicle)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _Customer_VehicleRepository.DeleteCustomer_Vehicle(IdCustomer_Vehicle, idUserCurrent);

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
        // HttpDelete: /api/Customer_Vehicle/DeleteCustomer_VehicleByList
        [HttpDelete("DeleteCustomer_VehicleByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCustomer_VehicleByList(List<Guid> IdCustomer_Vehicle)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _Customer_VehicleRepository.DeleteCustomer_VehicleByList(IdCustomer_Vehicle, idUserCurrent);

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
