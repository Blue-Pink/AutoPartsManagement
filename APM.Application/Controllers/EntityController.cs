using APM.ConTaxi.Taxi;
using Microsoft.AspNetCore.Mvc;

namespace APM.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityController : APMController
    {
        private readonly IConTaxiService _taxi;

        public EntityController(IConTaxiService taxi)
        {
            _taxi = taxi;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [HttpGet, Route("{entityName}/Get/{id}")]
        public dynamic Get(string entityName, Guid id)
        {
            var entity = _taxi.Get(entityName, id);
            return UsualResult(entity);
        }

    }
}
