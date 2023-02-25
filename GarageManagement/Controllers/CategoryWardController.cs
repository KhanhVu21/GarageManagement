using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.CategoryWard;
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
    public class CategoryWardController: Controller
    {
        #region Variables
        private readonly ICategoryWardRepository _CategoryWardRepository;
        private readonly ILogger<CategoryWardController> _logger;
        #endregion

        #region Contructor
        public CategoryWardController(ICategoryWardRepository CategoryWardRepository,
        ILogger<CategoryWardController> logger)
        {
            _CategoryWardRepository = CategoryWardRepository;
            _logger = logger;
        }
        #endregion

        #region METHOD
        // GET: api/CategoryWard/GetListCategoryWardByIdDistrict
        [HttpGet("GetListCategoryWardByIdDistrict")]
        public async Task<IActionResult> GetListCategoryWardByIdDistrict(int pageNumber, int pageSize, string DistrictCode)
        {
            TemplateApi templateApi = await _CategoryWardRepository.GetAllCategoryWardByIdDistrict(pageNumber, pageSize, DistrictCode);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPut: /api/CategoryWard/HideCategoryWardByList
        [HttpPut("HideCategoryWardByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryWardByList(List<Guid> IdCategoryWard, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryWardRepository.HideCategoryWardByList(IdCategoryWard, idUserCurrent, IsHide);
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
        // HttpPut: /api/CategoryWard/HideCategoryWard
        [HttpPut("HideCategoryWard")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryWard(Guid IdCategoryWard, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryWardRepository.HideCategoryWard(IdCategoryWard, idUserCurrent, IsHide);
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
        // GET: api/CategoryWard/GetListCategoryWard
        [HttpGet("GetListCategoryWard")]
        public async Task<IActionResult> GetListCategoryWard(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategoryWardRepository.GetAllCategoryWard(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/CategoryWard/GetCategoryWardById
        [HttpGet("GetCategoryWardById")]
        public async Task<IActionResult> GetCategoryWardById(Guid IdCategoryWard)
        {
            TemplateApi templateApi = await _CategoryWardRepository.GetCategoryWardById(IdCategoryWard);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/CategoryWard/InsertCategoryWard
        [HttpPost("InsertCategoryWard")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertCategoryWard(CategoryWardRequest CategoryWardRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var categoryWardDto = CategoryWardRequest.Adapt<CategoryWardDto>();

            // define some col with data concrete
            categoryWardDto.Id = Guid.NewGuid();
            categoryWardDto.IdUserCurrent = idUserCurrent;
            categoryWardDto.CreatedDate = DateTime.Now;
            categoryWardDto.Status = 0;
            categoryWardDto.IsHide = false;

            TemplateApi result = await _CategoryWardRepository.InsertCategoryWard(categoryWardDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/CategoryWard/UpdateCategoryWard
        [HttpPut("UpdateCategoryWard")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateCategoryWard(CategoryWardRequest CategoryWardRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategoryWardDto = CategoryWardRequest.Adapt<CategoryWardDto>();
            CategoryWardDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _CategoryWardRepository.UpdateCategoryWard(CategoryWardDto);
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
        // HttpDelete: /api/CategoryWard/DeleteCategoryWard
        [HttpDelete("DeleteCategoryWard")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryWard(Guid IdCategoryWard)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryWardRepository.DeleteCategoryWard(IdCategoryWard, idUserCurrent);

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
        // HttpDelete: /api/CategoryWard/DeleteCategoryWardByList
        [HttpDelete("DeleteCategoryWardByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryWardByList(List<Guid> IdCategoryWard)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryWardRepository.DeleteCategoryWardByList(IdCategoryWard, idUserCurrent);

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
