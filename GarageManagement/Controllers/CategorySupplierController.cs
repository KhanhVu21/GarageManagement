using GarageManagement.Attribute;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using GarageManagement.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mapster;
using GarageManagement.Controllers.Payload.CategorySupplier;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategorySupplierController: Controller
    {
        #region Variables
        private readonly ICategorySupplierRepository _CategorySupplierRepository;
        private readonly ILogger<CategorySupplierController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public CategorySupplierController(ICategorySupplierRepository CategorySupplierRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<CategorySupplierController> logger)
        {
            _CategorySupplierRepository = CategorySupplierRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region METHOD
        // GET: api/CategorySupplier/GetListCategorySupplierAvailable
        [HttpGet("GetListCategorySupplierAvailable")]
        public async Task<IActionResult> GetListCategorySupplierAvailable(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategorySupplierRepository.GetAllCategorySupplierAvailable(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPut: /api/CategorySupplier/HideCategorySupplierByList
        [HttpPut("HideCategorySupplierByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategorySupplierByList(List<Guid> IdCategorySupplier, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategorySupplierRepository.HideCategorySupplierByList(IdCategorySupplier, idUserCurrent, IsHide);
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
        // HttpPut: /api/CategorySupplier/HideCategorySupplier
        [HttpPut("HideCategorySupplier")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategorySupplier(Guid IdCategorySupplier, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategorySupplierRepository.HideCategorySupplier(IdCategorySupplier, idUserCurrent, IsHide);
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
        // GET: api/CategorySupplier/GetListCategorySupplier
        [HttpGet("GetListCategorySupplier")]
        public async Task<IActionResult> GetListCategorySupplier(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategorySupplierRepository.GetAllCategorySupplier(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/CategorySupplier/GetCategorySupplierById
        [HttpGet("GetCategorySupplierById")]
        public async Task<IActionResult> GetCategorySupplierById(Guid IdCategorySupplier)
        {
            TemplateApi templateApi = await _CategorySupplierRepository.GetCategorySupplierById(IdCategorySupplier);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/CategorySupplier/InsertCategorySupplier
        [HttpPost("InsertCategorySupplier")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertCategorySupplier(CategorySupplierModel CategorySupplierRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategorySupplierDto = CategorySupplierRequest.Adapt<CategorySupplierDto>();

            // define some col with data concrete
            CategorySupplierDto.Id = Guid.NewGuid();
            CategorySupplierDto.IdUserCurrent = idUserCurrent;
            CategorySupplierDto.CreatedDate = DateTime.Now;
            CategorySupplierDto.Status = 0;
            CategorySupplierDto.IsHide = false;

            TemplateApi result = await _CategorySupplierRepository.InsertCategorySupplier(CategorySupplierDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/CategorySupplier/UpdateCategorySupplier
        [HttpPut("UpdateCategorySupplier")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateCategorySupplier(CategorySupplierModel CategorySupplierRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategorySupplierDto = CategorySupplierRequest.Adapt<CategorySupplierDto>();
            CategorySupplierDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _CategorySupplierRepository.UpdateCategorySupplier(CategorySupplierDto);
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
        // HttpDelete: /api/CategorySupplier/DeleteCategorySupplier
        [HttpDelete("DeleteCategorySupplier")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategorySupplier(Guid IdCategorySupplier)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategorySupplierRepository.DeleteCategorySupplier(IdCategorySupplier, idUserCurrent);

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
        // HttpDelete: /api/CategorySupplier/DeleteCategorySupplierByList
        [HttpDelete("DeleteCategorySupplierByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategorySupplierByList(List<Guid> IdCategorySupplier)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategorySupplierRepository.DeleteCategorySupplierByList(IdCategorySupplier, idUserCurrent);

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
