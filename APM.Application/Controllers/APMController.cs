using APM.DbEntities;
using APM.UtilEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APM.Application.Controllers
{
    [Authorize(AuthenticationSchemes = ConstDictionary.Bearer)]
    public class APMController : ControllerBase
    {
        public UsualApiData<T> UsualResult<T>(T data, string? message = null, dynamic? customData = null)
        {
            return new UsualApiData<T>
            {
                Data = data,
                StateCode = UsualStateCode.Success,
                Message = message,
            };
        }

        public UsualApiData<T> UsualResult<T>(IEnumerable<T> data, string? message = null, dynamic? customData = null)
        {
            return new UsualApiData<T>
            {
                DataList = data,
                StateCode = UsualStateCode.Success,
                Message = message,
            };
        }
    }
}
