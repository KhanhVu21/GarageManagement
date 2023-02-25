using GarageManagement.Middleware.Exceptions;
using GarageManagement.Services.Common.Model;
using GarageManagement.Utility;
using System.Net;
using System.Text.Json;
using KeyNotFoundException = GarageManagement.Middleware.Exceptions.KeyNotFoundException;
using NotImplementedException = GarageManagement.Middleware.Exceptions.NotImplementedException;
using UnauthorizedAccessException = GarageManagement.Middleware.Exceptions.UnauthorizedAccessException;

namespace GarageManagement.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = new TemplateApi
            {
                Success = false,
                Fail= true,
            };
            var exceptionType = exception.GetType();
            if (exceptionType == typeof(BadRequestException))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Đã xảy ra lỗi";
            }
            else if (exceptionType == typeof(NotFoundException))
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = "Đã xảy ra lỗi";
            }
            else if (exceptionType == typeof(NotImplementedException))
            {
                response.StatusCode = (int)HttpStatusCode.NotImplemented;
                errorResponse.Message = "Đã xảy ra lỗi";
            }
            else if (exceptionType == typeof(UnauthorizedAccessException))
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "Đã xảy ra lỗi";
            }
            else if (exceptionType == typeof(KeyNotFoundException))
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "Đã xảy ra lỗi";
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "Đã xảy ra lỗi";
            }

            _logger.LogError("Đã xảy ra lỗi - {exception}", exception.Message);
            var result = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(result);
        }
    }
}
