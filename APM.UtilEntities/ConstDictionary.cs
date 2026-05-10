using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using APM.DbEntities;

namespace APM.UtilEntities
{
    public static class ConstDictionary
    {
        public const string Bearer = JwtBearerDefaults.AuthenticationScheme;
        public const string RedisCacheRolePermission = "RolePermission";
        public const string RedisChannelPrefix = "Channel";
        public const string RedisChannelMessage = "Message";
        public const string RedisCacheEntityRecord = "EntityRecord";
        public const string JwtClaimsUserId = "JwtClaimsUserId";
        public const string JwtClaimsRoleIds = "JwtClaimsRoleIds";
        public const string AdministratorId = "F1A89D52-1C0F-4070-A6DD-761A04FCF7F4";

        public static readonly Dictionary<Type, ICollection<EntityFieldSetting>> EntityFieldSettings = new()
        {
            {typeof(EntityRecord), new List<EntityFieldSetting>{new(nameof(EntityRecord.EntityName),true,true)}},
            {typeof(RolePermission), new List<EntityFieldSetting>{new(nameof(RolePermission.RoleId),true,true)}}
        };
    }
}
