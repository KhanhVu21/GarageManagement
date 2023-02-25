using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.Engineer;
using GarageManagement.Controllers.Payload.Holiday;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using GarageManagement.Utility;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Globalization;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController: Controller
    {
        #region Variables
        private readonly IEmployeeRepository _EmployeeRepository;
        private readonly IEmployee_Salary_HistoryRepository _Salary_HistoryRepository;
        private readonly ILogger<EmployeeController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public EmployeeController(IEmployeeRepository EmployeeRepository,
            IEmployee_Salary_HistoryRepository Salary_HistoryRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<EmployeeController> logger)
        {
            _EmployeeRepository = EmployeeRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
            _Salary_HistoryRepository = Salary_HistoryRepository;
        }
        #endregion

        #region METHOD
        // GET: api/Employee/GetExcelSalaryCalculateAllEmployeeByMonth
        [HttpGet]
        [Route("GetExcelSalaryCalculateAllEmployeeByMonth")]
        public IActionResult GetExcelSalaryCalculateAllEmployeeByMonth(string date)
        {
            var dateDetail = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var path = string.Concat(_appSettingModel.ServerFileExcel, "\\", "Mẫu thanh toán tiền lương.xlsx");
            FileInfo fi = new FileInfo(path);

            using (ExcelPackage excelPackage = new ExcelPackage(fi))
            {
                //Get a WorkSheet by name. If the worksheet doesn't exist, throw an exeption
                ExcelWorksheet namedWorksheet = excelPackage.Workbook.Worksheets[0];
                namedWorksheet.Cells["A5:J2000"].Clear();

                namedWorksheet.Cells[$"C3:D3"].Merge = true;
                namedWorksheet.Cells[3, 3].Style.Font.Size = 13;
                namedWorksheet.Cells[3, 3].Style.Font.Name = "Times New Roman";
                namedWorksheet.Cells[3, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                namedWorksheet.Cells[3, 3].Value = "Thời gian: " + date;

                var historySalary = _Salary_HistoryRepository.Employee_Salary_HistoryByIdEmployeeAndMonth(dateDetail.Month);


                int startRow = 5;
                int count = 1;
                foreach(var item in historySalary.Result)
                {
                    for (int j = 1; j <= 6; j++)
                    {
                        namedWorksheet.Cells[startRow, j].Style.Font.Size = 13;
                        namedWorksheet.Cells[startRow, j].Style.Font.Name = "Times New Roman";
                        namedWorksheet.Cells[startRow, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                    }
                    var employee = _EmployeeRepository.GetEmployeeExcel(item.IdEmployee ?? Guid.Empty);

                    namedWorksheet.Cells[startRow, 1].Value = count;
                    namedWorksheet.Cells[startRow, 2].Value = employee.Result.Name;
                    namedWorksheet.Cells[startRow, 3].Value = employee.Result.Code;
                    namedWorksheet.Cells[startRow, 4].Value = item.SocialInsurance?.ToString("C0", new CultureInfo("vi-VN")) ?? "";
                    namedWorksheet.Cells[startRow, 5].Value = item.TaxPay?.ToString("C0", new CultureInfo("vi-VN")) ?? "";
                    namedWorksheet.Cells[startRow, 6].Value = item.TotalSalaryReality?.ToString("C0", new CultureInfo("vi-VN")) ?? "";
                    count++;   
                    startRow++;
                }

                //overwrite to file old
                FileInfo fiToSave = new FileInfo(path);
                //Save your file
                excelPackage.SaveAs(fiToSave);
            }
            //download file excel
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "Mẫu thanh toán tiền lương.xlsx");
        }
        // GET: api/Employee/GetExcelSalaryCalculateEmployeeByMonth
        [HttpGet]
        [Route("GetExcelSalaryCalculateEmployeeByMonth")]
        public IActionResult GetExcelSalaryCalculateEmployeeByMonth(Guid IdEmployee, string date)
        {
            var dateDetail = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var path = string.Concat(_appSettingModel.ServerFileExcel, "\\", "Mẫu thanh toán tiền lương.xlsx");
            FileInfo fi = new FileInfo(path);

            using (ExcelPackage excelPackage = new ExcelPackage(fi))
            {
                //Get a WorkSheet by name. If the worksheet doesn't exist, throw an exeption
                ExcelWorksheet namedWorksheet = excelPackage.Workbook.Worksheets[0];
                namedWorksheet.Cells["A5:J2000"].Clear();

                namedWorksheet.Cells[$"C3:D3"].Merge = true;
                namedWorksheet.Cells[3, 3].Style.Font.Size = 13;
                namedWorksheet.Cells[3, 3].Style.Font.Name = "Times New Roman";
                namedWorksheet.Cells[3, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                namedWorksheet.Cells[3, 3].Value = "Thời gian: " + date;

                var historySalary = _Salary_HistoryRepository.Employee_Salary_HistoryByIdEmployeeAndMonth(IdEmployee, dateDetail.Month);
                var employee = _EmployeeRepository.GetEmployeeExcel(IdEmployee);

                for(int i  = 1; i <= 6;i++)
                {
                    namedWorksheet.Cells[5, i].Style.Font.Size = 13;
                    namedWorksheet.Cells[5, i].Style.Font.Name = "Times New Roman";
                    namedWorksheet.Cells[5, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                }
                namedWorksheet.Cells[5, 1].Value = "1";
                namedWorksheet.Cells[5, 2].Value = employee.Result.Name;
                namedWorksheet.Cells[5, 3].Value = employee.Result.Code;
                namedWorksheet.Cells[5, 4].Value = historySalary.Result.SocialInsurance?.ToString("C0", new CultureInfo("vi-VN")) ?? "";
                namedWorksheet.Cells[5, 5].Value = historySalary.Result.TaxPay?.ToString("C0", new CultureInfo("vi-VN")) ?? "";
                namedWorksheet.Cells[5, 6].Value = historySalary.Result.TotalSalaryReality?.ToString("C0", new CultureInfo("vi-VN")) ?? "";

                //overwrite to file old
                FileInfo fiToSave = new FileInfo(path);
                //Save your file
                excelPackage.SaveAs(fiToSave);
            }
            //download file excel
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "Mẫu thanh toán tiền lương.xlsx");
        }
        // GET: api/Employee/SalaryCalculateAllEmployeeByMonth
        [HttpGet("SalaryCalculateAllEmployeeByMonth")]
        public async Task<IActionResult> SalaryCalculateAllEmployeeByMonth(string date, int pageNumber, int pageSize)
        {
            var dateDetail = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            if (dateDetail.Year != DateTime.Now.Year)
            {
                return Ok(new
                {
                    Success = false,
                    Fail = true,
                    Message = "Không thể tính lương cho những năm lớn hơn hoặc bé hơn hiện tại !"
                });
            }

            if (dateDetail.Month > DateTime.Now.Month)
            {
                return Ok(new
                {
                    Success = false,
                    Fail = true,
                    Message = "Không thể tính lương cho những tháng lớn hơn !"
                });
            }

            TemplateApi templateApi = await _EmployeeRepository.SalaryCalculateAllEmployeeByMonth(dateDetail, pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Employee/SalaryCalculateByMonth
        [HttpGet("SalaryCalculateByMonth")]
        public async Task<IActionResult> SalaryCalculateByMonth(string date, Guid IdEmployee)
        {
            var dateDetail =  DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            if (dateDetail.Year != DateTime.Now.Year)
            {
                return Ok(new
                {
                    Success = false,
                    Fail = true,
                    Message = "Không thể tính lương cho những năm lớn hơn hoặc bé hơn hiện tại !"
                });
            }

            if (dateDetail.Month > DateTime.Now.Month)
            {
                return Ok(new
                {
                    Success = false,
                    Fail = true,
                    Message = "Không thể tính lương cho những tháng lớn hơn !"
                });
            }

            TemplateApi templateApi = await _EmployeeRepository.SalaryCalculateByMonth(IdEmployee, dateDetail);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Employee/GetListEmployeeByIdGroup
        [HttpGet("GetListEmployeeByIdGroup")]
        public async Task<IActionResult> GetListEmployeeByIdGroup(int pageNumber, int pageSize, Guid IdGroup)
        {
            TemplateApi templateApi = await _EmployeeRepository.GetAllEmployeeByIdGroup(pageNumber, pageSize, IdGroup);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPut: /api/Employee/HideEmployeeByList
        [HttpPut("HideEmployeeByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideEmployeeByList(List<Guid> IdEmployee, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _EmployeeRepository.HideEmployeeByList(IdEmployee, idUserCurrent, IsHide);
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
        // HttpPut: /api/Employee/HideEmployee
        [HttpPut("HideEmployee")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> HideEmployee(Guid IdEmployee, bool IsHide)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _EmployeeRepository.HideEmployee(IdEmployee, idUserCurrent, IsHide);
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
        // GET: api/Employee/GetListEmployee
        [HttpGet("GetListEmployee")]
        public async Task<IActionResult> GetListEmployee(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _EmployeeRepository.GetAllEmployee(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Employee/GetEmployeeById
        [HttpGet("GetEmployeeById")]
        public async Task<IActionResult> GetEmployeeById(Guid IdEmployee)
        {
            TemplateApi templateApi = await _EmployeeRepository.GetEmployeeById(IdEmployee);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/Employee/InsertEmployee
        [HttpPost("InsertEmployee")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertEmployee([FromForm]EmployeeRequest EmployeeRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var EmployeeDto = EmployeeRequest.Adapt<EmployeeDto>();

            // define some col with data concrete
            EmployeeDto.Id = Guid.NewGuid();
            EmployeeDto.IdUserCurrent = idUserCurrent;
            EmployeeDto.CreatedDate = DateTime.Now;
            EmployeeDto.Status = 0;
            EmployeeDto.Code = "EG - " + DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();


            // If directory does not exist, create it. 
            if (!Directory.Exists(_appSettingModel.Root))
            {
                Directory.CreateDirectory(_appSettingModel.Root);
            }
            if (!Directory.Exists(_appSettingModel.ServerFileAvartar))
            {
                Directory.CreateDirectory(_appSettingModel.ServerFileAvartar);
            }

            //save image
            if (Request.Form.Files.Count > 0)
            {
                foreach (var file in Request.Form.Files)
                {
                    var fileContentType = file.ContentType;

                    if (fileContentType == "image/jpeg"
                    || fileContentType == "image/png" || fileContentType == "image/jpg")
                    {
                        var filename = Path.Combine(_appSettingModel.ServerFileAvartar, Path.GetFileName($"{EmployeeDto.Id}.jpg"));

                        // save file
                        using (var stream = System.IO.File.Create(filename))
                        {
                            file.CopyTo(stream);
                        }
                        EmployeeDto.Avatar = file.FileName;
                    }
                }
            }

            TemplateApi result = await _EmployeeRepository.InsertEmployee(EmployeeDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/Employee/UpdateEmployee
        [HttpPut("UpdateEmployee")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateEmployee([FromForm]EmployeeRequest EmployeeRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var EmployeeDto = EmployeeRequest.Adapt<EmployeeDto>();
            EmployeeDto.IdUserCurrent = idUserCurrent;

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
            if (EmployeeRequest.idFile is null)
            {
                string IdFile = EmployeeDto.Id.ToString() + ".jpg";

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
                        var filename = Path.Combine(_appSettingModel.ServerFileAvartar, Path.GetFileName($"{EmployeeDto.Id}.jpg"));

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
                        EmployeeDto.Avatar = file.FileName;
                    }
                }
            }

            TemplateApi result = await _EmployeeRepository.UpdateEmployee(EmployeeDto);
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
        // HttpDelete: /api/Employee/DeleteEmployee
        [HttpDelete("DeleteEmployee")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteEmployee(Guid IdEmployee)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _EmployeeRepository.DeleteEmployee(IdEmployee, idUserCurrent);

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
        // HttpDelete: /api/Employee/DeleteEmployeeByList
        [HttpDelete("DeleteEmployeeByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteEmployeeByList(List<Guid> IdEmployee)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _EmployeeRepository.DeleteEmployeeByList(IdEmployee, idUserCurrent);

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
