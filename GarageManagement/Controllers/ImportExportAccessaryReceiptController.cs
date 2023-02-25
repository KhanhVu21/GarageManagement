using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.ImportExportAccessaryReceipt;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using GarageManagement.Services.Repository;
using GarageManagement.Utility;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportExportAccessaryReceiptController : Controller
    {
        #region Variables
        private readonly IImportExportAccessaryReceiptRepository _ImportExportAccessaryReceiptRepository;
        private readonly ILogger<ImportExportAccessaryReceiptController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public ImportExportAccessaryReceiptController(IImportExportAccessaryReceiptRepository ImportExportAccessaryReceiptRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<ImportExportAccessaryReceiptController> logger)
        {
            _ImportExportAccessaryReceiptRepository = ImportExportAccessaryReceiptRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region METHOD
        // GET: api/ImportExportAccessaryReceipt/GetListImportExportAccessaryReceiptByIdEmployee
        [HttpGet("GetListImportExportAccessaryReceiptByIdEmployee")]
        public async Task<IActionResult> GetListImportExportAccessaryReceiptByIdEmployee(int pageNumber, int pageSize, Guid IdEmployee)
        {
            TemplateApi templateApi = await _ImportExportAccessaryReceiptRepository.GetAllImportExportAccessaryReceiptByIdEmployee(pageNumber, pageSize, IdEmployee);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/ImportExportAccessaryReceipt/GetListImportExportAccessaryReceiptByIdAccessary
        [HttpGet("GetListImportExportAccessaryReceiptByIdAccessary")]
        public async Task<IActionResult> GetListImportExportAccessaryReceiptByIdAccessary(int pageNumber, int pageSize, Guid IdAccessary)
        {
            TemplateApi templateApi = await _ImportExportAccessaryReceiptRepository.GetAllImportExportAccessaryReceiptByIdAccessary(pageNumber, pageSize, IdAccessary);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/ImportExportAccessaryReceipt/GetListImportExportAccessaryReceipt
        [HttpGet("GetListImportExportAccessaryReceipt")]
        public async Task<IActionResult> GetListImportExportAccessaryReceipt(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _ImportExportAccessaryReceiptRepository.GetAllImportExportAccessaryReceipt(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/ImportExportAccessaryReceipt/GetImportExportAccessaryReceiptById
        [HttpGet("GetImportExportAccessaryReceiptById")]
        public async Task<IActionResult> GetImportExportAccessaryReceiptById(Guid IdImportExportAccessaryReceipt)
        {
            TemplateApi templateApi = await _ImportExportAccessaryReceiptRepository.GetImportExportAccessaryReceiptById(IdImportExportAccessaryReceipt);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/ImportExportAccessaryReceipt/InsertImportExportAccessaryReceipt
        [HttpPost("InsertImportExportAccessaryReceipt")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertImportExportAccessaryReceipt(ImportExportAccessaryReceiptModel ImportExportAccessaryReceiptRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var ImportExportAccessaryReceiptDto = ImportExportAccessaryReceiptRequest.Adapt<ImportExportAccessaryReceiptDto>();

            // define some col with data concrete
            ImportExportAccessaryReceiptDto.Id = Guid.NewGuid();
            ImportExportAccessaryReceiptDto.IdUserCurrent = idUserCurrent;
            ImportExportAccessaryReceiptDto.CreatedDate = DateTime.Now;
            ImportExportAccessaryReceiptDto.Status = 0;
            ImportExportAccessaryReceiptDto.Code = "IE - " + DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

            TemplateApi result = await _ImportExportAccessaryReceiptRepository.InsertImportExportAccessaryReceipt(ImportExportAccessaryReceiptDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/ImportExportAccessaryReceipt/UpdateImportExportAccessaryReceipt
        [HttpPut("UpdateImportExportAccessaryReceipt")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateImportExportAccessaryReceipt(ImportExportAccessaryReceiptModel ImportExportAccessaryReceiptRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var ImportExportAccessaryReceiptDto = ImportExportAccessaryReceiptRequest.Adapt<ImportExportAccessaryReceiptDto>();
            ImportExportAccessaryReceiptDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _ImportExportAccessaryReceiptRepository.UpdateImportExportAccessaryReceipt(ImportExportAccessaryReceiptDto);
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
        // HttpDelete: /api/ImportExportAccessaryReceipt/DeleteImportExportAccessaryReceipt
        [HttpDelete("DeleteImportExportAccessaryReceipt")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteImportExportAccessaryReceipt(Guid IdImportExportAccessaryReceipt)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _ImportExportAccessaryReceiptRepository.DeleteImportExportAccessaryReceipt(IdImportExportAccessaryReceipt, idUserCurrent);

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
        // HttpDelete: /api/ImportExportAccessaryReceipt/DeleteImportExportAccessaryReceiptByList
        [HttpDelete("DeleteImportExportAccessaryReceiptByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteImportExportAccessaryReceiptByList(List<Guid> IdImportExportAccessaryReceipt)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _ImportExportAccessaryReceiptRepository.DeleteImportExportAccessaryReceiptByList(IdImportExportAccessaryReceipt, idUserCurrent);

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
