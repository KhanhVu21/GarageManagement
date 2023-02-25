using Microsoft.AspNetCore.Mvc;
using Mapster;
using GarageManagement.Controllers.Payload.CategoryCity;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using GarageManagement.Attribute;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.Repository;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryCityController: Controller
    {
        #region Variables
        private readonly ICategoryCityRepository _CategoryCityRepository;
        private readonly ILogger<CategoryCityController> _logger;
        #endregion

        #region Contructor
        public CategoryCityController(ICategoryCityRepository CategoryCityRepository,
        ILogger<CategoryCityController> logger)
        {
            _CategoryCityRepository = CategoryCityRepository;
            _logger = logger;
        }
        #endregion

        #region METHOD
        // HttpPut: /api/CategoryCity/HideCategoryCityByList
        [HttpPut("HideCategoryCityByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryCityByList(List<Guid> IdCategoryCity, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryCityRepository.HideCategoryCityByList(IdCategoryCity, idUserCurrent, IsHide);
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
        // HttpPut: /api/CategoryCity/HideCategoryCity
        [HttpPut("HideCategoryCity")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryCity(Guid IdCategoryCity, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryCityRepository.HideCategoryCity(IdCategoryCity, idUserCurrent, IsHide);
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
        // GET: api/CategoryCity/GetListCategoryCity
        [HttpGet("GetListCategoryCity")]
        public async Task<IActionResult> GetListCategoryCity(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategoryCityRepository.GetAllCategoryCity(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/CategoryCity/GetCategoryCityById
        [HttpGet("GetCategoryCityById")]
        public async Task<IActionResult> GetCategoryCityById(Guid IdCategoryCity)
        {
            TemplateApi templateApi = await _CategoryCityRepository.GetCategoryCityById(IdCategoryCity);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/CategoryCity/InsertCategoryCity
        [HttpPost("InsertCategoryCity")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertCategoryCity(CategoryCityRequest CategoryCityRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategoryCityDto = CategoryCityRequest.Adapt<CategoryCityDto>();

            // define some col with data concrete
            CategoryCityDto.Id = Guid.NewGuid();
            CategoryCityDto.IdUserCurrent = idUserCurrent;
            CategoryCityDto.CreateDate = DateTime.Now;
            CategoryCityDto.Status = 0;
            CategoryCityDto.IsHide = false;

            TemplateApi result = await _CategoryCityRepository.InsertCategoryCity(CategoryCityDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/CategoryCity/UpdateCategoryCity
        [HttpPut("UpdateCategoryCity")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateCategoryCity(CategoryCityRequest CategoryCityRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategoryCityDto = CategoryCityRequest.Adapt<CategoryCityDto>();
            CategoryCityDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _CategoryCityRepository.UpdateCategoryCity(CategoryCityDto);
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
        // HttpDelete: /api/CategoryCity/DeleteCategoryCity
        [HttpDelete("DeleteCategoryCity")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryCity(Guid IdCategoryCity)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryCityRepository.DeleteCategoryCity(IdCategoryCity, idUserCurrent);

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
        // HttpDelete: /api/CategoryCity/DeleteCategoryCityByList
        [HttpDelete("DeleteCategoryCityByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryCityByList(List<Guid> IdCategoryCity)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryCityRepository.DeleteCategoryCityByList(IdCategoryCity, idUserCurrent);

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
