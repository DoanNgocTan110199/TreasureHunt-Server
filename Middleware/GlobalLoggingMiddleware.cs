using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Assignment.Middleware
{
    public class GlobalLoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public GlobalLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.next = next;
            logger = loggerFactory.CreateLogger<GlobalLoggingMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.ContentLength.HasValue)
                await next(context);

            var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
            await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            var requestBody = Encoding.UTF8.GetString(buffer);
            context.Request.Body.Seek(0, SeekOrigin.Begin);

            //Request String Builder
            //Bỏ qua log Swagger
            if (!context.Request.Path.ToString().Contains("swagger") && !context.Request.Path.ToString().Contains("hangfire"))
            {
                //METHOD POST thì lấy Body, Get thì lấy QueryString
                if (context.Request.Method.ToUpper() == "GET")
                {
                    requestBody = context.Request.QueryString.ToString();
                }

                var strLogRequest = $"REQUEST API: {context.Request.Path} --- METHOD: {context.Request.Method} --- CONTENT: {requestBody}";

                logger.LogInformation(strLogRequest);
            }

            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                //Handle Logging Exception
                try
                {
                    await next(context);

                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    var response = await new StreamReader(context.Response.Body).ReadToEndAsync();
                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    //Bỏ qua log Swagger
                    if (!context.Request.Path.ToString().Contains("swagger") && !context.Request.Path.ToString().Contains("hangfire"))
                    {
                        //Response String Builder
                        var strLogResponse = $"RESPONESE API: {context.Request.Path} --- METHOD: {context.Request.Method} --- CONTENT: {JsonConvert.SerializeObject(response)}";

                        logger.LogInformation(strLogResponse);
                    }

                    await responseBody.CopyToAsync(originalBodyStream);
                }
                catch (Exception ex)
                {
                    var strLogResponse = $"RESPONESE API: {context.Request.Path} --- METHOD: {context.Request.Method} --- EXCEPTION: {ex.ToString()}";
                    logger.LogError(strLogResponse);
                }
            }
        }
    }
}
