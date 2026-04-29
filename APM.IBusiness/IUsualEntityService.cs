using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace APM.IBusiness
{
    public interface IUsualEntityService
    {
        public dynamic? Get(string entityName, Guid id);
        public string AutoNumber(string entityName, string prefix, int digit);
        int Delete(string entityName, IEnumerable<Guid> ids);
        public List<object> GetChildrenDataSetQuery(string parentEntityName, string childEntityName, Guid parentId);
        public object Create(string entityName, JsonElement entity);
    }
}
