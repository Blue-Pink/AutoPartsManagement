using APM.IServices;
using APM.UtilEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace APM.Extensions.Filter
{
    public class APMActionFilter(IJsonWebTokenService jwtService, ILogger<APMActionFilter> logger) : ActionFilterAttribute
    {
        /// <summary>
        /// 进入APMController时判断token是否有效,无token不处理
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var auth = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            //
            if (string.IsNullOrEmpty(auth) || !auth.StartsWith(ConstDictionary.Bearer))
                return;

            var token = auth.Substring(ConstDictionary.Bearer.Length).Trim();
            var state = jwtService.CheckUserToken(token);
            if (!state)    
            {
                logger?.Log(LogLevel.Information, "APMActionFilter token invalid or expired.");
                context.Result = new UnauthorizedResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}