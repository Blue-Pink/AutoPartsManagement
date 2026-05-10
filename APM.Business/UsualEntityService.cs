using APM.ConTaxi.Taxi;
using APM.IBusiness;
using APM.IServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using APM.UtilEntities;

namespace APM.Business
{
    public class UsualEntityService(IConTaxiService taxi, IRedisService redis) : IUsualEntityService
    {
        public object? Get(string entityName, Guid id)
        {
            return taxi.Get(entityName, id);
        }

        public string AutoNumber(string entityName, string prefix = "", int digit = 4)
        {
            return redis.AutoNumber(entityName, prefix, digit);
        }

        public int Delete(string entityName, IEnumerable<Guid> ids)
        {
            return !ids.Any() ? 0 : taxi.Delete(entityName, ids);
        }

        public PagingData<object> GetChildrenDataSet(string parentEntityName,
            string childEntityName,
            Guid parentId,
            int pageIndex = 0,
            int pageSize = 10,
            string orderBy = "",
            bool descending = false,
            string filter = "",
            int depth = 1)
        {
            return taxi.GetChildrenDataSet(parentEntityName, childEntityName, parentId, pageIndex, pageSize, orderBy, descending, filter, depth);
        }

        public PagingData<object> GetDataSet(string entityName,
            int pageIndex = 0,
            int pageSize = 10,
            string orderBy = "",
            bool descending = false,
            string filter = "",
            int depth = 1)
        {
            return taxi.GetDataSet(entityName, pageIndex, pageSize, orderBy, descending, filter, depth);
        }

        public object Edit(string entityName, JsonElement entity)
        {
            return taxi.Edit(entityName, entity);
        }
    }
}
