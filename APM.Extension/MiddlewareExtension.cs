using APM.Extensions.Logger;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace APM.Extensions
{
    public static class MiddlewareExtension
    {
        #region Logger Extension
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandler>();
        }

        public static ILoggingBuilder AddLogger(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, LoggerProvider>();
            return builder;
        }
        #endregion

    }
}
