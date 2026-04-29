using APM.ConTaxi.Taxi;
using APM.IBusiness;
using APM.IServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace APM.Business
{
    public class UsualEntityService(IConTaxiService taxi, IRedisService redis) : IUsualEntityService
    {
        public object? Get(string entityName, Guid id)
        {
            return taxi.Get(entityName, id);
        }

        public string AutoNumber(string entityName, string prefix, int digit)
        {
            return redis.AutoNumber(entityName, prefix, digit);
        }

        public int Delete(string entityName, IEnumerable<Guid> ids)
        {
            return !ids.Any() ? 0 : taxi.Delete(entityName, ids);
        }
    }
}
