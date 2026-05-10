using APM.ConTaxi.Permission;
using APM.DbEntities;
using APM.DbEntities.Base;
using APM.DbEntities.DTOs;
using APM.IServices;
using APM.UtilEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            var entities = GetDataSetQuery(where);
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
            int pageIndex = 0,
            int pageSize = 10,
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

            if (pageIndex != 0)
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
            GetEntityNavigations(type, out var navigationNames);
            query = LinkWhereExpression(query, nameof(BaseEntity.Id), id);
            query = LinkIncludeExpression(query, navigationNames);

            var entity = BuildList(query).FirstOrDefault();

            return entity;
        }

        public int Delete(string entityName, IEnumerable<Guid> ids)
        {
            if (!UseAdministration)
                permission.CheckPermission(entityName, PermissionType.Delete);

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
            if (!UseAdministration)
                permission.CheckPermission(entityName, PermissionType.Read);

            var type = BuildRawTextEntity(entityName, entity, out var properties, out var instance, out var id);

            try
            {
                if (id is null || id == Guid.Empty)
                    return Create(type, instance) ?? throw new APMException($"{entityName} 创建失败");

                return Update(entityName, type, id.Value, instance, properties) ??
                       throw new APMException($"[{entityName}] - [{id.Value}] 更新失败");
            }
            catch (APMException)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (exception.InnerException != null)
                    throw exception.InnerException;
                throw;
            }

        }

        private object? Create(Type entityType, object instance)
        {
            // 调用泛型 Create<T>(T entity)
            var createMethod = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m is { Name: nameof(Create), IsGenericMethodDefinition: true } && m.GetParameters().Length == 1);

            if (createMethod == null)
                throw new APMException($"未找到泛型 [{nameof(Create)}] 方法");

            var generic = createMethod.MakeGenericMethod(entityType);
            var created = generic.Invoke(this, [instance]);

            return created;
        }

        private object? Update(string entityName, Type entityType, Guid id, object instance, IEnumerable<PropertyInfo> properties)
        {
            var efInstance = Get(entityName, id);
            if (efInstance is null)
                throw new APMException($"更新失败, 未找到对应数据：[{entityName}] - [{id}]");

            foreach (var propertyInfo in properties)
            {
                switch (propertyInfo.Name)
                {
                    case nameof(BaseEntity.OperatorUserId):
                        //propertyInfo.SetValue(efInstance, userContext.UserId ?? throw new APMException("未登录"));
                        break;
                    case nameof(BaseEntity.OperatorUser):
                        //var user = Get(nameof(User), userContext.UserId ?? throw new APMException("未登录"));
                        //propertyInfo.SetValue(efInstance, user);
                        break;
                    default:
                        var efValue = propertyInfo.GetValue(efInstance);
                        var value = propertyInfo.GetValue(instance);
                        if (efValue != value && value != null)
                            propertyInfo.SetValue(efInstance, value);
                        break;
                }
            }

            var updateMethod = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m is { Name: nameof(Update), IsGenericMethodDefinition: true } && m.GetParameters().Length == 1);

            if (updateMethod == null)
                throw new APMException($"未找到泛型 [{nameof(Update)}] 方法");

            var generic = updateMethod.MakeGenericMethod(entityType);
            var updated = generic.Invoke(this, [efInstance]);

            return updated;
        }

        private Type BuildRawTextEntity(string entityName, JsonElement entity, out IEnumerable<PropertyInfo> properties, out object instance, out Guid? id)
        {
            var type = CheckEntityName(entityName);
            GetEntityNavigations(type, out var navigationNames);

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

            foreach (var navigationName in navigationNames)
            {
                var propertyInfo = properties.FirstOrDefault(p => p.Name == navigationName);
                if (propertyInfo is null) continue;

                propertyInfo.SetValue(instance, null);
            }

            id = (instance as BaseEntity)?.Id;
            return type;
        }

        public PagingData<object> GetDataSet(string entityName,
            int pageIndex = 0,
            int pageSize = 10,
            string orderBy = "",
            bool descending = false,
            string filter = "",
            int depth = 1)
        {
            return GetChildrenDataSet(entityName, pageIndex, pageSize, orderBy, descending, filter, depth);
        }

        private PagingData<object> GetChildrenDataSet(string entityName,
            int pageIndex = 0,
            int pageSize = 10,
            string orderBy = "",
            bool descending = false,
            string filter = "",
            int depth = 1)
        {
            return GetChildrenDataSet(null, entityName, null, pageIndex, pageSize, orderBy, descending, filter, depth);
        }

        public PagingData<object> GetChildrenDataSet(string? parentEntityName,
            string childEntityName,
            Guid? parentId,
            int pageIndex = 0,
            int pageSize = 10,
            string orderBy = "",
            bool descending = false,
            string filter = "",
            int depth = 1)
        {
            if (!UseAdministration && !string.IsNullOrEmpty(parentEntityName))
                permission.CheckPermission(parentEntityName, PermissionType.Read);
            if (!UseAdministration)
                permission.CheckPermission(childEntityName, PermissionType.Read);

            var childType = CheckEntityName(childEntityName);

            GetEntityNavigations(childType, out var navigationNames);

            var query = BuildQuery(childType);

            if (!string.IsNullOrEmpty(parentEntityName) && parentId != null)
            {
                var parentType = CheckEntityName(parentEntityName);
                var foreignKeys = GetEntityForeignKey(childType, navigationNames);
                var parentForeignKey = foreignKeys.FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == parentType);
                if (parentForeignKey == null)
                    throw new APMException($"{childEntityName} 中没有找到指向 {parentEntityName} 的关联字段");

                query = LinkWhereExpression(query, parentForeignKey.Properties[0].Name, parentId);
            }

            if (!string.IsNullOrEmpty(filter))
            {
                ConstDictionary.EntityFieldSettings.TryGetValue(childType, out var fieldSettings);
                if (fieldSettings != null)
                {
                    query = fieldSettings.Where(fieldSetting => fieldSetting.Filter).Aggregate(query, (current, fieldSetting) => LinkWhereExpression(current, fieldSetting.Name, filter));
                }
            }

            var total = ExecuteQueryCount(query);

            query = LinkOrderByExpression(query, orderBy, descending);

            query = LinkIncludeExpression(query, navigationNames, depth);

            if (pageIndex > 0)
                query = LinkPaginationExpression(query, pageIndex, pageSize);

            return new PagingData<object>(BuildList(query), total, pageIndex, pageSize);
        }

        private IQueryable BuildQuery(Type entityType)
        {
            return GetType().GetMethod(nameof(BuildQuery), 1, Type.EmptyTypes)
                ?.MakeGenericMethod(entityType).Invoke(this, null) is not IQueryable query
                ? throw new APMException($"无法获取 [{entityType.Name}] 的 DbSet")
                : query;
        }

        private IQueryable LinkWhereExpression(IQueryable query, string entityFieldName, object? entityFieldValue)
        {
            var entityType = query.ElementType;
            var propertyInfos = entityType.GetProperties().Where(p => p.Name.Equals(entityFieldName) && p.DeclaringType is not null);
            if (!propertyInfos.Any())
                throw new APMException($"[{entityType}] 中不存在属性 [{entityFieldName}]");

            var propertyInfo = propertyInfos.Count() == 1
                ? propertyInfos.FirstOrDefault()
                : propertyInfos.FirstOrDefault(p => p.DeclaringType is { Name: nameof(BaseEntity) });

            if (propertyInfo == null)
                throw new APMException($"[{entityType}] 中不存在属性 [{entityFieldName}]");

            var parameter = Expression.Parameter(entityType);
            var property = Expression.Property(parameter, propertyInfo);
            var constant = Expression.Constant(DynamicConvert(entityFieldValue, propertyInfo.PropertyType));
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda(equality, parameter);

            var whereMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m is { Name: nameof(Queryable.Where), IsGenericMethod: true } && m.GetParameters().Length == 2)
                ?.MakeGenericMethod(entityType);
            if (whereMethod == null)
                throw new APMException($"无法获取 [{nameof(Queryable.Where)}] 方法");

            return whereMethod.Invoke(null, [query, lambda]) as IQueryable
                   ?? throw new APMException($"查询连接表达式时出错: [{nameof(LinkWhereExpression)}]");
        }

        public object? DynamicConvert(object? value, Type targetType)
        {
            // 如果值已经是目标类型，直接返回
            if (value is null || value.GetType() == targetType) return value;

            // 获取目标类型的转换器
            var converter = TypeDescriptor.GetConverter(targetType);

            if (converter.CanConvertFrom(value.GetType()))
            {
                // 这里会自动处理 string -> Guid 的内部逻辑
                return converter.ConvertFrom(value);
            }

            // 备用逻辑：如果是 Guid 且没匹配到，强制解析字符串
            if (targetType == typeof(Guid))
            {
                return Guid.Parse(value.ToString()!);
            }

            throw new InvalidOperationException($"无法将类型 {value.GetType()} 转换为 {targetType}");
        }

        private int ExecuteQueryCount(IQueryable query)
        {
            var entityType = query.ElementType;
            var countMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m is { Name: nameof(Queryable.Count), IsGenericMethod: true }
                            && m.GetParameters().Length == 1)
                ?.MakeGenericMethod(entityType);
            if (countMethod == null)
                throw new APMException($"无法获取 [{nameof(Queryable.Count)}] 方法");

            return Convert.ToInt32(countMethod.Invoke(null, [query]));
        }

        private IQueryable LinkIncludeExpression(IQueryable query, IEnumerable<string> navigations, int depth = 1)
        {
            var entityType = query.ElementType;
            var navigationArray = navigations as string[] ?? navigations.ToArray();
            if (!navigationArray.Any()) return query;

            var includeMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .FirstOrDefault(m => m is { Name: nameof(EntityFrameworkQueryableExtensions.Include), IsGenericMethod: true }
                            && m.GetParameters().Length == 2
                            && m.GetParameters()[1].ParameterType == typeof(string))
                ?.MakeGenericMethod(entityType);
            if (includeMethod == null)
                throw new APMException($"无法获取 [{nameof(EntityFrameworkQueryableExtensions.Include)}] 方法");

            var includePaths = new List<string>();

            BuildIncludePaths(
                entityType,
                navigations,
                depth,
                string.Empty,
                includePaths,
                []);

            foreach (var navigation in includePaths.Distinct())
            {
                query = includeMethod.Invoke(null, [query, navigation]) as IQueryable
                        ?? throw new APMException($"联查失败 [{entityType.Name}] - [{navigation}] - [{nameof(EntityFrameworkQueryableExtensions.Include)}] 方法");
            }

            return query;
        }

        private void BuildIncludePaths(Type entityType,
            IEnumerable<string> navigations,
            int depth,
            string parentPath,
            List<string> paths,
            HashSet<Type> visited)
        {
            if (depth <= 0)
                return;

            visited.Add(entityType);

            foreach (var navigation in navigations)
            {
                var property = entityType.GetProperty(navigation);

                if (property == null)
                    continue;

                var childType = property.PropertyType;

                // 集合导航
                if (childType.IsGenericType &&
                    typeof(IEnumerable).IsAssignableFrom(childType))
                {
                    childType = childType.GetGenericArguments()[0];
                }

                // 防止循环导航
                if (visited.Contains(childType))
                    continue;

                var currentPath = string.IsNullOrEmpty(parentPath)
                    ? navigation
                    : $"{parentPath}.{navigation}";

                paths.Add(currentPath);

                if (!UseAdministration)
                    permission.CheckPermission(childType.Name, PermissionType.Read);

                GetEntityNavigations(childType, out var childNavigations);

                BuildIncludePaths(
                    childType,
                    childNavigations,
                    depth - 1,
                    currentPath,
                    paths,
                    [.. visited]);
            }
        }

        private IQueryable LinkPaginationExpression(IQueryable query, int pageIndex, int pageSize)
        {
            var entityType = query.ElementType;
            var skipMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m is { Name: nameof(Queryable.Skip), IsGenericMethod: true } && m.GetParameters().Length == 2)
                ?.MakeGenericMethod(entityType);
            if (skipMethod == null)
                throw new APMException($"无法获取 [{nameof(Queryable.Skip)}] 方法");

            query = skipMethod.Invoke(null, [query, (pageIndex - 1) * pageSize]) as IQueryable
                    ?? throw new APMException($"查询连接表达式时出错: [{nameof(LinkPaginationExpression)}] - [{nameof(Queryable.Skip)}]");

            var takeMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m is { Name: nameof(Queryable.Take), IsGenericMethod: true } && m.GetParameters().Length == 2)
                ?.MakeGenericMethod(entityType);
            if (takeMethod == null)
                throw new APMException($"无法获取 [{nameof(Queryable.Take)}] 方法");

            return takeMethod.Invoke(null, [query, pageSize]) as IQueryable
                   ?? throw new APMException($"查询连接表达式时出错: [{nameof(LinkPaginationExpression)}] - [{nameof(Queryable.Take)}]");
        }

        private IQueryable LinkOrderByExpression(IQueryable query, string orderBy, bool descending)
        {
            var entityType = query.ElementType;
            descending = string.IsNullOrEmpty(orderBy) || descending;
            orderBy = string.IsNullOrEmpty(orderBy) ? nameof(APMBaseEntity.CreatedAt) : orderBy;

            var methodName = descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
            var propertyInfo = entityType.GetProperties().FirstOrDefault(p => p.Name.Equals(orderBy, StringComparison.CurrentCultureIgnoreCase))
                               ?? throw new APMException($"查询连接表达式时出错: [{nameof(LinkOrderByExpression)}] - [{entityType.Name}] - [{orderBy}]");

            var orderByMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == methodName && m.IsGenericMethod && m.GetParameters().Length == 2);

            LambdaExpression lambda;
            var navigations = GetEntityNavigations(entityType, out var navigationNames);
            if (navigationNames.Contains(propertyInfo.Name))
            {
                var navigation = navigations.FirstOrDefault(n => n.Name == propertyInfo.Name) ?? throw new APMException($"未找到导航属性 [{propertyInfo.Name}]");
                var entityProperty = navigation.TargetEntityType.ClrType.GetProperties()
                    .FirstOrDefault(p => p.Name.ToLower().Contains("name", StringComparison.CurrentCultureIgnoreCase)
                                || p.Name.Contains("no", StringComparison.CurrentCultureIgnoreCase)
                                || p.Name.Contains("id", StringComparison.CurrentCultureIgnoreCase))
                                     ?? throw new APMException($"未找到此实体的可导航属性 [{navigation.TargetEntityType.ClrType.Name}]");
                var parameter = Expression.Parameter(entityType);
                var property = Expression.Property(parameter, propertyInfo);
                property = Expression.Property(property, entityProperty);
                lambda = Expression.Lambda(property, parameter);

                orderByMethod = orderByMethod?.MakeGenericMethod(entityType, entityProperty.PropertyType);

            }
            else
            {
                var parameter = Expression.Parameter(entityType);
                var property = Expression.Property(parameter, propertyInfo);
                lambda = Expression.Lambda(property, parameter);
                orderByMethod = orderByMethod?.MakeGenericMethod(entityType, propertyInfo.PropertyType);
            }

            if (orderByMethod == null)
                throw new APMException($"无法获取 [{methodName}] 方法");

            return orderByMethod.Invoke(null, [query, lambda]) as IQueryable ?? throw new APMException($"查询连接表达式时出错: [{nameof(LinkOrderByExpression)}]");
        }

        private List<object> BuildList(IQueryable query)
        {
            var entityType = query.ElementType;
            var toListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList), BindingFlags.Public | BindingFlags.Static)?.MakeGenericMethod(entityType);
            if (toListMethod == null)
                throw new APMException($"无法获取 [{nameof(Enumerable.ToList)}] 方法");

            var typedList = toListMethod.Invoke(null, [query]);
            var resultAsEnumerable = typedList as IEnumerable
                                     ?? throw new APMException($"构建数据集失败 {nameof(BuildList)}");
            return resultAsEnumerable.Cast<object>().ToList();
        }

        private IEnumerable<INavigation> GetEntityNavigations(Type entityType, out string[] navigationNames)
        {
            var efEntityType = context.Model.FindEntityType(entityType);
            if (efEntityType is null)
                throw new APMException($"Context 中未找到 [{entityType.Name}]");

            var navigations = efEntityType.GetNavigations();
            navigationNames = navigations.Select(n => n.Name).ToArray();

            return navigations;
        }

        private IEnumerable<IForeignKey> GetEntityForeignKey(Type entityType, string[] navigationNames)
        {
            var efEntityType = context.Model.FindEntityType(entityType);
            return efEntityType?.GetForeignKeys()
                .Where(fk => navigationNames.Contains(fk.DependentToPrincipal?.Name)) ?? throw new APMException($"获取 [{entityType.Name}] 的关系失败");
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
