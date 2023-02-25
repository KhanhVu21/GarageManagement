using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.CategoryBrandVehicle;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using GarageManagement.Utility;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryBrandVehicleController: Controller
    {
        #region Variables
        private readonly ICategoryBrandVehicleRepository _CategoryBrandVehicleRepository;
        private readonly ILogger<CategoryBrandVehicleController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public CategoryBrandVehicleController(ICategoryBrandVehicleRepository CategoryBrandVehicleRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<CategoryBrandVehicleController> logger)
        {
            _CategoryBrandVehicleRepository = CategoryBrandVehicleRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region METHOD
        // GET: api/CategoryBrandVehicle/GetListCategoryBrandVehicleAvailable
        [HttpGet("GetListCategoryBrandVehicleAvailable")]
        public async Task<IActionResult> GetListCategoryBrandVehicleAvailable(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategoryBrandVehicleRepository.GetAllCategoryBrandVehicleAvailable(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpGet: api/CategoryBrandVehicle/GetFileLogoCategoryBrandVehicle
        [HttpGet]
        [Route("GetFileLogoCategoryBrandVehicle")]
        public IActionResult GetFileLogoCategoryBrandVehicle(string fileNameId)
        {
            var temp = fileNameId.Split('.');
            byte[] fileBytes = Array.Empty<byte>();
            if (temp[1] == "jpg" || temp[1] == "png")
            {
                fileBytes = System.IO.File.ReadAllBytes(string.Concat(_appSettingModel.ServerFileLogo, "\\", fileNameId));
            }
            return File(fileBytes, "image/jpeg");
        }
        // HttpPut: /api/CategoryBrandVehicle/HideCategoryBrandVehicleByList
        [HttpPut("HideCategoryBrandVehicleByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryBrandVehicleByList(List<Guid> IdCategoryBrandVehicle, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryBrandVehicleRepository.HideCategoryBrandVehicleByList(IdCategoryBrandVehicle, idUserCurrent, IsHide);
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
        // HttpPut: /api/CategoryBrandVehicle/HideCategoryBrandVehicle
        [HttpPut("HideCategoryBrandVehicle")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideCategoryBrandVehicle(Guid IdCategoryBrandVehicle, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryBrandVehicleRepository.HideCategoryBrandVehicle(IdCategoryBrandVehicle, idUserCurrent, IsHide);
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
        // GET: api/CategoryBrandVehicle/GetListCategoryBrandVehicle
        [HttpGet("GetListCategoryBrandVehicle")]
        public async Task<IActionResult> GetListCategoryBrandVehicle(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _CategoryBrandVehicleRepository.GetAllCategoryBrandVehicle(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/CategoryBrandVehicle/GetCategoryBrandVehicleById
        [HttpGet("GetCategoryBrandVehicleById")]
        public async Task<IActionResult> GetCategoryBrandVehicleById(Guid IdCategoryBrandVehicle)
        {
            TemplateApi templateApi = await _CategoryBrandVehicleRepository.GetCategoryBrandVehicleById(IdCategoryBrandVehicle);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/CategoryBrandVehicle/InsertCategoryBrandVehicle
        [HttpPost("InsertCategoryBrandVehicle")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertCategoryBrandVehicle([FromForm]CategoryBrandVehicleRequest CategoryBrandVehicleRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var categoryBrandVehicleDto = CategoryBrandVehicleRequest.Adapt<CategoryBrandVehicleDto>();

            // define some col with data concrete
            categoryBrandVehicleDto.Id = Guid.NewGuid();
            categoryBrandVehicleDto.IdUserCurrent = idUserCurrent;
            categoryBrandVehicleDto.CreatedDate = DateTime.Now;
            categoryBrandVehicleDto.Status = 0;
            categoryBrandVehicleDto.IsHide = false;
            categoryBrandVehicleDto.Code = "BR - " + DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

            // If directory does not exist, create it. 
            if (!Directory.Exists(_appSettingModel.Root))
            {
                Directory.CreateDirectory(_appSettingModel.Root);
            }
            if (!Directory.Exists(_appSettingModel.ServerFileLogo))
            {
                Directory.CreateDirectory(_appSettingModel.ServerFileLogo);
            }

            //save image user
            if (Request.Form.Files.Count > 0)
            {
                foreach (var file in Request.Form.Files)
                {
                    var fileContentType = file.ContentType;

                    if (fileContentType == "image/jpeg"
                    || fileContentType == "image/png" || fileContentType == "image/jpg")
                    {
                        var filename = Path.Combine(_appSettingModel.ServerFileLogo, Path.GetFileName($"{categoryBrandVehicleDto.Id}.jpg"));

                        // save file
                        using (var stream = System.IO.File.Create(filename))
                        {
                            file.CopyTo(stream);
                        }
                        categoryBrandVehicleDto.logo = file.FileName;
                    }
                }
            }

            TemplateApi result = await _CategoryBrandVehicleRepository.InsertCategoryBrandVehicle(categoryBrandVehicleDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/CategoryBrandVehicle/UpdateCategoryBrandVehicle
        [HttpPut("UpdateCategoryBrandVehicle")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateCategoryBrandVehicle([FromForm]CategoryBrandVehicleRequest CategoryBrandVehicleRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CategoryBrandVehicleDto = CategoryBrandVehicleRequest.Adapt<CategoryBrandVehicleDto>();
            CategoryBrandVehicleDto.IdUserCurrent = idUserCurrent;

            // If directory does not exist, create it. 
            if (!Directory.Exists(_appSettingModel.Root))
            {
                Directory.CreateDirectory(_appSettingModel.Root);
            }
            if (!Directory.Exists(_appSettingModel.ServerFileLogo))
            {
                Directory.CreateDirectory(_appSettingModel.ServerFileLogo);
            }
            //delete file logo
            if(CategoryBrandVehicleRequest.idFile is null)
            {
                string IdFile = CategoryBrandVehicleDto.Id.ToString() + ".jpg";

                // set file path to save file
                var filename = Path.Combine(_appSettingModel.ServerFileLogo, Path.GetFileName(IdFile));
                //delete file before save
                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }
            }

            // check file exits
            if (Request.Form.Files.Count > 0)
            {
                foreach (var file in Request.Form.Files)
                {
                    var fileContentType = file.ContentType;

                    if (fileContentType == "image/jpeg" || fileContentType == "image/png"
                        || fileContentType == "image/jpg")
                    {
                        // prepare path to save file image
                        string pathTo = _appSettingModel.ServerFileLogo;
                        // get extention form file name
                        string IdFile = CategoryBrandVehicleDto.Id.ToString() + ".jpg";

                        // set file path to save file
                        var filename = Path.Combine(pathTo, Path.GetFileName(IdFile));

                        //delete file before save
                        if (System.IO.File.Exists(filename))
                        {
                            System.IO.File.Delete(filename);
                        }

                        // save file
                        using (var stream = System.IO.File.Create(filename))
                        {
                            file.CopyTo(stream);
                        }
                        // set data document avatar
                        CategoryBrandVehicleDto.logo = file.FileName;
                    }
                }
            }

            TemplateApi result = await _CategoryBrandVehicleRepository.UpdateCategoryBrandVehicle(CategoryBrandVehicleDto);
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
        // HttpDelete: /api/CategoryBrandVehicle/DeleteCategoryBrandVehicle
        [HttpDelete("DeleteCategoryBrandVehicle")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryBrandVehicle(Guid IdCategoryBrandVehicle)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryBrandVehicleRepository.DeleteCategoryBrandVehicle(IdCategoryBrandVehicle, idUserCurrent);

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
        // HttpDelete: /api/CategoryBrandVehicle/DeleteCategoryBrandVehicleByList
        [HttpDelete("DeleteCategoryBrandVehicleByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCategoryBrandVehicleByList(List<Guid> IdCategoryBrandVehicle)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _CategoryBrandVehicleRepository.DeleteCategoryBrandVehicleByList(IdCategoryBrandVehicle, idUserCurrent);

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
