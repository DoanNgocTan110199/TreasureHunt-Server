using Assignment.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Assignment.Middleware
{
    public class RequestLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly int _limit;

        public RequestLimitMiddleware(RequestDelegate next, int limit)
        {
            _next = next;
            _limit = limit;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            //Áp dụng với API get Authen lấy Token
            if (context.Request.Path.ToString().Contains("/api/Users/Authenticate"))
            {
                var ip = context.Connection.RemoteIpAddress.ToString();

                var memoryCache = context.RequestServices.GetRequiredService<IMemoryCache>();
                var cacheKey = $"{ip}_requestCount";
                var requestCount = memoryCache.Get(cacheKey) as int? ?? 0;

                if (requestCount >= _limit)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;

                    var responseModel = new ResponseModel();
                    responseModel.Success = false;
                    responseModel.Code = "01";
                    responseModel.Message = $"Bạn đã vượt quá giới hạn {_limit} lần gửi yêu cầu trong 1 phút.";

                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 429;

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(responseModel));
                    return;
                }

                requestCount++;
                memoryCache.Set(cacheKey, requestCount, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                });
            }

            await _next(context);

        }
    }
}
