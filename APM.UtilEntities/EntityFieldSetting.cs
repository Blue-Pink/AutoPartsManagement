using System;
using System.Collections.Generic;
using System.Text;

namespace APM.UtilEntities
{
    public class EntityFieldSetting(string fieldName, bool filter = false, bool orderBy = false, bool record = false)
    {
        public string FieldName = fieldName;
        public bool Filter = filter;
        public bool OrderByBy = orderBy;
        public bool Record = record;
    }
}
