using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace APM.DbEntities
{
    public static class EntityDriver
    {
        private static IDictionary<string, Type>? _baseEntityChildren;
        public static IDictionary<string, Type> GetBasicEntityChildren()
        {
            _baseEntityChildren ??= Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(BaseEntity).IsAssignableFrom(t))
                .ToDictionary(t => t.Name, t => t);

            return _baseEntityChildren;
        }

        public static Type? GetType(string entityName)
        {
            GetBasicEntityChildren().TryGetValue(entityName,out var type);
            return type;
        }

    }
}
