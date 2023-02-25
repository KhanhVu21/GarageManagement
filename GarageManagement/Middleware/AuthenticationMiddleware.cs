using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using GarageManagement.Utility;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GarageManagement.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;
        private readonly IUserRepository _userRepository;
        private readonly AppSettingModel _appSettingModel;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger, IUserRepository userRepository,
        IOptionsMonitor<AppSettingModel> optionsMonitor)
        {
            _next = next;
            _logger = logger;
            _userRepository = userRepository;
            _appSettingModel = optionsMonitor.CurrentValue;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                var jwt = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var userEmail = ValidateToken(jwt ?? "");

                //get user from database by email
                var user = _userRepository.getUserByEmail(userEmail ?? "");
                if (user.Id != Guid.Empty)
                {
                    // attach user to context on successful jwt validation
                    httpContext.Items["User"] = user.Id;
                }
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        private string? ValidateToken(string token)
        {
            if (token == "")
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettingModel.SecretKey);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userEmail = jwtToken.Claims.First(x => x.Type == "email").Value;

                // return user id from JWT token if validation successful
                return userEmail;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;
            response.StatusCode = (int)HttpStatusCode.BadRequest;

            var errorResponse = new TemplateApi
            {
                Success = false,
                Fail = true,
                Message = "Đã xảy ra lỗi khi xác thực"
            };

            _logger.LogError("Đã xảy ra lỗi - {exception}", exception.Message);
            var result = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(result);
        }
    }
}
