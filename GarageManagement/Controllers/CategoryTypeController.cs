using GarageManagement.Attribute;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Microsoft.AspNetCore.Mvc;
using Mapster;
using GarageManagement.Controllers.Payload.CategoryType;
using GarageManagement.Services.Repository;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryTypeController: Controller
    {
        #region Variables
        private readonly ICategoryTypeRepository _CategoryTypeRepository;
        private readonly ILogger<CategoryTypeController> _logger;
        #endregion

        #region Contructor
        public CategoryTypeController(ICategoryTypeRepository CategoryTypeRepository,
        ILogger<CategoryTypeController> logger)
        {
            _CategoryTypeRepository = CategoryTypeRepository;
            _logger = logger;
        }
        #endregion

        #region METHOD
        // GET: api/CategoryType/GetListCategoryTypeAvailable
        [HttpGet("GetListCategoryTypeAvailable")]
        public async Task<IActionResult> GetListCategoryTypeAvailable(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategoryTypeRepository.GetAllCategoryTypeAvailable(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPut: /api/CategoryType/HideCategoryTypeByList
        [HttpPut("HideCategoryTypeByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryTypeByList(List<Guid> IdCategoryType, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryTypeRepository.HideCategoryTypeByList(IdCategoryType, idUserCurrent, IsHide);
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
        // HttpPut: /api/CategoryType/HideCategoryType
        [HttpPut("HideCategoryType")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryType(Guid IdCategoryType, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryTypeRepository.HideCategoryType(IdCategoryType, idUserCurrent, IsHide);
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
        // GET: api/CategoryType/GetListCategoryType
        [HttpGet("GetListCategoryType")]
        public async Task<IActionResult> GetListCategoryType(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategoryTypeRepository.GetAllCategoryType(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/CategoryType/GetCategoryTypeById
        [HttpGet("GetCategoryTypeById")]
        public async Task<IActionResult> GetCategoryTypeById(Guid IdCategoryType)
        {
            TemplateApi templateApi = await _CategoryTypeRepository.GetCategoryTypeById(IdCategoryType);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/CategoryType/InsertCategoryType
        [HttpPost("InsertCategoryType")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertCategoryType(CategoryTypeRequest CategoryTypeRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategoryTypeDto = CategoryTypeRequest.Adapt<CategoryTypeDto>();

            // define some col with data concrete
            CategoryTypeDto.Id = Guid.NewGuid();
            CategoryTypeDto.IdUserCurrent = idUserCurrent;
            CategoryTypeDto.CreatedDate = DateTime.Now;
            CategoryTypeDto.Status = 0;
            CategoryTypeDto.IsHide = false;

            TemplateApi result = await _CategoryTypeRepository.InsertCategoryType(CategoryTypeDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/CategoryType/UpdateCategoryType
        [HttpPut("UpdateCategoryType")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateCategoryType(CategoryTypeRequest CategoryTypeRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategoryTypeDto = CategoryTypeRequest.Adapt<CategoryTypeDto>();
            CategoryTypeDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _CategoryTypeRepository.UpdateCategoryType(CategoryTypeDto);
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
        // HttpDelete: /api/CategoryType/DeleteCategoryType
        [HttpDelete("DeleteCategoryType")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryType(Guid IdCategoryType)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryTypeRepository.DeleteCategoryType(IdCategoryType, idUserCurrent);

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
        // HttpDelete: /api/CategoryType/DeleteCategoryTypeByList
        [HttpDelete("DeleteCategoryTypeByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryTypeByList(List<Guid> IdCategoryType)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryTypeRepository.DeleteCategoryTypeByList(IdCategoryType, idUserCurrent);

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
