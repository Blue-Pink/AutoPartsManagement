using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using APM.UtilEntities;

namespace APM.IBusiness
{
    public interface IUsualEntityService
    {
        public dynamic? Get(string entityName, Guid id);
        public string AutoNumber(string entityName, string prefix = "", int digit = 4);
        int Delete(string entityName, IEnumerable<Guid> ids);
        public object Edit(string entityName, JsonElement entity);
        public PagingData<object> GetChildrenDataSet(string parentEntityName,
            string childEntityName,
            Guid parentId,
            int pageIndex = 0,
            int pageSize = 10,
            string orderBy = "",
            bool descending = false);
        public PagingData<object> GetDataSet(string entityName,
            int pageIndex = 0,
            int pageSize = 10,
            string orderBy = "",
            bool descending = false);
    }
}
