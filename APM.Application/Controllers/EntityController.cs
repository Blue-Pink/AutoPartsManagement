using System.Text.Json;
using APM.IBusiness;
using APM.IServices;
using APM.UtilEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APM.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityController(IUsualEntityService usualEntity) : APMController
    {
        [HttpGet, Route("[action]/{entityName}/{id}")]
        public dynamic Get(string entityName, Guid id)
        {
            if (id == Guid.Empty)
                throw new APMException($"{entityName} 中 {id} 是无效数据");

            return UsualResult(usualEntity.Get(entityName, id));
        }

        [HttpDelete, Route("[action]/{entityName}")]
        public UsualApiData<int> Delete(string entityName, IEnumerable<Guid> ids)
        {
            return UsualResult(usualEntity.Delete(entityName, ids));
        }

        [HttpGet, Route("[action]/{entityName}")]
        public UsualApiData<object> GetDataSet(string entityName,
            int pageIndex = 0,
            int pageSize = 10,
            string orderBy = "",
            bool descending = false)
        {
            return UsualResult(usualEntity.GetDataSet(entityName, pageIndex, pageSize, orderBy, descending));
        }

        [HttpGet, Route("[action]/{parentEntityName}/{childEntityName}/{parentId}")]
        public UsualApiData<object> GetChildrenDataSet(string parentEntityName,
            string childEntityName,
            Guid parentId,
            int pageIndex = 0,
            int pageSize = 10,
            string orderBy = "",
            bool descending = false)
        {
            return UsualResult(usualEntity.GetChildrenDataSet(parentEntityName, childEntityName, parentId, pageIndex, pageSize, orderBy, descending));
        }

        [HttpPost, Route("[action]/{entityName}")]
        public UsualApiData<object?> Edit(string entityName, [FromBody] JsonElement entity)
        {
            return UsualResult(usualEntity.Edit(entityName, entity));
        }

        [HttpGet, Route("[action]")]
        public UsualApiData<string?> AutoNumber(string entityName, string prefix = "", int digit = 4)
        {
            return UsualResult(usualEntity.AutoNumber(entityName, prefix, digit));
        }
    }
}
