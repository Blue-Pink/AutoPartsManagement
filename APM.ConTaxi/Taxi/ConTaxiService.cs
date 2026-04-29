using APM.ConTaxi.Permission;
using APM.DbEntities;
using APM.DbEntities.Base;
using APM.DbEntities.DTOs;
using APM.UtilEntities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APM.ConTaxi.Taxi
{
    internal class ConTaxiService(APMDbContext context, ITaxiPermission permission) : IConTaxiService
    {
        internal bool UseAdministration { get; set; }

        private Type CheckEntityName(string entityName)
        {
            var type = EntityDriver.GetType(entityName);

            return type ?? throw new APMException($"未找到实体 {entityName}");
        }
        public object? Get(string entityName, Guid id)
        {
            if (!UseAdministration)
                permission.CheckPermission(entityName, PermissionType.Read);

            var type = CheckEntityName(entityName);

            var entity = context.Find(type, id);

            return entity;
        }

        public int Delete(string entityName, IEnumerable<Guid> ids)
        {
            var type = CheckEntityName(entityName);

            var method = GetType().GetMethod(nameof(Delete), [ids.GetType()]);
            if (method == null)
                throw new APMException($"未找到方法 {nameof(Delete)}");
            var genericMethod = method.MakeGenericMethod(type);
            var result = genericMethod.Invoke(this, [ids]);
            return result != null ? (int)result : 0;
        }

        public object Create(string entityName, JsonElement entity)
        {
            var type = CheckEntityName(entityName);

            // 权限校验（与 Get/Delete 保持一致的风格）
            if (!UseAdministration)
                permission.CheckPermission(entityName, PermissionType.Create);

            if (type.IsAbstract || type.IsInterface)
                throw new APMException($"实体 {entityName} 不可实例化");

            if (!typeof(BaseEntity).IsAssignableFrom(type))
                throw new APMException($"实体 {entityName} 必须继承自 BaseEntity");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonDateTimeConverter("yyyy-MM-dd HH:mm:ss") }
            };
            var instance = JsonSerializer.Deserialize(entity.GetRawText(), type, options);

            // 调用泛型 Create<T>(T entity)
            var createMethod = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m is { Name: nameof(Create), IsGenericMethodDefinition: true } && m.GetParameters().Length == 1);

            if (createMethod == null)
                throw new APMException("未找到泛型 Create 方法");

            var generic = createMethod.MakeGenericMethod(type);
            var created = generic.Invoke(this, [instance]);

            return created ?? throw new APMException($"{entityName} 创建失败");
        }

        public T? Get<T>(Guid id) where T : APMBaseEntity
        {
            if (!UseAdministration)
                permission.CheckPermission<T>(PermissionType.Read);
            return context.Find<T>(id);
        }

        public T? FirstOrDefault<T>(Expression<Func<T, bool>>? where = null) where T : APMBaseEntity
        {
            if (!UseAdministration)
                permission.CheckPermission<T>(PermissionType.Read);
            var query = context.Set<T>().AsQueryable();
            if (where != null)
                query = query.Where(where).AsQueryable();

            return query.FirstOrDefault();
        }

        public int Total<T>(Expression<Func<T, bool>>? where = null) where T : APMBaseEntity
        {
            if (!UseAdministration)
                permission.CheckPermission<T>(PermissionType.Read);

            return where != null ? context.Set<T>().Where(where).Count() : context.Set<T>().Count();
        }

        public IQueryable<T> GetDataSetQuery<T>(
            Expression<Func<T, bool>>? where = null,
            int pageIndex = 1,
            int pageSize = 10,
            bool paging = true,
            Expression<Func<T, object?>>? orderBy = null,
            bool descending = false,
            Expression<Func<T, object?>>[]? includes = null) where T : APMBaseEntity
        {
            if (!UseAdministration)
                permission.CheckPermission<T>(PermissionType.Read);

            IQueryable<T> query = context.Set<T>();

            if (includes != null)
                query = includes.Aggregate(query, (current, include) => current.Include(include));

            if (where != null)
                query = query.Where(where);

            if (orderBy != null)
                query = (descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy)).AsQueryable();
            else
            {
                query = query.OrderByDescending(t => t.CreatedAt);
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
            var user = context.User.FirstOrDefault(u => u.Username == username && u.IsActive);
            if (user is null)
                throw new APMException("用户名不存在或该用户已停用");
            var userRole = context.UserRole.Where(ur => ur.UserId == user.Id).ToList();
            if (!userRole.Any())
                throw new APMException("请联系管理员设置该用户所属角色");
            var dic = new Dictionary<User, List<UserRole>>
            {
                { user, userRole }
            };
            return dic;
        }

        public UserDTO GetCurrentUser(Guid userId)
        {
            var user = context.User.Find(userId);

            if (user is null)
                throw new APMException("未找到当前用户");

            var roles = context.UserRole.Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .Select(ur => new RoleDTO()
                {

                    RoleName = ur.Role == null ? "" : ur.Role.RoleName,
                    Description = ur.Role == null ? "" : ur.Role.Description,
                    Id = ur.RoleId
                });
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Realname = user.Realname,
                Roles = roles
            };

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

        public int Delete<T>(Expression<Func<T, bool>>? where) where T : BaseEntity
        {
            var entities = GetDataSetQuery(where, paging: false);
            if (entities.Any())
                return Transaction(entities, EntityState.Deleted);
            return 0;
        }

        public List<object> GetChildrenDataSetQuery(string parentEntityName, string childEntityName, Guid parentId)
        {
            if (!UseAdministration)
                permission.CheckPermission(parentEntityName, PermissionType.Read);
            if (!UseAdministration)
                permission.CheckPermission(childEntityName, PermissionType.Read);

            var parentType = EntityDriver.GetType(parentEntityName);
            var childType = EntityDriver.GetType(childEntityName);

            if (parentType == null || childType == null)
                throw new Exception("指定的实体名称无效");

            var entityType = context.Model.FindEntityType(childType);
            var foreignKey = entityType?.GetForeignKeys()
                .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == parentType);

            if (foreignKey == null)
                throw new Exception($"{childEntityName} 中没有找到指向 {parentEntityName} 的关联字段");

            var fkPropertyName = foreignKey.Properties[0].Name;

            //获取到子实体的 DbSet
            var query = context.GetType().GetMethod(nameof(context.Set), 1, Type.EmptyTypes)?.MakeGenericMethod(childType).Invoke(context, null) as IQueryable;
            if (query == null)
                throw new Exception($"无法获取 {childEntityName} 的 DbSet");

            var parameter = Expression.Parameter(childType, "e");
            var property = Expression.Property(parameter, fkPropertyName);
            var constant = Expression.Constant(parentId);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda(equality, parameter);

            var whereMethod = typeof(Queryable).GetMethods().First(m => m.Name == "Where" && m.GetParameters().Length == 2)?.MakeGenericMethod(childType);
            if (whereMethod == null)
                throw new APMException("无法获取 Where 方法");

            var filteredQuery = whereMethod.Invoke(null, [query, lambda]) as IQueryable;

            var toListMethod = typeof(Enumerable).GetMethod("ToList", BindingFlags.Public | BindingFlags.Static)?.MakeGenericMethod(childType);
            if (toListMethod == null)
                throw new APMException("无法获取 ToList 方法");

            var typedList = toListMethod.Invoke(null, [filteredQuery]);
            var resultAsEnumerable = typedList as System.Collections.IEnumerable
                                     ?? throw new APMException("查询结果为空");
            var resultList = resultAsEnumerable.Cast<object>().ToList();

            return resultList;
        }
    }
}
