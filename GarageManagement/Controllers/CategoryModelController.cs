using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.CategoryModel;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Microsoft.AspNetCore.Mvc;
using Mapster;
using GarageManagement.Services.Repository;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryModelController: Controller
    {
        #region Variables
        private readonly ICategoryModelRepository _CategoryModelRepository;
        private readonly ILogger<CategoryModelController> _logger;
        #endregion

        #region Contructor
        public CategoryModelController(ICategoryModelRepository CategoryModelRepository,
        ILogger<CategoryModelController> logger)
        {
            _CategoryModelRepository = CategoryModelRepository;
            _logger = logger;
        }
        #endregion

        #region METHOD
        // GET: api/CategoryModel/GetListCategoryModelAvailable
        [HttpGet("GetListCategoryModelAvailable")]
        public async Task<IActionResult> GetListCategoryModelAvailable(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategoryModelRepository.GetAllCategoryModelAvailable(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/CategoryModel/GetAllCategoryModelByIdCategoryBrandVehicle
        [HttpGet("GetAllCategoryModelByIdCategoryBrandVehicle")]
        public async Task<IActionResult> GetAllCategoryModelByIdCategoryBrandVehicle(int pageNumber, int pageSize, Guid IdCategoryBrandVehicle)
        {
            TemplateApi templateApi = await _CategoryModelRepository.GetAllCategoryModelByIdIdCategoryBrandVehicle(pageNumber, pageSize, IdCategoryBrandVehicle);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPut: /api/CategoryModel/HideCategoryModelByList
        [HttpPut("HideCategoryModelByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryModelByList(List<Guid> IdCategoryModel, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryModelRepository.HideCategoryModelByList(IdCategoryModel, idUserCurrent, IsHide);
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
        // HttpPut: /api/CategoryModel/HideCategoryModel
        [HttpPut("HideCategoryModel")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryModel(Guid IdCategoryModel, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryModelRepository.HideCategoryModel(IdCategoryModel, idUserCurrent, IsHide);
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
        // GET: api/CategoryModel/GetListCategoryModel
        [HttpGet("GetListCategoryModel")]
        public async Task<IActionResult> GetListCategoryModel(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategoryModelRepository.GetAllCategoryModel(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/CategoryModel/GetCategoryModelById
        [HttpGet("GetCategoryModelById")]
        public async Task<IActionResult> GetCategoryModelById(Guid IdCategoryModel)
        {
            TemplateApi templateApi = await _CategoryModelRepository.GetCategoryModelById(IdCategoryModel);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/CategoryModel/InsertCategoryModel
        [HttpPost("InsertCategoryModel")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertCategoryModel(CategoryModelRequest CategoryModelRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategoryModelDto = CategoryModelRequest.Adapt<CategoryModelDto>();

            // define some col with data concrete
            CategoryModelDto.Id = Guid.NewGuid();
            CategoryModelDto.IdUserCurrent = idUserCurrent;
            CategoryModelDto.CreatedDate = DateTime.Now;
            CategoryModelDto.Status = 0;
            CategoryModelDto.IsHide = false;

            TemplateApi result = await _CategoryModelRepository.InsertCategoryModel(CategoryModelDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/CategoryModel/UpdateCategoryModel
        [HttpPut("UpdateCategoryModel")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateCategoryModel(CategoryModelRequest CategoryModelRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategoryModelDto = CategoryModelRequest.Adapt<CategoryModelDto>();
            CategoryModelDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _CategoryModelRepository.UpdateCategoryModel(CategoryModelDto);
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
        // HttpDelete: /api/CategoryModel/DeleteCategoryModel
        [HttpDelete("DeleteCategoryModel")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryModel(Guid IdCategoryModel)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryModelRepository.DeleteCategoryModel(IdCategoryModel, idUserCurrent);

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
        // HttpDelete: /api/CategoryModel/DeleteCategoryModelByList
        [HttpDelete("DeleteCategoryModelByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryModelByList(List<Guid> IdCategoryModel)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryModelRepository.DeleteCategoryModelByList(IdCategoryModel, idUserCurrent);

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
