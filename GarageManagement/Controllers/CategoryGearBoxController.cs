using GarageManagement.Attribute;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Microsoft.AspNetCore.Mvc;
using Mapster;
using GarageManagement.Controllers.Payload.CategoryGearBox;
using GarageManagement.Services.Repository;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryGearBoxController: Controller
    {
        #region Variables
        private readonly ICategoryGearBoxRepository _CategoryGearBoxRepository;
        private readonly ILogger<CategoryGearBoxController> _logger;
        #endregion

        #region Contructor
        public CategoryGearBoxController(ICategoryGearBoxRepository CategoryGearBoxRepository,
        ILogger<CategoryGearBoxController> logger)
        {
            _CategoryGearBoxRepository = CategoryGearBoxRepository;
            _logger = logger;
        }
        #endregion

        #region METHOD
        // GET: api/CategoryGearBox/GetListCategoryGearBoxAvailable
        [HttpGet("GetListCategoryGearBoxAvailable")]
        public async Task<IActionResult> GetListCategoryGearBoxAvailable(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategoryGearBoxRepository.GetAllCategoryAvailable(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPut: /api/CategoryGearBox/HideCategoryGearBoxByList
        [HttpPut("HideCategoryGearBoxByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryGearBoxByList(List<Guid> IdCategoryGearBox, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryGearBoxRepository.HideCategoryGearBoxByList(IdCategoryGearBox, idUserCurrent, IsHide);
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
        // HttpPut: /api/CategoryGearBox/HideCategoryGearBox
        [HttpPut("HideCategoryGearBox")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryGearBox(Guid IdCategoryGearBox, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryGearBoxRepository.HideCategoryGearBox(IdCategoryGearBox, idUserCurrent, IsHide);
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
        // GET: api/CategoryGearBox/GetListCategoryGearBox
        [HttpGet("GetListCategoryGearBox")]
        public async Task<IActionResult> GetListCategoryGearBox(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategoryGearBoxRepository.GetAllCategoryGearBox(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/CategoryGearBox/GetCategoryGearBoxById
        [HttpGet("GetCategoryGearBoxById")]
        public async Task<IActionResult> GetCategoryGearBoxById(Guid IdCategoryGearBox)
        {
            TemplateApi templateApi = await _CategoryGearBoxRepository.GetCategoryGearBoxById(IdCategoryGearBox);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/CategoryGearBox/InsertCategoryGearBox
        [HttpPost("InsertCategoryGearBox")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertCategoryGearBox(CategoryGearBoxRequest CategoryGearBoxRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategoryGearBoxDto = CategoryGearBoxRequest.Adapt<CategoryGearBoxDto>();

            // define some col with data concrete
            CategoryGearBoxDto.Id = Guid.NewGuid();
            CategoryGearBoxDto.IdUserCurrent = idUserCurrent;
            CategoryGearBoxDto.CreatedDate = DateTime.Now;
            CategoryGearBoxDto.Status = 0;
            CategoryGearBoxDto.IsHide = false;

            TemplateApi result = await _CategoryGearBoxRepository.InsertCategoryGearBox(CategoryGearBoxDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/CategoryGearBox/UpdateCategoryGearBox
        [HttpPut("UpdateCategoryGearBox")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateCategoryGearBox(CategoryGearBoxRequest CategoryGearBoxRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategoryGearBoxDto = CategoryGearBoxRequest.Adapt<CategoryGearBoxDto>();
            CategoryGearBoxDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _CategoryGearBoxRepository.UpdateCategoryGearBox(CategoryGearBoxDto);
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
        // HttpDelete: /api/CategoryGearBox/DeleteCategoryGearBox
        [HttpDelete("DeleteCategoryGearBox")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryGearBox(Guid IdCategoryGearBox)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryGearBoxRepository.DeleteCategoryGearBox(IdCategoryGearBox, idUserCurrent);

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
        // HttpDelete: /api/CategoryGearBox/DeleteCategoryGearBoxByList
        [HttpDelete("DeleteCategoryGearBoxByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryGearBoxByList(List<Guid> IdCategoryGearBox)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryGearBoxRepository.DeleteCategoryGearBoxByList(IdCategoryGearBox, idUserCurrent);

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
