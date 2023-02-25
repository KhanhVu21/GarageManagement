using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.Customer;
using GarageManagement.Controllers.Payload.Vehicle;
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
    public class VehicleController: Controller
    {
        #region Variables
        private readonly IVehicleRepository _VehicleRepository;
        private readonly ILogger<VehicleController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public VehicleController(IVehicleRepository VehicleRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<VehicleController> logger)
        {
            _VehicleRepository = VehicleRepository;
            _appSettingModel = optionsMonitor.CurrentValue;
            _logger = logger;
        }
        #endregion

        #region METHOD
        // GET: api/Vehicle/GetListVehicleByIdCustomer
        [HttpGet("GetListVehicleByIdCustomer")]
        public async Task<IActionResult> GetListVehicleByIdCustomer(int pageNumber, int pageSize, Guid IdCustomer)
        {
            TemplateApi templateApi = await _VehicleRepository.GetAllVehicleByIdCustomer(pageNumber, pageSize, IdCustomer);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Vehicle/GetListCustomerByIdVehicle
        [HttpGet("GetListCustomerByIdVehicle")]
        public async Task<IActionResult> GetListCustomerByIdVehicle(int pageNumber, int pageSize, Guid IdVehicle)
        {
            TemplateApi templateApi = await _VehicleRepository.GetAllCustomerByIdVehicle(pageNumber, pageSize, IdVehicle);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPut: /api/Vehicle/HideVehicleByList
        [HttpPut("HideVehicleByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideVehicleByList(List<Guid> IdVehicle, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _VehicleRepository.HideVehicleByList(IdVehicle, idUserCurrent, IsHide);
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
        // HttpPut: /api/Vehicle/HideVehicle
        [HttpPut("HideVehicle")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideVehicle(Guid IdVehicle, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _VehicleRepository.HideVehicle(IdVehicle, idUserCurrent, IsHide);
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
        // GET: api/Vehicle/GetListVehicle
        [HttpGet("GetListVehicle")]
        public async Task<IActionResult> GetListVehicle(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _VehicleRepository.GetAllVehicle(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Vehicle/GetVehicleById
        [HttpGet("GetVehicleById")]
        public async Task<IActionResult> GetVehicleById(Guid IdVehicle)
        {
            TemplateApi templateApi = await _VehicleRepository.GetVehicleById(IdVehicle);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/Vehicle/InsertVehicle
        [HttpPost("InsertVehicle")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertVehicle([FromForm]VehicleRequest VehicleRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var vehicleDto = VehicleRequest.Adapt<VehicleDto>();

            // define some col with data concrete
            vehicleDto.Id = Guid.NewGuid();
            vehicleDto.IdUserCurrent = idUserCurrent;
            vehicleDto.CreatedDate = DateTime.Now;
            vehicleDto.Status = 0;
            vehicleDto.IsHide = false;

            // If directory does not exist, create it. 
            if (!Directory.Exists(_appSettingModel.Root))
            {
                Directory.CreateDirectory(_appSettingModel.Root);
            }
            if (!Directory.Exists(_appSettingModel.ServerFileAvartar))
            {
                Directory.CreateDirectory(_appSettingModel.ServerFileAvartar);
            }

            if (Request.Form.Files.Count > 0)
            {
                foreach (var file in Request.Form.Files)
                {
                    var fileContentType = file.ContentType;

                    if (fileContentType == "image/jpeg"
                    || fileContentType == "image/png" || fileContentType == "image/jpg")
                    {
                        // prepare path to save file image
                        string pathTo = _appSettingModel.ServerFileAvartar;
                        // get extention form file name
                        string IdFile = vehicleDto.Id.ToString() + ".jpg";

                        // set file path to save file
                        var filename = Path.Combine(pathTo, Path.GetFileName(IdFile));

                        // save file
                        using (var stream = System.IO.File.Create(filename))
                        {
                            file.CopyTo(stream);
                        }
                        vehicleDto.Avatar = file.FileName;
                    }
                }
            }

            TemplateApi result = await _VehicleRepository.InsertVehicle(vehicleDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/Vehicle/UpdateVehicle
        [HttpPut("UpdateVehicle")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateVehicle([FromForm]VehicleRequest VehicleRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var VehicleDto = VehicleRequest.Adapt<VehicleDto>();
            VehicleDto.IdUserCurrent = idUserCurrent;

            // If directory does not exist, create it. 
            if (!Directory.Exists(_appSettingModel.Root))
            {
                Directory.CreateDirectory(_appSettingModel.Root);
            }
            if (!Directory.Exists(_appSettingModel.ServerFileAvartar))
            {
                Directory.CreateDirectory(_appSettingModel.ServerFileAvartar);
            }

            //delete file logo
            if (VehicleRequest.idFile is null)
            {
                string IdFile = VehicleDto.Id.ToString() + ".jpg";

                // set file path to save file
                var filename = Path.Combine(_appSettingModel.ServerFileAvartar, Path.GetFileName(IdFile));
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
                        string pathTo = _appSettingModel.ServerFileAvartar;
                        // get extention form file name
                        string IdFile = VehicleDto.Id.ToString() + ".jpg";

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
                        VehicleDto.Avatar = file.FileName;
                    }
                }
            }

            TemplateApi result = await _VehicleRepository.UpdateVehicle(VehicleDto);
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
        // HttpDelete: /api/Vehicle/DeleteVehicle
        [HttpDelete("DeleteVehicle")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteVehicle(Guid IdVehicle)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _VehicleRepository.DeleteVehicle(IdVehicle, idUserCurrent);

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
        // HttpDelete: /api/Vehicle/DeleteVehicleByList
        [HttpDelete("DeleteVehicleByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteVehicleByList(List<Guid> IdVehicle)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _VehicleRepository.DeleteVehicleByList(IdVehicle, idUserCurrent);

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
