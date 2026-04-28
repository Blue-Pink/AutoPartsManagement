using APM.ConTaxi.Permission;
using APM.DbEntities;
using APM.DbEntities.Base;
using APM.UtilEntities;
using Microsoft.EntityFrameworkCore;

namespace APM.ConTaxi.Taxi
{
    internal class ConTaxiService(APMDbContext context, ITaxiPermission permission) : IConTaxiService
    {
        internal bool UseAdministration { get; set; }

        public object? Get(string entityName, Guid id)
        {
            if (!UseAdministration)
                permission.CheckPermission(entityName, PermissionType.Read);

            var type = EntityDriver.GetType(entityName);

            var entity = context.Find(type, id);

            return entity;
        }

        public T? Get<T>(Guid id) where T : APMBaseEntity
        {
            if (!UseAdministration)
                permission.CheckPermission<T>(PermissionType.Read);
            return context.Find<T>(id);
        }

        public T? FirstOrDefault<T>(Func<T, bool>? where = null) where T : APMBaseEntity
        {
            if (!UseAdministration)
                permission.CheckPermission<T>(PermissionType.Read);
            var query = context.Set<T>().AsEnumerable();
            if (where != null)
                query = query.Where(where).AsEnumerable();

            return query.FirstOrDefault();
        }

        public int Total<T>() where T : APMBaseEntity
        {
            if (!UseAdministration)
                permission.CheckPermission<T>(PermissionType.Read);

            return context.Set<T>().Count();
        }

        public IEnumerable<T> GetDataSetQuery<T>(
            Func<T, bool>? where = null,
            int pageIndex = 1,
            int pageSize = 10,
            bool paging = true,
            Func<T, object>? orderBy = null,
            bool descending = false) where T : APMBaseEntity
        {
            if (!UseAdministration)
                permission.CheckPermission<T>(PermissionType.Read);

            var query = context.Set<T>().AsEnumerable();

            if (where != null)
                query = query.Where(where).AsEnumerable();

            if (orderBy != null)
                query = (descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy)).AsEnumerable();
            else
            {
                //默认创建时间倒序
                var createdAtProp = typeof(T).GetProperty("CreatedAt");
                if (createdAtProp != null && createdAtProp.PropertyType == typeof(DateTime))
                {
                    query = query.OrderByDescending(x => (DateTime)createdAtProp.GetValue(x)!);
                }
            }

            if (paging)
                query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return query;
        }

        public int Transaction<T>(T entity, EntityState entityState) where T : BaseEntity
        {
            if (!UseAdministration)
                permission.CheckPermission<T>(entityState);
            var result = 0;
            using var transaction = context.Database.BeginTransaction();
            try
            {
                UpdateIdAndTimestamps(entity, entityState);

                context.Entry(entity).State = entityState;
                result = context.SaveChanges();
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
                permission.CheckPermission<T>(entityState);
            var result = 0;
            using var transaction = context.Database.BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    UpdateIdAndTimestamps(entity, entityState);
                    context.Entry(entity).State = entityState;
                }

                result = context.SaveChanges();
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
                permission.CheckPermission<T>(entitiesState.Select(es => es.Value).Distinct().ToList());

            var result = 0;
            using var transaction = context.Database.BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    var keyExists = entitiesState.TryGetValue(entity.Id, out var entityState);
                    if (!keyExists)
                        continue;

                    UpdateIdAndTimestamps(entity, entityState);
                    context.Entry(entity).State = entityState;
                }

                result = context.SaveChanges();
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
            try
            {
                context.Database.Migrate();

            }
            catch (Exception)
            {

            }
        }

        private void UpdateIdAndTimestamps<T>(T entity, EntityState entityState) where T : BaseEntity
        {
            if (entityState == EntityState.Added)
            {
                entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
                entity.CreatedAt = DateTime.UtcNow;
            }
            if (entityState == EntityState.Modified)
                entity.ModifiedAt = DateTime.UtcNow;
        }

        public Dictionary<User, List<UserRole>> UserLogin(string username)
        {
            var user = context.User.FirstOrDefault(u => u.Username == username);
            if (user is null)
                throw new APMException("用户名不存在或密码错误");
            var userRole = context.UserRole.Where(ur => ur.UserId == user.Id).ToList();
            if (!userRole.Any())
                throw new APMException("请联系管理员设置该用户所属角色");
            var dic = new Dictionary<User, List<UserRole>>
            {
                { user, userRole }
            };
            return dic;
        }

        public T Create<T>(T entity) where T : BaseEntity
        {
            Transaction(entity, EntityState.Added);
            return Get<T>(entity.Id) ?? throw new APMException($"{typeof(T).Name} 创建失败");
        }

        public int Create<T>(IEnumerable<T> entities) where T : BaseEntity
        {
            return Transaction(entities, EntityState.Added);
        }

        public T Update<T>(T entity) where T : BaseEntity
        {
            var tEntity = FirstOrDefault<T>(t => t.Id == entity.Id);
            if (tEntity is null)
                throw new APMException($"更新失败，未找到对应数据：{typeof(T).Name}({entity.Id})");
            Transaction(entity, EntityState.Modified);

            return Get<T>(tEntity.Id) ?? throw new APMException($"{typeof(T).Name}({tEntity.Id}) 更新失败");
        }

        public int Delete<T>(Guid id) where T : BaseEntity
        {
            var entity = FirstOrDefault<T>(t => t.Id == id);
            if (entity is null)
                return 0;
            return Transaction(entity, EntityState.Deleted);
        }

        public int Delete<T>(IEnumerable<Guid> ids) where T : BaseEntity
        {
            var entities = context.Set<T>().Where(t => ids.Contains(t.Id));
            if (entities.Any())
                return Transaction(entities, EntityState.Deleted);
            return 0;
        }

        public int Delete<T>(Func<T, bool> where) where T : BaseEntity
        {
            var entities = GetDataSetQuery(where);
            if (entities.Any())
                return Transaction(entities, EntityState.Deleted);
            return 0;
        }
    }
}
