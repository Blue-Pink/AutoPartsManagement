using APM.DbEntities;
using APM.DbEntities.Base;
using APM.IServices;
using APM.UtilEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace APM.ConTaxi.Permission
{
    internal class TaxiPermission(IRedisService redis, IHttpContextAccessor httpContextAccessor)
        : ITaxiPermission
    {
        public void CheckPermission<T>(PermissionType type) where T : APMBaseEntity
        {
            var entityName = typeof(T).FullName;
            if (string.IsNullOrEmpty(entityName))
                throw new APMException($"异常数据: {entityName}");
            CheckPermission(entityName, type);
        }

        public void CheckPermission<T>(EntityState state) where T : APMBaseEntity
        {
            PermissionType permissionType;
            switch (state)
            {
                case EntityState.Deleted:
                    permissionType = PermissionType.Delete;
                    break;
                case EntityState.Modified:
                    permissionType = PermissionType.Update;
                    break;
                case EntityState.Added:
                    permissionType = PermissionType.Create;
                    break;
                default:
                    throw new APMException("Only can do Deleted/Modified/Added.");

            }
            CheckPermission<T>(permissionType);
        }

        public void CheckPermission<T>(List<EntityState> status) where T : APMBaseEntity
        {
            foreach (var state in status)
            {
                CheckPermission<T>(state);
            }
        }

        public void CheckPermission(string entityName, PermissionType type)
        {
            if (string.IsNullOrEmpty(entityName))
                throw new APMException("缺少实体的名称");

            var roleIds = GetCurrentUserRoles();
            var permissions = new List<RolePermission>();
            foreach (var roleId in roleIds)
            {
                permissions.AddRange(redis.GetList<RolePermission>($"{ConstDictionary.RedisCacheRolePermission}:{roleId}") ?? permissions);
            }
            var entityRecords = redis.GetList<EntityRecord>(ConstDictionary.RedisCacheEntityRecord);
            var assembly = typeof(APMBaseEntity)?.FullName?.Replace(nameof(APMBaseEntity), "")?.Replace("Base", "").Replace("..", ".") ?? "";

            if (!string.IsNullOrEmpty(assembly) && permissions.Any() && entityRecords != null && entityRecords.Any() && roleIds.Any())
            {
                if (!entityName.Contains(assembly))
                    entityName = assembly + entityName;

                var currentEntityRecord = entityRecords?.FirstOrDefault(er => er?.FullName?.Equals(entityName) ?? false);
                if (currentEntityRecord != null)
                {
                    var rolePermissions = permissions?.Where(p => p.EntityId == currentEntityRecord.Id && roleIds.Contains(p.RoleId)).ToList();
                    var checkResult = rolePermissions?.Where(rp =>
                    {
                        switch (type)
                        {
                            case PermissionType.Create:
                                if (rp.CanCreate)
                                    return true;
                                break;
                            case PermissionType.Delete:
                                if (rp.CanDelete)
                                    return true;
                                break;
                            case PermissionType.Update:
                                if (rp.CanUpdate)
                                    return true;
                                break;
                            case PermissionType.Read:
                                if (rp.CanRead)
                                    return true;
                                break;
                        }
                        return false;
                    });

                    if (checkResult is null || !checkResult.Any())
                        throw new APMException($"当前用户无权限进行此操作: {entityName} - {type}");
                }
                else
                {
                    throw new APMException($"实体记录中未找到该实体: {entityName}");
                }

            }
            else
            {
                throw new APMException($"系统权限相关缓存丢失");
            }
        }

        private Guid GetCurrentUserId()
        {
            var claim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }

        private List<Guid> GetCurrentUserRoles()
        {
            var claim = httpContextAccessor.HttpContext?.User?.FindFirst(ConstDictionary.JwtClaimsRoleIds);
            var roles = JsonSerializer.Deserialize<List<Guid>>(claim?.Value ?? "[]");
            return roles ?? new List<Guid>();
        }
    }
}
