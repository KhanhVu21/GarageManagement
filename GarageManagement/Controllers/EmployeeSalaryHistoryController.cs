using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.Employee_Salary_History;
using GarageManagement.Controllers.Payload.Holiday;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using GarageManagement.Services.Repository;
using GarageManagement.Utility;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeSalaryHistoryController: Controller
    {
        #region Variables
        private readonly IEmployee_Salary_HistoryRepository _Employee_Salary_HistoryRepository;
        private readonly ILogger<EmployeeSalaryHistoryController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public EmployeeSalaryHistoryController(IEmployee_Salary_HistoryRepository Employee_Salary_HistoryRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<EmployeeSalaryHistoryController> logger)
        {
            _Employee_Salary_HistoryRepository = Employee_Salary_HistoryRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region METHOD
        // GET: api/Employee_Salary_History/GetListEmployee_Salary_HistoryByIdEmployee
        [HttpGet("GetListEmployee_Salary_HistoryByIdEmployee")]
        public async Task<IActionResult> GetListEmployee_Salary_HistoryByIdEmployee(int pageNumber, int pageSize, Guid IdEmployee)
        {
            TemplateApi templateApi = await _Employee_Salary_HistoryRepository.GetAllEmployee_Salary_History(pageNumber, pageSize, IdEmployee);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Employee_Salary_History/GetEmployee_Salary_HistoryById
        [HttpGet("GetEmployee_Salary_HistoryById")]
        public async Task<IActionResult> GetEmployee_Salary_HistoryById(Guid IdEmployee_Salary_History)
        {
            TemplateApi templateApi = await _Employee_Salary_HistoryRepository.GetEmployee_Salary_HistoryById(IdEmployee_Salary_History);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/Employee_Salary_History/InsertEmployee_Salary_History
        [HttpPost("InsertEmployee_Salary_History")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertEmployee_Salary_History(Employee_Salary_HistoryModel Employee_Salary_HistoryRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var Employee_Salary_HistoryDto = Employee_Salary_HistoryRequest.Adapt<Employee_Salary_HistoryDto>();

            if(Employee_Salary_HistoryRequest.DateSalary > DateTime.Now)
            {
                return Ok(new
                {
                    Success = false,
                    Fail = true,
                    Message = "Ngày tính không thể lớn hơn ngày hiện tại !"
                });
            }

            // define some col with data concrete
            Employee_Salary_HistoryDto.Id = Guid.NewGuid();
            Employee_Salary_HistoryDto.IdUserCurrent = idUserCurrent;
            Employee_Salary_HistoryDto.CreatedDate = DateTime.Now;
            Employee_Salary_HistoryDto.Status = 0;
            Employee_Salary_HistoryRequest.DateSalary = new DateTime(Employee_Salary_HistoryRequest.DateSalary.Value.Year, Employee_Salary_HistoryRequest.DateSalary.Value.Month, Employee_Salary_HistoryRequest.DateSalary.Value.Day);


            TemplateApi result = await _Employee_Salary_HistoryRepository.InsertEmployee_Salary_History(Employee_Salary_HistoryDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        #endregion
    }
}
