using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.RequestList;
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
    public class RequestListController: Controller
    {
        #region Variables
        private readonly IRequestListRepository _RequestListRepository;
        private readonly ILogger<RequestListController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public RequestListController(IRequestListRepository RequestListRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<RequestListController> logger)
        {
            _RequestListRepository = RequestListRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region METHOD
        // HttpPut: /api/RequestList/CancelRequestListByList
        [HttpPut("CancelRequestListByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> CancelRequestListByList(List<Guid> IdRequestList, bool IsCanceled)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _RequestListRepository.CancelRequestListByList(IdRequestList, idUserCurrent, IsCanceled);
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
        // HttpPut: /api/RequestList/CancelRequestList
        [HttpPut("CancelRequestList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> CancelRequestList(Guid IdRequestList, bool IsCanceled)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _RequestListRepository.CancelRequestList(IdRequestList, idUserCurrent, IsCanceled);
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
        // HttpPut: /api/RequestList/CompleteRequestListByList
        [HttpPut("CompleteRequestListByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> CompleteRequestListByList(List<Guid> IdRequestList, bool IsCompleted)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _RequestListRepository.CompleteRequestListByList(IdRequestList, idUserCurrent, IsCompleted);
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
        // HttpPut: /api/RequestList/CompleteRequestList
        [HttpPut("CompleteRequestList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> CompleteRequestList(Guid IdRequestList, bool IsCompleted)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _RequestListRepository.CompleteRequestList(IdRequestList, idUserCurrent, IsCompleted);
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
        // HttpPut: /api/RequestList/ProcessingRequestListByList
        [HttpPut("ProcessingRequestListByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> ProcessingRequestListByList(List<Guid> IdRequestList, bool IsProcessing)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _RequestListRepository.ProcessRequestListByList(IdRequestList, idUserCurrent, IsProcessing);
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
        // HttpPut: /api/RequestList/ProcessingRequestList
        [HttpPut("ProcessingRequestList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> ProcessingRequestList(Guid IdRequestList, bool IsProcessing)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _RequestListRepository.ProcessRequestList(IdRequestList, idUserCurrent, IsProcessing);
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
        // GET: api/RequestList/GetListRequestList
        [HttpGet("GetListRequestList")]
        public async Task<IActionResult> GetListRequestList(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _RequestListRepository.GetAllRequestList(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/RequestList/GetRequestListById
        [HttpGet("GetRequestListById")]
        public async Task<IActionResult> GetRequestListById(Guid IdRequestList)
        {
            TemplateApi templateApi = await _RequestListRepository.GetRequestListById(IdRequestList);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/RequestList/InsertRequestList
        [HttpPost("InsertRequestList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertRequestList(RequestListModel RequestListRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var RequestListDto = RequestListRequest.Adapt<RequestListDto>();

            // define some col with data concrete
            RequestListDto.Id = Guid.NewGuid();
            RequestListDto.IdUserCurrent = idUserCurrent;
            RequestListDto.CreatedDate = DateTime.Now;
            RequestListDto.Status = 0;
            RequestListDto.IsProcessing = true;
            RequestListDto.IsCompleted = false;
            RequestListDto.IsCanceled = false;

            TemplateApi result = await _RequestListRepository.InsertRequestList(RequestListDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/RequestList/UpdateRequestList
        [HttpPut("UpdateRequestList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateRequestList(RequestListModel RequestListRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var RequestListDto = RequestListRequest.Adapt<RequestListDto>();
            RequestListDto.IdUserCurrent = idUserCurrent;
    
            TemplateApi result = await _RequestListRepository.UpdateRequestList(RequestListDto);
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
        // HttpDelete: /api/RequestList/DeleteRequestList
        [HttpDelete("DeleteRequestList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteRequestList(Guid IdRequestList)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _RequestListRepository.DeleteRequestList(IdRequestList, idUserCurrent);

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
        // HttpDelete: /api/RequestList/DeleteRequestListByList
        [HttpDelete("DeleteRequestListByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteRequestListByList(List<Guid> IdRequestList)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _RequestListRepository.DeleteRequestListByList(IdRequestList, idUserCurrent);

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
