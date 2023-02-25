using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.RO_RepairOders;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RO_RepairOdersController: Controller
    {
        #region Variables
        private readonly IRO_RepairOdersRepository _RO_RepairOdersRepository;
        private readonly ILogger<RO_RepairOdersController> _logger;
        #endregion

        #region Contructor
        public RO_RepairOdersController(IRO_RepairOdersRepository RO_RepairOdersRepository,
        ILogger<RO_RepairOdersController> logger)
        {
            _RO_RepairOdersRepository = RO_RepairOdersRepository;
            _logger = logger;
        }
        #endregion

        #region METHOD
        // GET: api/RO_RepairOders/GetListRO_RepairOdersByIdCustomer
        [HttpGet("GetListRO_RepairOdersByIdCustomer")]
        public async Task<IActionResult> GetListRO_RepairOdersByIdCustomer(int pageNumber, int pageSize, Guid IdCustomer)
        {
            TemplateApi templateApi = await _RO_RepairOdersRepository.GetAllRO_RepairOdersByIdCustomer(pageNumber, pageSize, IdCustomer);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/RO_RepairOders/InsertRepairOders
        [HttpPost("InsertRepairOders")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertRepairOders(RepairOrderPayload repairOrderPayload)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _RO_RepairOdersRepository.InsertRO_RepairOders(repairOrderPayload, idUserCurrent);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // GET: api/RO_RepairOders/GetListRO_RepairOders
        [HttpGet("GetListRO_RepairOders")]
        public async Task<IActionResult> GetListRO_RepairOders(int pageNumber, int pageSize, bool IsPaid)
        {
            TemplateApi templateApi = await _RO_RepairOdersRepository.GetAllRO_RepairOders(pageNumber, pageSize, IsPaid);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/RO_RepairOders/GetRO_RepairOdersById
        [HttpGet("GetRO_RepairOdersById")]
        public async Task<IActionResult> GetRO_RepairOdersById(Guid IdRO_RepairOders)
        {
            TemplateApi templateApi = await _RO_RepairOdersRepository.GetRO_RepairOdersById(IdRO_RepairOders);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPut: api/RO_RepairOders/UpdateRO_RepairOders
        [HttpPut("UpdateRO_RepairOders")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateRO_RepairOders(RepairOrderPayload repairOrderPayload)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _RO_RepairOdersRepository.UpdateRO_RepairOders(repairOrderPayload, idUserCurrent);
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

            _logger.LogError("Xảy ra lỗi : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/RO_RepairOders/PayRepairOders
        [HttpPut("PayRepairOders")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> PayRepairOders(float totalMoneys, Guid IdRepairOrder, bool continueDeposit)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _RO_RepairOdersRepository.PayRepairOders(totalMoneys, IdRepairOrder, idUserCurrent, continueDeposit);
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

            _logger.LogError("Xảy ra lỗi : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        #endregion
    }
}
