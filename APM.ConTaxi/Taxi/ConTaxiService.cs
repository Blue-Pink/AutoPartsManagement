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
using Microsoft.EntityFrameworkCore.Metadata;

namespace APM.ConTaxi.Taxi
{
    internal class ConTaxiService(APMDbContext context, ITaxiPermission permission) : IConTaxiService
    {
        internal bool UseAdministration { get; set; }

        private Type CheckEntityName(string entityName)
        {
            var type = EntityDriver.GetType(entityName);

            if (type is null)
                throw new APMException($"未找到实体 {entityName}");

            if (!UseAdministration)
                permission.CheckPermission(entityName, PermissionType.Create);

            if (type.IsAbstract || type.IsInterface)
                throw new APMException($"实体 {entityName} 不可实例化");

            if (!typeof(BaseEntity).IsAssignableFrom(type))
                throw new APMException($"实体 {entityName} 必须继承自 BaseEntity");

            return type;
        }

        #region 泛型

        public IQueryable<T> BuildQuery<T>() where T : APMBaseEntity
        {
            if (!UseAdministration)
                permission.CheckPermission<T>(PermissionType.Read);

            return context.Set<T>();
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

        public int Count<T>(Expression<Func<T, bool>>? where = null) where T : APMBaseEntity
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

        #endregion

        #region 用户相关
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

        #endregion

        #region 实体名通用

        public object? Get(string entityName, Guid id)
        {
            if (!UseAdministration)
                permission.CheckPermission(entityName, PermissionType.Read);

            var type = CheckEntityName(entityName);

            var query = BuildQuery(type);
            GetEntityNavigationNames(type, out _, out string[] navigationNames, out _);
            query = LinkWhereExpression(query, type, nameof(BaseEntity.Id), id);
            query = LinkIncludeExpression(query, type, navigationNames);

            var entity = BuildList(query, type).FirstOrDefault();

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

        public object Edit(string entityName, JsonElement entity)
        {
            var type = BuildRawTextEntity(entityName, entity, out var properties, out var instance, out var id);

            if (id is null || id == Guid.Empty)
                return Create(type, instance) ?? throw new APMException($"{entityName} 创建失败");

            return Update(entityName, type, id.Value, instance, properties) ?? throw new APMException($"{entityName} - [{id.Value}] 更新失败");

        }

        public object? Create(Type entityType, object instance)
        {
            // 调用泛型 Create<T>(T entity)
            var createMethod = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m is { Name: nameof(Create), IsGenericMethodDefinition: true } && m.GetParameters().Length == 1);

            if (createMethod == null)
                throw new APMException($"未找到泛型 {nameof(Create)} 方法");

            var generic = createMethod.MakeGenericMethod(entityType);
            var created = generic.Invoke(this, [instance]);

            return created;
        }

        public object Update(string entityName, Type entityType, Guid id, object instance, IEnumerable<PropertyInfo> properties)
        {
            var efInstance = Get(entityName, id);
            if (efInstance is null)
                throw new APMException($"更新失败, 未找到对应数据：{entityName}({id})");


            foreach (var propertyInfo in properties)
            {
                var efValue = propertyInfo.GetValue(efInstance);
                var value = propertyInfo.GetValue(instance);
                if (efValue != value)
                {
                    propertyInfo.SetValue(efInstance, value);
                }
            }

            var updateMethod = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m is { Name: nameof(Update), IsGenericMethodDefinition: true } && m.GetParameters().Length == 1);

            if (updateMethod == null)
                throw new APMException($"未找到泛型 {nameof(Update)} 方法");

            var generic = updateMethod.MakeGenericMethod(entityType);
            var updated = generic.Invoke(this, [efInstance]);

            return updated ?? throw new APMException($"{entityName} 创建失败");
        }

        private Type BuildRawTextEntity(string entityName, JsonElement entity, out IEnumerable<PropertyInfo> properties, out object instance, out Guid? id)
        {
            var type = CheckEntityName(entityName);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonDateTimeConverter("yyyy-MM-dd HH:mm:ss") }
            };

            properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !typeof(BaseEntity).GetProperties().Contains(p));

            var valueProperties = properties.Where(p =>
                p.PropertyType.IsValueType && Nullable.GetUnderlyingType(p.PropertyType) == null);

            var nullProperty = valueProperties.FirstOrDefault(p => !entity.TryGetProperty(p.Name[0].ToString().ToLower() + p.Name[1..], out var value) || value.ValueKind == JsonValueKind.Null);
            if (nullProperty is not null)
                throw new APMException($"{nullProperty.Name}读取失败, {nullProperty.Name} 不可为 [NULL]");

            var serializedInstance = JsonSerializer.Deserialize(entity.GetRawText(), type, options);

            instance = serializedInstance ?? throw new APMException($"更新失败, 数据实例化失败");
            id = (instance as BaseEntity)?.Id;
            return type;
        }

        public List<object> GetChildrenDataSetQuery(string parentEntityName, string childEntityName, Guid parentId)
        {
            if (!UseAdministration)
                permission.CheckPermission(parentEntityName, PermissionType.Read);
            if (!UseAdministration)
                permission.CheckPermission(childEntityName, PermissionType.Read);

            var parentType = CheckEntityName(parentEntityName);
            var childType = CheckEntityName(childEntityName);

            GetEntityNavigationNames(childType, out _, out string[] navigationNames, out var foreignKeys);

            var parentForeignKey = foreignKeys.FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == parentType);
            if (parentForeignKey == null)
                throw new APMException($"{childEntityName} 中没有找到指向 {parentEntityName} 的关联字段");

            //获取到子实体的 DbSet
            //var query = context.GetType().GetMethod(nameof(context.Set), 1, Type.EmptyTypes)?.MakeGenericMethod(childType).Invoke(context, null) as IQueryable;

            var query = BuildQuery(childType);
            query = LinkWhereExpression(query, childType, parentForeignKey.Properties[0].Name, parentId);
            query = LinkIncludeExpression(query, childType, navigationNames);

            return BuildList(query, childType);
        }

        private IQueryable BuildQuery(Type entityType)
        {
            return GetType().GetMethod(nameof(BuildQuery), 1, Type.EmptyTypes)
                ?.MakeGenericMethod(entityType).Invoke(this, null) is not IQueryable query
                ? throw new APMException($"无法获取 {entityType.Name} 的 DbSet")
                : query;
        }

        private IQueryable LinkWhereExpression(IQueryable query, Type entityType, string entityFieldName, object? entityFieldValue)
        {
            var properties = entityType.GetProperties().Where(p => p.Name.Equals(entityFieldName) && p.DeclaringType is not null);
            if (!properties.Any())
                throw new APMException($"{entityType} 中不存在属性 {entityFieldName}");

            var parameter = Expression.Parameter(entityType);
            var property = Expression.Property(parameter, properties.Count() == 1
                ? properties.First()
                : properties.First(p => p.DeclaringType.Name.Equals(nameof(BaseEntity))));
            var constant = Expression.Constant(entityFieldValue);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda(equality, parameter);

            var whereMethod = typeof(Queryable).GetMethods().First(m => m.Name == "Where" && m.GetParameters().Length == 2)?.MakeGenericMethod(entityType);
            if (whereMethod == null)
                throw new APMException("无法获取 Where 方法");

            return whereMethod.Invoke(null, [query, lambda]) as IQueryable ?? throw new APMException($"查询连接表达式时出错: [{nameof(LinkWhereExpression)}]");
        }

        private IQueryable LinkIncludeExpression(IQueryable query, Type entityType, IEnumerable<string> navigations)
        {
            var navigationArray = navigations as string[] ?? navigations.ToArray();
            if (!navigationArray.Any()) return query;

            var includeMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "Include"
                                     && m.GetParameters().Length == 2
                                     && m.GetParameters()[1].ParameterType == typeof(string))
                ?.MakeGenericMethod(entityType);
            return includeMethod == null ? throw new APMException("无法获取 Include 方法") : navigationArray.Aggregate(query, (current, navigation) => (IQueryable)includeMethod.Invoke(null, [current, navigation])!);
        }

        private List<object> BuildList(IQueryable query, Type entityType)
        {
            var toListMethod = typeof(Enumerable).GetMethod("ToList", BindingFlags.Public | BindingFlags.Static)?.MakeGenericMethod(entityType);
            if (toListMethod == null)
                throw new APMException("无法获取 ToList 方法");

            var typedList = toListMethod.Invoke(null, [query]);
            var resultAsEnumerable = typedList as System.Collections.IEnumerable
                                     ?? throw new APMException("查询结果为空");
            return resultAsEnumerable.Cast<object>().ToList();
        }

        private void GetEntityNavigationNames(Type entityType, out IEnumerable<INavigation> navigations, out string[] navigationNames, out IEnumerable<IForeignKey> foreignKeys)
        {
            var efEntityType = context.Model.FindEntityType(entityType);
            if (efEntityType is null)
                throw new APMException($"Context 中未找到 {entityType.Name}");

            navigations = efEntityType.GetNavigations();
            navigationNames = navigations.Select(n => n.Name).ToArray();
            var linkNames = navigationNames;
            foreignKeys = efEntityType?.GetForeignKeys()
               .Where(fk => linkNames.Contains(fk.DependentToPrincipal?.Name)) ?? throw new APMException($"{entityType.Name} 缺少与任一表的关系"); ;
        }
        #endregion

        public void Migrate()
        {
            try
            {
                context.Database.Migrate();

            }
            catch (Exception)
            {
                // ignored
            }
        }

    }
}
