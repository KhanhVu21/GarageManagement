using GarageManagement.Attribute;
using GarageManagement.Controllers.Payload.User;
using GarageManagement.Controllers.Payload.User_Role;
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
using System.Security.Cryptography;
using System.Text;

namespace GarageManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        #region Variables
        private readonly IUnitRepository _unitRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UnitController> _logger;
        private readonly AppSettingModel _appSettingModel;
        #endregion

        #region Contructor
        public UserController(IUnitRepository unitRepository,
        IUserRepository userRepository,
        ILogger<UnitController> logger,
        IOptionsMonitor<AppSettingModel> optionsMonitor)
        {
            _unitRepository = unitRepository;
            _logger = logger;
            _userRepository = userRepository;
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

        #region tabel Role
        // GET: api/User/GetRoleByIdUserCurrent
        [HttpGet("GetRoleByIdUserCurrent")]
        public async Task<IActionResult> GetRoleByIdUserCurrent(Guid IdUser)
        {
            TemplateApi templateApi = await _userRepository.getRoleByIdUser(IdUser);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        #endregion

        #region GENARATE TOKEN
        private TokenModel GenerateToken(UserDto user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettingModel.SecretKey);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Fullname ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        private static string GenerateRefreshToken()
        {
            var random = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(random);
            return Convert.ToBase64String(random);
        }
        #endregion

        #region CRUD TABLE USER
        // GET: api/User/GetListUserAvailable
        [HttpGet("GetListUserAvailable")]
        public async Task<IActionResult> GetListUserAvailable(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _userRepository.getAllUserAvailable(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // HttpDelete: /api/User/RemoveUserByList
        [HttpDelete("RemoveUserByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> RemoveUserByList(List<Guid> IdUser)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var response = await _userRepository.RemoveUserByList(IdUser, idUserCurrent);

            if (response.Success)
            {
                _logger.LogInformation("Thành công : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
            else
            {
                _logger.LogError("Xảy ra lỗi : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
        }
        // HttpPut: /api/User/LockUserAccountByList
        [HttpPut]
        [Route("LockUserAccountByList")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> LockUserAccountByList(List<Guid> IdUser, bool isLock)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var response = await _userRepository.LockAccountUserByList(IdUser, isLock, idUserCurrent);

            if (response.Success)
            {
                _logger.LogInformation("Thành công : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
            else
            {
                _logger.LogError("Xảy ra lỗi : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
        }
        // GET: api/User/GetListUser
        [HttpGet("GetListUser")]
        public async Task<IActionResult> GetListUser(int pageNumber, int pageSize)
        {
            TemplateApi templateApi = await _userRepository.getAllUser(pageNumber, pageSize);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi);
        }
        // GET: api/User/GetUerById
        [HttpGet("GetUerById")]
        public async Task<IActionResult> GetUerById(Guid IdUser)
        {
            TemplateApi templateApi = await _userRepository.getUserById(IdUser);
            _logger.LogInformation("Thành công : {message}", templateApi.Message);
            return Ok(templateApi); ;
        }
        // HttpDelete: /api/User/RemoveUser
        [HttpDelete("RemoveUser")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> RemoveUser(Guid Id)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var response = await _userRepository.RemoveUser(Id, idUserCurrent);
            if (response.Success)
            {
                _logger.LogInformation("Thành công : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
            else
            {
                _logger.LogError("Xảy ra lỗi : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
        }
        // HttpPut: /api/User/LockUserAccount
        [HttpPut]
        [Route("LockUserAccount")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> LockUserAccount(Guid Id, bool isLock)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            var response = await _userRepository.LockAccountUser(Id, isLock, idUserCurrent);
            if (response.Success)
            {
                _logger.LogInformation("Thành công : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
            else
            {
                _logger.LogError("Xảy ra lỗi : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
        }
        // HttpPut: /api/User/UpdateUser
        [HttpPut]
        [Route("UpdateUser")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> UpdateUser([FromForm] UserRequest userModel)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            //check account deleted
            UserDto userById = _userRepository.getUserByID(userModel.Id ?? Guid.Empty);
            if (userById != null && userById.IsDeleted) return Ok(new { message = "Tài khoản này đã bị xóa !", Success = false });

            UserDto userDTO = userModel.Adapt<UserDto>();
            if (userModel.Fullname == "null")
            {
                userDTO.Fullname = null;
            }
            if (userModel.Description == "null")
            {
                userDTO.Description = null;
            }
            if (userModel.Phone == "null")
            {
                userDTO.Phone = null;
            }
            if (userModel.Address == "null")
            {
                userDTO.Address = null;
            }

            // If directory does not exist, create it. 
            if (!Directory.Exists(_appSettingModel.Root))
            {
                Directory.CreateDirectory(_appSettingModel.Root);
            }
            if (!Directory.Exists(_appSettingModel.ServerFileAvartar))
            {
                Directory.CreateDirectory(_appSettingModel.ServerFileAvartar);
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
                        string IdFile = userDTO.Id.ToString() + ".jpg";

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
                        userDTO.Avatar = IdFile;
                    }
                }
            }

            userDTO.IdUserCurrent = idUserCurrent;
            TemplateApi response = await _userRepository.UpdateUser(userDTO);

            if (response.Success)
            {
                _logger.LogInformation("Thành công : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
            else
            {
                _logger.LogError("Xảy ra lỗi : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
        }
        // HttpPost: /api/User/InsertUser
        [HttpPost]
        [Route("InsertUser")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> InsertUser([FromForm] UserRequest userModel)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            //check exits email
            UserDto userByEmail = _userRepository.getUserByEmail(userModel.Email);
            if (userByEmail.Id != Guid.Empty) return Ok(new { message = "Email đã tồn tại !", Success = false });

            UserDto userDTO = userModel.Adapt<UserDto>();

            // define some col with data concrete
            userDTO.Id = Guid.NewGuid();
            userDTO.Status = userModel.Status.HasValue ? userModel.Status : 0;
            userDTO.CreatedDate = DateTime.Now;
            userDTO.IsLocked = false;
            userDTO.IsDeleted = false;
            userDTO.IsActive = userModel.IsActive;
            userDTO.ActiveCode = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            userDTO.UserCode = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            userDTO.CreatedBy = idUserCurrent;
            userDTO.Password = BCrypt.Net.BCrypt.HashPassword(userModel.Password + _appSettingModel.SecretKey);
            userDTO.IdUserCurrent = idUserCurrent;

            if (userModel.Fullname == "null")
            {
                userDTO.Fullname = null;
            }
            if (userModel.Description == "null")
            {
                userDTO.Description = null;
            }
            if (userModel.Phone == "null")
            {
                userDTO.Phone = null;
            }
            if (userModel.Address == "null")
            {
                userDTO.Address = null;
            }

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

                        // prepare path to save file image
                        string pathTo = _appSettingModel.ServerFileAvartar;
                        // get extention form file name
                        string IdFile = userDTO.Id.ToString() + ".jpg";

                        // set file path to save file
                        var filename = Path.Combine(pathTo, Path.GetFileName(IdFile));

                        // save file
                        using (var stream = System.IO.File.Create(filename))
                        {
                            file.CopyTo(stream);
                        }
                        userDTO.Avatar = IdFile;
                    }
                }
            }

            //save to table user
            TemplateApi response = await _userRepository.InsertUserByAdmin(userDTO);

            if (response.Success)
            {
                _logger.LogInformation("Thành công : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
            else
            {
                _logger.LogError("Xảy ra lỗi : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
        }
        // HttpPost: /api/User/AddRoleUser
        [HttpPost]
        [Route("AddRoleUser")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> AddRoleUser(User_RoleRequest user_RoleModel)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            //cannot save one role many time
            List<User_RoleDto> user_RoleDto = _userRepository.getListRoleOfUser(user_RoleModel.IdUser);
            for (int i = 0; i < user_RoleDto.Count; i++)
            {
                if (user_RoleDto[i].IdRole == user_RoleModel.IdRole && user_RoleDto[i].IdUser == user_RoleModel.IdUser)
                {
                    return Ok(new
                    {
                        Message = "Role này đã được thêm cho tài khoản",
                        Fail = true,
                        Success = false
                    });
                }
            }

            //save to table user_role
            TemplateApi response = await _userRepository.InsertUser_Role(user_RoleModel.IdRole, user_RoleModel.IdUser, idUserCurrent);

            if (response.Success)
            {
                _logger.LogInformation("Thành công : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
            else
            {
                _logger.LogError("Xảy ra lỗi : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
        }
        // HttpDelete: /api/User/DeleteUserRole
        [HttpDelete]
        [Route("DeleteUserRole")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> DeleteUserRole(User_RoleRequest user_RoleModel)
        {
            //get id user current login
            var idUserCurrent = (Guid)Request.HttpContext.Items["User"]!;

            //delete to table user_role
            TemplateApi response = await _userRepository.DeleteUser_Role(user_RoleModel.IdUser, user_RoleModel.IdRole, idUserCurrent);

            if (response.Success)
            {
                _logger.LogInformation("Thành công : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
            else
            {
                _logger.LogError("Xảy ra lỗi : {message}", response.Message);
                return Ok(new
                {
                    Success = response.Success,
                    Fail = response.Fail,
                    Message = response.Message
                });
            }
        }
        #endregion

        #region METHOD USER
        // HttpPost: /api/User/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegistRequest userRegistRequest)
        {
            //check email exist
            UserDto userByEmail = _userRepository.getUserByEmail(userRegistRequest.Email);

            // get id Unit and user Type
            UnitDto unit = await _unitRepository.GetUnitByUnitCode(_appSettingModel.Unit);
            UserTypeDto userType = _userRepository.getTypeUser(_appSettingModel.UserTypeDefault);

            if (userByEmail?.Id != Guid.Empty && userByEmail?.IsActive == true)
                return Ok(new { message = "Tài khoản này đã được kích hoạt", Success = false, Fail = true });

            if (userByEmail?.Id != Guid.Empty)
                return Ok(new { message = "Tài khoản này chưa được kích hoạt", Success = false, Fail = true });

            string activeCode = "";

            if (userByEmail?.Id == Guid.Empty)
            {
                //save user
                var user = new UserDto()
                {
                    Id = Guid.NewGuid(),
                    Fullname = userRegistRequest?.Fullname,
                    Email = userRegistRequest is not null ? userRegistRequest.Email : "",
                    Password = BCrypt.Net.BCrypt.HashPassword(userRegistRequest?.Password + _appSettingModel.SecretKey),
                    CreatedDate = DateTime.Now,
                    IsDeleted = false,
                    IsLocked = false,
                    IsActive = false,
                    ActiveCode = DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                    UserCode = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
                    UserTypeId = userType.Id,
                    UnitId = unit.Id,
                    Address = userRegistRequest?.Address,
                    Phone = userRegistRequest?.Phone,
                };
                user.IdUserCurrent = user.Id;
                activeCode = user.ActiveCode;
                await _userRepository.InsertUser(user);
            }

            // prepare content for mail
            string fromMail = _appSettingModel.FromMail;
            string password = _appSettingModel.Password;
            string Subject = "Vui lòng xác thực tài khoản !";
            string body =
            $"<div style='max-width: 700px; margin: auto; border: 10px solid #ddd; padding: 50px 20px; font-size: 110%;'>" +
            $"<h2 style='text-align: center; text-transform: uppercase;color: teal;'> Chào mừng bạn đến với ứng dụng quản lí bán hàng </h2>" +
            $"<p>Chúc mừng bạn đã đăng kí thành công tài khoản. Hãy lấy mã và kích hoạt tài khoản của bạn !</p>" +
            $"<a style='background: crimson; text-decoration: none; color: white; padding: 10px 20px; margin-left: 300px; display: inline-block;text-align:center'>{activeCode}</a>";

            //send mail confirm
            var sendMail = new SendMail();
            sendMail.SendMailAuto(fromMail, userRegistRequest is not null ? userRegistRequest.Password : "", password, body, Subject);

            return Ok(new
            {
                message = "Kiểm tra mail của bạn và kích hoạt tài khoản !",
                Success = true,
                Fail = false
            });
        }
        // HttpPost: /api/User/Login
        [HttpPost("Login")]
        public IActionResult Login(UserLoginRequest userLoginRequest)
        {
            // get one user by email
            UserDto userByEmail = _userRepository.getUserByEmail(userLoginRequest.Email);

            if (userByEmail?.Id != Guid.Empty && userByEmail!.IsLocked)
            {
                return Ok(new { message = "Tài khoản này đã bị khóa !", Success = false, Fail = true });
            }

            if (userByEmail?.Id == Guid.Empty) return Ok(new { message = "Tài khoản không tồn tại !", Success = false, Fail = true });

            if (userByEmail?.IsActive == false) return Ok(new { message = "Tài khoản này chưa được kich hoạt !", Success = false, Fail = true });

            if (!BCrypt.Net.BCrypt.Verify(userLoginRequest.Password + _appSettingModel.SecretKey, userByEmail?.Password))
            {
                return Ok(new { message = "Mật khẩu không chính xác", Success = false, Fail = true });
            }

            // get list id in table user table
            List<User_RoleDto> user_Role_List = _userRepository.getListRoleOfUser(userByEmail!.Id);
            var customApiRoleOfUsers = new List<CustomApiRoleOfUser>();

            for (int i = 0; i < user_Role_List.Count; i++)
            {
                RoleDto roleOfUser = _userRepository.getUserRolebyId(user_Role_List[i].IdRole);
                var customApiRoleOfUser = new CustomApiRoleOfUser()
                {
                    IdRole = roleOfUser.Id,
                    NameRole = roleOfUser.RoleName,
                    IdUser = user_Role_List[i].IdUser,
                    IsAdmin = roleOfUser.IsAdmin
                };

                customApiRoleOfUsers.Add(customApiRoleOfUser);
            }

            //cấp token
            var token = GenerateToken(userByEmail);

            Response.Cookies.Append("jwt", token.AccessToken, new CookieOptions
            {
                HttpOnly = true
            });

            return Ok(new UserRespons
            {
                Id = userByEmail.Id,
                Success = true,
                Message = "Xác thực thành công !",
                Data = token,
                RoleList = customApiRoleOfUsers,
                IsAdmin = customApiRoleOfUsers[0].IsAdmin
            });
        }
        // HttpPost: /api/User/Logout
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new
            {
                message = "Đăng xuất thành công !",
                Success = false,
                Fail = true
            });
        }
        // GET: api/User/GetUser
        [HttpGet("GetUser")]
        public IActionResult GetUser()
        {
            var jwt = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (jwt is null)
            {
                return Ok(new { message = "Không tìm thấy token !", Success = false, Fail = true });
            }

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? tokenS = handler.ReadToken(jwt) as JwtSecurityToken;
            string profile = tokenS!.Claims.First(claim => claim.Type == "email").Value;
            string expire = tokenS.Claims.First(claim => claim.Type == "exp").Value;


            double doubleVal = Convert.ToDouble(expire);
            DateTime DateAfterConvert = UnixTimeStampToDateTime(doubleVal);

            if (DateAfterConvert < DateTime.Now)
            {
                return Ok(new
                {
                    Message = "Token đã hết hạn !",
                    Success = false,
                    Fail = true
                });
            }

            UserDto user = _userRepository.getUserByEmail(profile);
            if (user != null && user.IsLocked)
            {
                return Ok(new
                {
                    Message = "Tài khoản đã bị khóa !",
                    Success = false,
                    Fail = true
                });
            }

            User_RoleDto user_Role = _userRepository.getRoleOfUser(user!.Id);
            RoleDto role = _userRepository.getUserRolebyId(user_Role.IdRole);

            // get list id in table user table
            List<User_RoleDto> user_Role_List = _userRepository.getListRoleOfUser(user.Id);
            var customApiRoleOfUsers = new List<CustomApiRoleOfUser>();

            for (int i = 0; i < user_Role_List.Count; i++)
            {
                RoleDto roleOfUser = _userRepository.getUserRolebyId(user_Role_List[i].IdRole);
                var customApiRoleOfUser = new CustomApiRoleOfUser()
                {
                    IdRole = roleOfUser.Id,
                    NameRole = roleOfUser.RoleName,
                    IdUser = user_Role_List[i].IdUser,
                    IsAdmin = roleOfUser.IsAdmin
                };
                customApiRoleOfUsers.Add(customApiRoleOfUser);
            }

            return Ok(new
            {
                Data = user,
                role.RoleName,
                listRole = customApiRoleOfUsers,
                Success = true,
                customApiRoleOfUsers[0].IsAdmin
            });
        }
        // HttpPut: /api/User/ActiveUserByCode
        [HttpPut("ActiveUserByCode")]
        public async Task<IActionResult> ActiveUserByCode(string email, string code)
        {
            UserDto userByEmail = _userRepository.getUserByEmail(email);

            if (userByEmail.Id == Guid.Empty) return Ok(new { message = "Tài khoản này chưa tồn tại", Success = false, Fail = true });

            if (userByEmail.Id != Guid.Empty && userByEmail.IsActive == true)
                return Ok(new { message = "Tài khoản này đã được kích hoạt", Success = false });

            if (userByEmail?.ActiveCode != code) return Ok(new { message = "Vui lòng nhập đúng mã code", Success = false, Fail = true });

            TemplateApi status = await _userRepository.ActiveUserByCode(email, code);

            if (status.Success)
            {
                return Ok(new
                {
                    message = "Tài khoản của bạn đã được kích hoạt !",
                    Success = true,
                    Fail = true
                });
            }
            else
            {
                return Ok(new { message = "Kích hoạt không thành công !", Success = false, Fail = true });
            }
        }
        // HttpPut: /api/User/SendAgainCode
        [HttpPut("SendAgainCode")]
        public IActionResult SendAgainCode(string email)
        {
            UserDto userByEmail = _userRepository.getUserByEmail(email);

            if (userByEmail == null) return Ok(new { message = "Tài khoản này chưa tồn tại", Success = false, Fail = true });

            if (userByEmail != null && userByEmail.IsActive == true)
                return Ok(new { message = "Tài khoản này đã được kích hoạt", Success = false, Fail = true });

            string CodeActive = userByEmail!.ActiveCode;
            string fromMail = _appSettingModel.FromMail;
            string password = _appSettingModel.Password;
            string Subject = "Vui lòng xác thực tài khoản !";
            string body =
            $"<div style='max-width: 700px; margin: auto; border: 10px solid #ddd; padding: 50px 20px; font-size: 110%;'>" +
            $"<h2 style='text-align: center; text-transform: uppercase;color: teal;'> Chào mừng bạn đến với kỉ yếu tỉnh ủy </h2>" +
            $"<p>Chúc mừng bạn đã đăng kí thành công tài khoản. Hãy lấy mã và kích hoạt tài khoản của bạn !</p>" +
            $"<a style='background: crimson; text-decoration: none; color: white; padding: 10px 20px; margin-left: 300px; display: inline-block;text-align:center'>{CodeActive}</a>";

            //send mail confirm
            var sendMail = new SendMail();
            sendMail.SendMailAuto(fromMail, email, password, body, Subject);

            return Ok(new
            {
                message = "Kiểm tra mail của bạn !",
                Success = true,
                Fail = true
            });
        }
        // HttpPut: /api/User/SendCodeWithAccountActive
        [HttpPut("SendCodeWithAccountActive")]
        public async Task<IActionResult> SendCodeWithAccountActive(string email)
        {
            UserDto userByEmail = _userRepository.getUserByEmail(email);

            if (userByEmail == null) return Ok(new { message = "Tài khoản này chưa tồn tại", Success = false, Fail = true });

            if (userByEmail != null && userByEmail.IsActive == true)
            {
                var CodeActive = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
                TemplateApi resuilt = await _userRepository.UpdateActiveCode(CodeActive, userByEmail.Email);

                if (resuilt.Success)
                {
                    string fromMail = _appSettingModel.FromMail;
                    string password = _appSettingModel.Password;
                    string Subject = "Vui lòng xác thực tài khoản !";
                    string body =
                    $"<div style='max-width: 700px; margin: auto; border: 10px solid #ddd; padding: 50px 20px; font-size: 110%;'>" +
                    $"<h2 style='text-align: center; text-transform: uppercase;color: teal;'> Chào mừng bạn đến với kỉ yếu tỉnh ủy </h2>" +
                    $"<p>Chúc mừng bạn đã lấy thành công mã kich hoạt tài khoản. Hãy lấy mã và kích hoạt tài khoản của bạn !</p>" +
                    $"<a style='background: crimson; text-decoration: none; color: white; padding: 10px 20px; margin-left: 300px; display: inline-block;text-align:center'>{CodeActive}</a>";

                    //send mail confirm
                    var sendMail = new SendMail();
                    sendMail.SendMailAuto(fromMail, email, password, body, Subject);

                    return Ok(new
                    {
                        message = "Kiểm tra mail của bạn !",
                        Success = true,
                        Fail = true
                    });
                }
                else
                {
                    return Ok(new
                    {
                        message = "Không thể tạo mã code !",
                        Success = false,
                        Fail = true
                    });
                }

            }
            else
            {
                return Ok(new
                {
                    message = "Tài khoản của bạn chưa được kích hoạt hoặc gmail không chính xác !",
                    Success = false,
                    Fail = true
                });
            }
        }
        // HttpPost: /api/User/VerifyCode
        [HttpPost("VerifyCode")]
        public IActionResult VerifyCode(string code, string email)
        {
            UserDto userByEmail = _userRepository.getUserByEmail(email);
            if (userByEmail == null) return Ok(new { message = "Tài khoản này chưa tồn tại", Success = false, Fail = true });

            if (userByEmail != null && userByEmail.IsActive == true && userByEmail.ActiveCode == code)
            {
                return Ok(new
                {
                    message = "Xác thực thành công !",
                    Success = true,
                    Fail = true
                });
            }
            else
            {
                return Ok(new
                {
                    message = "Vui lòng nhập chính xác thông tin !",
                    Success = false,
                    Fail = true
                });
            }
        }
        // HttpPut: /api/User/ForgotPassWord
        [HttpPut("ForgotPassWord")]
        public async Task<IActionResult> ForgotPassWord(string email, string newPassword)
        {
            UserDto userByEmail = _userRepository.getUserByEmail(email);

            if (userByEmail == null) return Ok(new { message = "Tài khoản này chưa tồn tại", Success = false, Fail = true });
            if (userByEmail != null && userByEmail.IsActive == false) return Ok(new { message = "Tài khoản này chưa được kich hoạt", Success = false, Fail = true });

            TemplateApi resuilt = await _userRepository.UpdatePassword(email, BCrypt.Net.BCrypt.HashPassword(newPassword + _appSettingModel.SecretKey));

            if (resuilt.Success)
            {
                return Ok(new
                {
                    message = resuilt.Message,
                    Success = true,
                    Fail = false
                });
            }
            else
            {
                return Ok(new
                {
                    message = resuilt.Message,
                    Success = false,
                    Fail = true
                });
            }
        }
        // HttpPut: /api/User/ChangePassWord
        [HttpPut("ChangePassWord")]
        public async Task<IActionResult> ChangePassWord(string email, string oldPassword, string newPassword)
        {
            UserDto userByEmail = _userRepository.getUserByEmail(email);

            if (userByEmail == null) return Ok(new { message = "Tài khoản này chưa tồn tại", Success = false, Fail = true });
            if (userByEmail != null && userByEmail.IsActive == false) return Ok(new { message = "Tài khoản này chưa được kich hoạt", Success = false, Fail = true });
            if (!BCrypt.Net.BCrypt.Verify(oldPassword + _appSettingModel.SecretKey, userByEmail?.Password))
                return Ok(new { message = "Mật khẩu cũ không chính xác", Success = false, Fail = true });

            TemplateApi resuilt = await _userRepository.UpdatePassword(email, BCrypt.Net.BCrypt.HashPassword(newPassword + _appSettingModel.SecretKey));

            if (resuilt.Success)
            {
                return Ok(new
                {
                    message = resuilt.Message,
                    Success = true,
                    Fail = false
                });
            }
            else
            {
                return Ok(new
                {
                    message = resuilt.Message,
                    Success = false,
                    Fail = true
                });
            }
        }
        #endregion

    }
}
