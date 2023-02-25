using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.Unit;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitController : Controller
    {
        #region Variables
        private readonly IUnitRepository _unitRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UnitController> _logger;
        #endregion

        #region Contructor
        public UnitController(IUnitRepository unitRepository,
        ILogger<UnitController> logger,
        IUserRepository userRepository)
        {
            _unitRepository = unitRepository;
            _userRepository = userRepository;
            _logger = logger;
        }
        #endregion

        #region METHOD
        // GET: api/Unit/GetListUnitAndUser
        [HttpGet("GetListUnitAndUser")]
        public async Task<IActionResult> GetListUnitAndUser(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _unitRepository.GetAllUnitAndUser(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Unit/GetListUnitByIdParent
        [HttpGet("GetListUnitByIdParent")]
        public async Task<IActionResult> GetListUnitByIdParent(int pageNumber, int pageSize, Guid IdUnit)
        {
            TemplateApi templateApi = await _unitRepository.GetAllUnitByIdParent(IdUnit, pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Unit/GetListUnitNotHide
        [HttpGet("GetListUnitNotHide")]
        public async Task<IActionResult> GetListUnitNotHide(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _unitRepository.GetUnitNotHide(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Unit/GetListUnit
        [HttpGet("GetListUnit")]
        public async Task<IActionResult> GetListUnit(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _unitRepository.GetAllUnit(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Unit/GetUnitById
        [HttpGet("GetUnitById")]
        public async Task<IActionResult> GetUnitById(Guid IdUnit)
        {
            TemplateApi templateApi = await _unitRepository.GetUnitById(IdUnit);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/Unit/InsertUnit
        [HttpPost("InsertUnit")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertUnit(UnitRequest unitRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            UnitDto unitDtoByUnitCode = await _unitRepository.GetUnitByUnitCode(unitRequest.UnitCode);
            if (unitDtoByUnitCode.Id != Guid.Empty)
            {
                return Ok(new
                {
                    Success = "Mã phòng ban này đã tồn tại",
                    Fail = true,
                    Message = false
                });
            }

            var unitDto = unitRequest.Adapt<UnitDto>();

            // define some col with data concrete
            unitDto.Id = Guid.NewGuid();
            unitDto.CreatedDate = DateTime.Now;
            unitDto.Status = 0;
            unitDto.IsHide = false;
            unitDto.IdUserCurrent = idUserCurrent;
            unitDto.CreatedBy = idUserCurrent;

            TemplateApi result = await _unitRepository.InsertUnit(unitDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/Unit/UpdateUnit
        [HttpPut("UpdateUnit")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateUnit(UnitRequest unitRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var unitDto = unitRequest.Adapt<UnitDto>();

            unitDto.IdUserCurrent = idUserCurrent;

            TemplateApi result = await _unitRepository.UpdateUnit(unitDto);
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
        // HttpDelete: /api/Unit/DeleteUnit
        [HttpDelete("DeleteUnit")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteUnit(Guid IdUnit)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _unitRepository.DeleteUnit(IdUnit, idUserCurrent);

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
        // HttpPut: /api/Unit/HideUnit
        [HttpPut("HideUnit")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideUnit(Guid IdUnit, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _unitRepository.HideUnit(IdUnit, IsHide, idUserCurrent);

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
