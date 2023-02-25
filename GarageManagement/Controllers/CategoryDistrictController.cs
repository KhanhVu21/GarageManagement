using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.CategoryDistrict;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryDistrictController: Controller
    {
        #region Variables
        private readonly ICategoryDistrictRepository _CategoryDistrictRepository;
        private readonly ILogger<CategoryDistrictController> _logger;
        #endregion

        #region Contructor
        public CategoryDistrictController(ICategoryDistrictRepository CategoryDistrictRepository,
        ILogger<CategoryDistrictController> logger)
        {
            _CategoryDistrictRepository = CategoryDistrictRepository;
            _logger = logger;
        }
        #endregion

        #region METHOD
        // GET: api/CategoryDistrict/GetListCategoryDistrictByCityNumber
        [HttpGet("GetListCategoryDistrictByCityNumber")]
        public async Task<IActionResult> GetListCategoryDistrictByCityNumber(int pageNumber, int pageSize, string CityCode)
        {
            TemplateApi templateApi = await _CategoryDistrictRepository.GetAllCategoryDistrictByIdCity(pageNumber, pageSize, CityCode);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPut: /api/CategoryDistrict/HideCategoryDistrictByList
        [HttpPut("HideCategoryDistrictByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryDistrictByList(List<Guid> IdCategoryDistrict, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryDistrictRepository.HideCategoryDistrictByList(IdCategoryDistrict, idUserCurrent, IsHide);
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
        // HttpPut: /api/CategoryDistrict/HideCategoryDistrict
        [HttpPut("HideCategoryDistrict")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryDistrict(Guid IdCategoryDistrict, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryDistrictRepository.HideCategoryDistrict(IdCategoryDistrict, idUserCurrent, IsHide);
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
        // GET: api/CategoryDistrict/GetListCategoryDistrict
        [HttpGet("GetListCategoryDistrict")]
        public async Task<IActionResult> GetListCategoryDistrict(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategoryDistrictRepository.GetAllCategoryDistrict(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/CategoryDistrict/GetCategoryDistrictById
        [HttpGet("GetCategoryDistrictById")]
        public async Task<IActionResult> GetCategoryDistrictById(Guid IdCategoryDistrict)
        {
            TemplateApi templateApi = await _CategoryDistrictRepository.GetCategoryDistrictById(IdCategoryDistrict);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/CategoryDistrict/InsertCategoryDistrict
        [HttpPost("InsertCategoryDistrict")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertCategoryDistrict(CategoryDistrictRequest CategoryDistrictRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategoryDistrictDto = CategoryDistrictRequest.Adapt<CategoryDistrictDto>();

            // define some col with data concrete
            CategoryDistrictDto.Id = Guid.NewGuid();
            CategoryDistrictDto.IdUserCurrent = idUserCurrent;
            CategoryDistrictDto.CreatedDate = DateTime.Now;
            CategoryDistrictDto.Status = 0;
            CategoryDistrictDto.IsHide = false;

            TemplateApi result = await _CategoryDistrictRepository.InsertCategoryDistrict(CategoryDistrictDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/CategoryDistrict/UpdateCategoryDistrict
        [HttpPut("UpdateCategoryDistrict")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateCategoryDistrict(CategoryDistrictRequest CategoryDistrictRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategoryDistrictDto = CategoryDistrictRequest.Adapt<CategoryDistrictDto>();
            CategoryDistrictDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _CategoryDistrictRepository.UpdateCategoryDistrict(CategoryDistrictDto);
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
        // HttpDelete: /api/CategoryDistrict/DeleteCategoryDistrict
        [HttpDelete("DeleteCategoryDistrict")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryDistrict(Guid IdCategoryDistrict)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryDistrictRepository.DeleteCategoryDistrict(IdCategoryDistrict, idUserCurrent);

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
        // HttpDelete: /api/CategoryDistrict/DeleteCategoryDistrictByList
        [HttpDelete("DeleteCategoryDistrictByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryDistrictByList(List<Guid> IdCategoryDistrict)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryDistrictRepository.DeleteCategoryDistrictByList(IdCategoryDistrict, idUserCurrent);

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
