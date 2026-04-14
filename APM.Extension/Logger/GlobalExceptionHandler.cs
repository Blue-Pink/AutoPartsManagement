using APM.UtilEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace APM.Extensions.Logger
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public GlobalExceptionHandler(RequestDelegate next, ILoggerFactory loggerFactory, IHostEnvironment hostEnvironment)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<GlobalExceptionHandler>();
            _hostEnvironment = hostEnvironment;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (APMException ex)
            {
                var body = JsonSerializer.Serialize(new UsualApiData<dynamic>()
                {
                    StateCode = UsualStateCode.Error,
                    Message = ex.Message,
                },
                    new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    });

                await context.Response.WriteAsync(body);
            }
            catch (Exception ex)
            {
                context.Response.Headers["content-type"] = "application/json; charset=utf-8";
                context.Response.Headers["server"] = "Kestrel";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                if (_hostEnvironment.IsDevelopment())
                {
                    var body = JsonSerializer.Serialize(new UsualApiData<dynamic>()
                    {
                        StateCode = UsualStateCode.Error,
                        Message = ex.Message,
                        CustomData = ex.ToString()
                    },
                    new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    });

                    await context.Response.WriteAsync(body);
                }
                else
                {
                    var body = JsonSerializer.Serialize(new UsualApiData<dynamic>()
                    {
                        StateCode = UsualStateCode.Error,
                        Message = $"遇到了一点问题，请联系系统管理员。",
                    },
                    new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    });

                    await context.Response.WriteAsync(body);
                }

                _logger.LogError(ex, " GlobalExceptionHandler ");
            }
        }
    }
}