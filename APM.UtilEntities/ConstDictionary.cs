using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Text;

namespace APM.UtilEntities
{
    public class ConstDictionary
    {
        public const string Bearer = JwtBearerDefaults.AuthenticationScheme;
        public const string RedisCacheRolePermission = "RolePermission";
        public const string RedisCacheEntityRecord = "EntityRecord";
    }
}
