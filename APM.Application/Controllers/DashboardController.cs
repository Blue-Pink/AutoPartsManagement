using APM.DbEntities.DTOs;
using APM.IBusiness;
using APM.UtilEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APM.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController(IDashboardService dashboardService) : APMController
    {
        [HttpGet, Route("[action]")]
        public UsualApiData<DashboardDTO?> GetStockFlowDash()
        {
            return UsualResult(dashboardService.GetStockFlowDash());
        }
    }
}
