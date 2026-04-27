using System;
using System.Collections.Generic;
using System.Text;
using APM.ConTaxi.Taxi;
using APM.IBusiness;

namespace APM.Business
{
    public class UsualEntityService(IConTaxiService taxi) : IUsualEntityService
    {
        public object? Get(string entityName, Guid id)
        {
            return taxi.Get(entityName, id);
        }
    }
}
