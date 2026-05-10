using System;
using System.Collections.Generic;
using System.Text;

namespace APM.UtilEntities
{
    public class EntityFieldSetting(string name, bool filter, bool orderBy)
    {
        public string Name = name;
        public bool Filter = filter;
        public bool OrderByBy = orderBy;
    }
}
