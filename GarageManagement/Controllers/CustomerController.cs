using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.Customer;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using GarageManagement.Utility;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController: Controller
    {
        #region Variables
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomerController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public CustomerController(ICustomerRepository customerRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor,
        ILogger<CustomerController> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        #endregion

        #region Method
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        #endregion

        #region METHOD
        // HttpGet: /api/Customer/GetCustomerByEmailorPhone
        [HttpGet("GetCustomerByEmailorPhone")]
        public async Task<IActionResult> GetCustomerByEmailorPhone(String filter, int pageNumber, int pageSize)
        {
            TemplateApi result = await _customerRepository.GetCustomerByPhoneOrByEmail(filter, pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(result);
        }
        // HttpGet: /api/Customer/GetInforRepairOrderById
        [HttpGet("GetInforRepairOrderById")]
        public async Task<IActionResult> GetInforRepairOrderById(Guid IdRepairOrder)
        {
            var jwt = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (jwt is null)
            {
                return Ok(new { Message = "Không tìm thấy token !", Success = false, Fail = true });
            }

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? tokenS = handler.ReadToken(jwt) as JwtSecurityToken;
            string expire = tokenS.Claims.First(claim => claim.Type == "exp").Value;

            double doubleVal = Convert.ToDouble(expire);
            DateTime DateAfterConvert = UnixTimeStampToDateTime(doubleVal);

            if (DateAfterConvert < DateTime.Now)
            {
                return Ok(new
                {
                    Message = "Token đã hết hạn !",
                    Fail = true,
                    Success = false
                });
            }

            string cusCode = tokenS.Claims.First(claim => claim.Type == "sub").Value;

            TemplateApi result = await _customerRepository.GetInforRepairOrderById(IdRepairOrder, cusCode);
            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(result);
        }
        // HttpGet: /api/Customer/GetAllRepairOrderByCustomer
        [HttpGet("GetAllRepairOrderByCustomer")]
        public async Task<IActionResult> GetAllRepairOrderByCustomer(int pageNumber, int pageSize)
        {
            var jwt = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (jwt is null)
            {
                return Ok(new { Message = "Không tìm thấy token !", Success = false, Fail = true });
            }

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? tokenS = handler.ReadToken(jwt) as JwtSecurityToken;
            string expire = tokenS.Claims.First(claim => claim.Type == "exp").Value;

            double doubleVal = Convert.ToDouble(expire);
            DateTime DateAfterConvert = UnixTimeStampToDateTime(doubleVal);

            if (DateAfterConvert < DateTime.Now)
            {
                return Ok(new
                {
                    Message = "Token đã hết hạn !",
                    Fail = true,
                    Success = false
                });
            }

            string cusCode = tokenS.Claims.First(claim => claim.Type == "sub").Value;

            TemplateApi result = await _customerRepository.GetAllRepairOrderByCustomer(cusCode, pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(result);
        }
        // HttpPut: /api/Customer/RefreshOtp
        [HttpPut("RefreshOtp")]
        public async Task<IActionResult> RefreshOtp(String email, String cusCode)
        {
            var generator = new Random();
            String random = generator.Next(0, 1000000).ToString("D6");

            TemplateApi result = await _customerRepository.RefreshOtp(email, random, cusCode);
            if(!result.Success) {
                return Ok(new
                {
                    Success = result.Success,
                    Fail = result.Fail,
                    Message = result.Message
                });
            }

            // prepare content for mail
            string fromMail = _appSettingModel.FromMail;
            string password = _appSettingModel.Password;
            string Subject = "Mã xác thực tài khoản khách hàng !";
            string body =
            $"<div style='max-width: 700px; margin: auto; border: 10px solid #ddd; padding: 50px 20px; font-size: 110%;'>" +
            $"<h2 style='text-align: center; text-transform: uppercase;color: teal;'>Dưới đây là mã xác thực khách hàng của bạn </h2>" +
            $"<p>Chúc mừng bạn đã đăng kí thành công tài khoản. Hãy lấy mã và kích hoạt tài khoản của bạn !</p>" +
            $"<a style='background: crimson; text-decoration: none; color: white; padding: 10px 20px; margin-left: 300px; display: inline-block;text-align:center'>{random}</a>";

            //send mail confirm
            var sendMail = new SendMail();
            sendMail.SendMailAuto(fromMail, email, password, Subject, body);

            _logger.LogInformation("Thành công : {message}", result.Message);

            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpGet: /api/Customer/VerifyOtp
        [HttpGet("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp(String cusCode, String email, String otp)
        {
            TemplateApi result = await _customerRepository.VerifyOtp(cusCode, email, otp);
            _logger.LogInformation("Thành công : {message}", result.Message);

            if(result.Success)
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettingModel.SecretKey);

                var tokenDescription = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] {
                        new Claim(JwtRegisteredClaimNames.Email, email),
                        new Claim(JwtRegisteredClaimNames.Sub, cusCode),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddYears(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature)
                };

                var token = jwtTokenHandler.CreateToken(tokenDescription);
                var accessToken = jwtTokenHandler.WriteToken(token);

                return Ok(new
                {
                    Success = result.Success,
                    Fail = result.Fail,
                    Message = result.Message,
                    Token = accessToken
                });
            }

            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message,
                Token = ""
            });
        }
        // HttpPost: /api/Customer/InsertCustomerNotToken
        [HttpPost("InsertCustomerNotToken")]
        public async Task<IActionResult> InsertCustomerNotToken([FromForm] CustomerRequest CustomerRequest)
        {
            var generator = new Random();
            String random = generator.Next(0, 1000000).ToString("D6");

            var CustomerDto = CustomerRequest.Adapt<CustomerDto>();

            // define some col with data concrete
            CustomerDto.Id = Guid.NewGuid();
            CustomerDto.CreatedDate = DateTime.Now;
            CustomerDto.Status = 0;
            CustomerDto.Code = "KH - " + DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            CustomerDto.Otp = Int32.Parse(random);


            // If directory does not exist, create it. 
            if (!Directory.Exists(_appSettingModel.Root))
            {
                Directory.CreateDirectory(_appSettingModel.Root);
            }
            if (!Directory.Exists(_appSettingModel.ServerFileAvartar))
            {
                Directory.CreateDirectory(_appSettingModel.ServerFileAvartar);
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
                        // set file path to save file
                        var filename = Path.Combine(_appSettingModel.ServerFileAvartar, Path.GetFileName($"{CustomerDto.Id}.jpg"));

                        // save file
                        using (var stream = System.IO.File.Create(filename))
                        {
                            file.CopyTo(stream);
                        }
                        CustomerDto.Avatar = file.FileName;
                    }
                }
            }

            TemplateApi result = await _customerRepository.InsertCustomerNotToken(CustomerDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            if (result.Success)
            {
                // prepare content for mail
                string fromMail = _appSettingModel.FromMail;
                string password = _appSettingModel.Password;
                string Subject = "Mã xác thực tài khoản khách hàng !";
                string body =
                $"<div style='max-width: 700px; margin: auto; border: 10px solid #ddd; padding: 50px 20px; font-size: 110%;'>" +
                $"<h2 style='text-align: center; text-transform: uppercase;color: teal;'>Dưới đây là mã xác thực khách hàng của bạn </h2>" +
                $"<p>Chúc mừng bạn đã đăng kí thành công tài khoản. Hãy lấy mã và kích hoạt tài khoản của bạn !</p>" +
                $"<a style='background: crimson; text-decoration: none; color: white; padding: 10px 20px; margin-left: 300px; display: inline-block;text-align:center'>{CustomerDto.Code}</a>";

                //send mail confirm
                var sendMail = new SendMail();
                sendMail.SendMailAuto(fromMail, CustomerRequest.Email ?? "", password, Subject, body);
            }
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpGet: api/Customer/GetFileCustomer
        [HttpGet("GetFileCustomer")]
        public IActionResult GetFileCustomer(string fileNameId)
        {
            var temp = fileNameId.Split('.');
            byte[] fileBytes = Array.Empty<byte>();
            if (temp[1] == "jpg" || temp[1] == "png")
            {
                fileBytes = System.IO.File.ReadAllBytes(string.Concat(_appSettingModel.ServerFileAvartar, "\\", fileNameId));
            }
            return File(fileBytes, "image/jpeg");
        }
        // GET: api/Customer/GetListCustomerByIdGroup
        [HttpGet("GetListCustomerByIdGroup")]
        public async Task<IActionResult> GetListCustomerByIdGroup(int pageNumber, int pageSize, Guid IdGroup)
        {
            TemplateApi templateApi = await _customerRepository.GetAllCustomerByIdGroup(pageNumber, pageSize, IdGroup);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Customer/GetListCustomer
        [HttpGet("GetListCustomer")]
        public async Task<IActionResult> GetListCustomer(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _customerRepository.GetAllCustomer(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/Customer/GetCustomerById
        [HttpGet("GetCustomerById")]
        public async Task<IActionResult> GetCustomerById(Guid IdCustomer)
        {
            TemplateApi templateApi = await _customerRepository.GetCustomerById(IdCustomer);
            if (templateApi.Success) _logger.LogInformation("Thành công : {message}", templateApi.Message);
            else _logger.LogError("Xảy ra lỗi : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpPost: /api/Customer/InsertCustomer
        [HttpPost("InsertCustomer")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertCustomer([FromForm]CustomerRequest CustomerRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CustomerDto = CustomerRequest.Adapt<CustomerDto>();

            // define some col with data concrete
            CustomerDto.Id = Guid.NewGuid();
            CustomerDto.CreatedDate = DateTime.Now;
            CustomerDto.Status = 0;
            CustomerDto.IdUserCurrent = idUserCurrent;
            CustomerDto.Code = "KH - " + DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

            // If directory does not exist, create it. 
            if (!Directory.Exists(_appSettingModel.Root))
            {
                Directory.CreateDirectory(_appSettingModel.Root);
            }
            if (!Directory.Exists(_appSettingModel.ServerFileAvartar))
            {
                Directory.CreateDirectory(_appSettingModel.ServerFileAvartar);
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
                        // set file path to save file
                        var filename = Path.Combine(_appSettingModel.ServerFileAvartar, Path.GetFileName($"{CustomerDto.Id}.jpg"));

                        // save file
                        using (var stream = System.IO.File.Create(filename))
                        {
                            file.CopyTo(stream);
                        }
                        CustomerDto.Avatar = file.FileName;
                    }
                }
            }

            TemplateApi result = await _customerRepository.InsertCustomer(CustomerDto);

            _logger.LogInformation("Thành công : {message}", result.Message);
            return Ok(new
            {
                Success = result.Success,
                Fail = result.Fail,
                Message = result.Message
            });
        }
        // HttpPut: api/Customer/UpdateCustomer
        [HttpPut("UpdateCustomer")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateCustomer([FromForm]CustomerRequest CustomerRequest)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var CustomerDto = CustomerRequest.Adapt<CustomerDto>();
            CustomerDto.IdUserCurrent = idUserCurrent;

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
            if (CustomerRequest.idFile is null)
            {
                string IdFile = CustomerDto.Id.ToString() + ".jpg";

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
                        var filename = Path.Combine(_appSettingModel.ServerFileAvartar, Path.GetFileName($"{CustomerDto.Id}.jpg"));

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
                        CustomerDto.Avatar = file.FileName;
                    }
                }
            }

            TemplateApi result = await _customerRepository.UpdateCustomer(CustomerDto);
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
        // HttpDelete: /api/Customer/DeleteCustomer
        [HttpDelete("DeleteCustomer")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCustomer(Guid IdCustomer)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _customerRepository.DeleteCustomer(IdCustomer, idUserCurrent);

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
        // HttpDelete: /api/Customer/DeleteCustomerByList
        [HttpDelete("DeleteCustomerByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteCustomerByList(List<Guid> IdCustomer)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            TemplateApi result = await _customerRepository.DeleteCustomerByList(IdCustomer, idUserCurrent);

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
