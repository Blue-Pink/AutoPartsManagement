using System;
using System.Collections.Generic;
using System.Text;

namespace APM.IBusiness
{
    public interface IUsualEntityService
    {
        public dynamic? Get(string entityName, Guid id);
        public string AutoNumber(string entityName, string prefix, int digit);
        int Delete(string entityName, IEnumerable<Guid> ids);
    }
}
