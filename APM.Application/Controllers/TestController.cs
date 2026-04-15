using APM.ConTaxi.Taxi;
using APM.DbEntities;
using APM.IServices;
using APM.Services;
using APM.UtilEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APM.Application.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class TestController : APMController
    {
        private readonly IJsonWebTokenService _tokenService;
        private readonly IConTaxiService _taxi;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TestController(IJsonWebTokenService tokenService, IConTaxiService taxi, IHttpContextAccessor httpContextAccessor)
        {
            _tokenService = tokenService;
            _taxi = taxi;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet, Route("[action]"), AllowAnonymous]
        public string Test0()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            var userid = claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
            return userid.ToString();
            //return BCrypt.Net.BCrypt.HashPassword(@$"Administrator@01", BCrypt.Net.BCrypt.GenerateSalt());
        }

        [HttpGet, Route("[action]"), Authorize(AuthenticationSchemes = ConstDictionary.Bearer)]
        public UsualApiData<UserRole> Test1()
        {
            return UsualResult(_taxi.FirstOrDefault<UserRole>());
        }
    }
}
