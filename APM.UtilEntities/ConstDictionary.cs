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
        public const string JwtClaimsUserId = "JwtClaimsUserId";
        public const string JwtClaimsRoleIds = "JwtClaimsRoleIds";
        public const string AdministratorId = "F1A89D52-1C0F-4070-A6DD-761A04FCF7F4";
    }
}
