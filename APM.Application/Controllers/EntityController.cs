using APM.IBusiness;
using APM.UtilEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APM.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityController(IUsualEntityService entityService) : APMController
    {
        [HttpGet, Route("[action]/{entityName}/{id}")]
        public dynamic Get(string entityName, Guid id)
        {
            if (id == Guid.Empty)
                throw new APMException($"{entityName} 中 {id} 是无效数据");

            return UsualResult(entityService.Get(entityName, id));
        }
    }
}
