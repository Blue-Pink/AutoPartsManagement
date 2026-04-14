using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace APM.DbEntities
{
    public static class EntityDriver
    {
        private static IDictionary<string, Type>? baseEntityChildren = null;
        public static IDictionary<string, Type> GetBascEntityChildren()
        {
            if (baseEntityChildren is null)
                baseEntityChildren = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(BaseEntity).IsAssignableFrom(t))
                    .ToDictionary(t => t.Name, t => t);
            return baseEntityChildren;
        }

        public static Type GetType(string entityName)
        {
            return GetBascEntityChildren().FirstOrDefault(kv => kv.Key.Equals(entityName, StringComparison.CurrentCultureIgnoreCase)).Value;
        }

    }
}
