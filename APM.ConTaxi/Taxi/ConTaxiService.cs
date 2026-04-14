using APM.ConTaxi.Permission;
using APM.DbEntities;
using APM.DbEntities.Base;
using APM.UtilEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace APM.ConTaxi.Taxi
{
    internal class ConTaxiService : IConTaxiService
    {
        internal bool UseAdministration { get; set; }
        private readonly APMDbContext _context;
        private readonly ITaxiPermission _permission;

        public ConTaxiService(APMDbContext context, ITaxiPermission permission)
        {
            _context = context;
            _permission = permission;
        }

        public dynamic? Get(string entityName, Guid id)
        {
            // 程序集中所有继承自 BaseEntity 的非抽象类
            var type = EntityDriver.GetType(entityName);

            var entity = _context.Find(type, id);

            return entity;
        }

        public T? Get<T>(Guid id) where T : APMBaseEntity
        {
            if (!UseAdministration)
                _permission.CheckPermission<T>(PermissionType.Read);
            return _context.Find<T>(id);
        }

        public T? FirstOrDefault<T>(Func<T, bool>? where = null) where T : APMBaseEntity
        {
            if (!UseAdministration)
                _permission.CheckPermission<T>(PermissionType.Read);
            var query = _context.Set<T>().AsEnumerable();
            if (where != null)
                query = query.Where(where).AsEnumerable();

            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetDataSetQuery<T>(
            Func<T, bool>? where = null,
            int pageIndex = 1,
            int pageSize = 10,
            bool paging = true,
            Func<T, T>? orderby = null,
            bool descending = false) where T : APMBaseEntity
        {
            if (!UseAdministration)
                _permission.CheckPermission<T>(PermissionType.Read);
            var query = _context.Set<T>().AsEnumerable();
            if (where != null)
                query = query.Where(where).AsEnumerable();
            if (paging)
                query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            if (orderby != null)
                query = (descending ? query.OrderByDescending(orderby) : query.OrderBy(orderby)).AsEnumerable();

            return query;
        }

        public int Transaction<T>(T entity, EntityState entityState) where T : BaseEntity
        {
            if (!UseAdministration)
                _permission.CheckPermission<T>(entityState);
            var result = 0;
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                UpdateTimestamps(entity, entityState);

                _context.Entry(entity).State = entityState;
                result = _context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            return result;
        }

        public int Transaction<T>(IEnumerable<T> entities, EntityState entityState) where T : BaseEntity
        {
            if (!UseAdministration)
                _permission.CheckPermission<T>(entityState);
            var result = 0;
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    UpdateTimestamps(entity, entityState);
                    _context.Entry(entity).State = entityState;
                }

                result = _context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            return result;
        }

        public int Transaction<T>(IEnumerable<T> entities, IDictionary<Guid, EntityState> entitiesState) where T : BaseEntity
        {
            if (!UseAdministration)
                _permission.CheckPermission<T>(entitiesState.Select(es => es.Value).Distinct().ToList());

            var result = 0;
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    var keyExists = entitiesState.TryGetValue(entity.Id, out var entityState);
                    if (!keyExists)
                        continue;

                    UpdateTimestamps(entity, entityState);
                    _context.Entry(entity).State = entityState;
                }

                result = _context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            return result;
        }

        public void Migrate()
        {
            _context.Database.Migrate();
        }

        private void UpdateTimestamps<T>(T entity, EntityState entityState) where T : BaseEntity
        {
            if (entityState == EntityState.Added)
                entity.CreatedAt = DateTime.UtcNow;
            if (entityState == EntityState.Modified)
                entity.ModifiedAt = DateTime.UtcNow;
        }

        public Dictionary<User, List<UserRole>> UserLogin(string username)
        {
            var user = _context.User.FirstOrDefault(u => u.Username == username);
            if (user is null)
                throw new APMException("用户名不存在或密码错误。");
            var userRole = _context.UserRole.Where(ur => ur.UserId == user.Id).ToList();
            if (!userRole.Any())
                throw new APMException("请联系管理员设置该用户所属角色。");
            var dic = new Dictionary<User, List<UserRole>>();
            dic.Add(user, userRole);
            return dic;
        }
    }
}
