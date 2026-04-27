using APM.DbEntities;
using APM.DbEntities.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace APM.ConTaxi.Permission
{
    public interface ITaxiPermission
    {
        public void CheckPermission<T>(PermissionType type) where T : APMBaseEntity;
        public void CheckPermission<T>(EntityState state) where T : APMBaseEntity;
        public void CheckPermission<T>(List<EntityState> status) where T : APMBaseEntity;
        public void CheckPermission(string entityName, PermissionType type);
        public void CacheUserTokenRoles(string jwtoken, List<UserRole> roles);


    }
    public enum PermissionType
    {
        Create,
        Delete,
        Update,
        Read,
    }
}
