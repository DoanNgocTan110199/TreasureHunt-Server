using Assignment.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Assignment.Middleware
{
    public class GlobalHandlerExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IHostEnvironment _env;
        public GlobalHandlerExceptionMiddleware(RequestDelegate next, ILogger<GlobalHandlerExceptionMiddleware> logger, IHostEnvironment env)
        {
            _logger = logger;
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (AccessViolationException avEx) //Exception chủ động raise lên để gửi message custom
            {
                _logger.LogError(avEx.ToString());
                await HandleExceptionDevelopmentAsync(httpContext, avEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                if (_env.IsDevelopment())
                    await HandleExceptionDevelopmentAsync(httpContext, ex);
                else
                    await HandleExceptionProductionAsync(httpContext, ex);
            }
        }

        //Xử lý trả về lỗi chung ở Production
        private static async Task HandleExceptionProductionAsync(HttpContext context, Exception exception)
        {
            var responseModel = new ResponseModel
            {
                Success = false,
                Code = "01",
                Message = "ERROR: Lỗi hệ thống"
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var responseStr = JsonConvert.SerializeObject(responseModel);

            await context.Response.WriteAsync(responseStr);
        }

        //Xử lý trả về Exception ở Development
        private static async Task HandleExceptionDevelopmentAsync(HttpContext context, Exception exception)
        {
            var responseModel = new ResponseModel
            {
                Success = false,
                Code = "99",
                Message = exception.Message.ToString()
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var responseStr = JsonConvert.SerializeObject(responseModel);

            await context.Response.WriteAsync(responseStr);
        }
    }
}
