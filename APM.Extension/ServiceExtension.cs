using APM.ConTaxi.Bridger;
using APM.DbEntities;
using APM.DbEntities.Base;
using APM.UtilEntities;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text.Json;

namespace APM.Extensions
{
    public static class ServiceExtension
    {
        /// <summary>
        /// 为指定的接口与实现类型使用 Castle DynamicProxy 创建一个带拦截器的 scoped 代理并注册到依赖注入容器
        /// </summary>
        /// <typeparam name="TInterface">要注册的接口类型（必须为引用类型）</typeparam>
        /// <typeparam name="TImplementation">接口的具体实现类型（必须为引用类型并实现 <typeparamref name="TInterface"/>）</typeparam>
        /// <param name="service">要扩展的 <see cref="IServiceCollection"/> 实例</param>
        /// <remarks>
        /// 本方法执行以下步骤：
        /// 1. 将 <typeparamref name="TImplementation"/> 以 scoped 方式注册为真实实现 
        /// 2. 将 <typeparamref name="TInterface"/> 注册为一个 scoped 工厂，工厂从容器解析 <see cref="ProxyGenerator"/>, 
        ///    <typeparamref name="TImplementation"/> 和 <see cref="IInterceptor"/>，并调用
        ///    <see cref="ProxyGenerator.CreateInterfaceProxyWithTarget(Type, object, IInterceptor)"/> 为接口创建代理
        /// 
        /// 重要：容器必须先注册好 <see cref="ProxyGenerator"/>（通常为 singleton）和对应的 <see cref="IInterceptor"/>（通常为 scoped），
        /// 否则在解析时会由 <c>GetRequiredService</c> 抛出异常
        /// </remarks>
        /// <example>
        /// services.AddSingleton<ProxyGenerator>();
        /// services.AddScoped<IInterceptor, APMExtensionInterceptor>();
        /// services.AddProxiedScoped<IMyService, MyService>();
        /// </example>
        public static void AddProxiedScoped<TInterface, TImplementation>(this IServiceCollection service)
            where TInterface : class where TImplementation : class, TInterface
        {
            service.AddScoped<TImplementation>();
            service.AddScoped(typeof(TInterface), provider =>
            {
                var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
                var implementation = provider.GetRequiredService<TImplementation>();
                var interceptor = provider.GetRequiredService<IInterceptor>();
                return proxyGenerator.CreateInterfaceProxyWithTarget(typeof(TInterface), implementation, interceptor);
            });

        }

        /// <summary>
        /// 为具体实现类型创建一个带拦截器的 scoped 代理并注册到依赖注入容器（按类型注册）
        /// </summary>
        /// <typeparam name="TImplementation">要注册并代理的实现类型（必须为引用类型）</typeparam>
        /// <param name="service">要扩展的 <see cref="IServiceCollection"/> 实例</param>
        /// <remarks>
        /// 本方法会将 <typeparamref name="TImplementation"/> 按类型注册为 scoped，工厂解析 <see cref="ProxyGenerator"/>,
        /// <typeparamref name="TImplementation"/>（真实实现）和 <see cref="IInterceptor"/>,
        /// 并通过 <see cref="ProxyGenerator.CreateInterfaceProxyWithTarget(object, IInterceptor)"/> 创建代理实例返回
        /// 
        /// 注意：当前实现的工厂在解析真实实现时调用了 <c>provider.GetRequiredService<TImplementation>()</c>，如果未先以其他方式注册该实现，
        /// 可能导致循环解析或异常通常应先显式注册真实实现，再使用本方法，或调整注册逻辑以避免自引用解析
        /// </remarks>
        /// <example>
        /// services.AddSingleton<ProxyGenerator>();
        /// services.AddScoped<IInterceptor, APMExtensionInterceptor>();
        /// // 若需要，先注册真实实现：
        /// services.AddScoped<MyConcreteService>();
        /// services.AddProxiedScoped<MyConcreteService>();
        /// </example>
        public static void AddProxiedScoped<TImplementation>(this IServiceCollection service) where TImplementation : class
        {
            service.AddScoped(typeof(TImplementation), provider =>
            {
                var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
                var implementation = provider.GetRequiredService<TImplementation>();
                var interceptor = provider.GetRequiredService<IInterceptor>();
                var implementationProxy = proxyGenerator.CreateInterfaceProxyWithTarget(implementation, interceptor);
                return implementationProxy;
            });
        }

        public static void AddJsonWebTokenService(this IServiceCollection service, IConfiguration configuration)
        {
            if (configuration is null) throw new ArgumentNullException(nameof(configuration));

            var jwtSection = configuration.GetSection("JsonWebTokenSettings");
            var jwtSetting = new JsonWebTokenSetting
            {
                IssuerSigningKey = jwtSection["IssuerSigningKey"]?.ToString() ?? string.Empty,
                ValidAudience = jwtSection["ValidAudience"]?.ToString() ?? string.Empty,
                ValidIssuer = jwtSection["ValidIssuer"]?.ToString() ?? string.Empty,
                RequireExpirationTime = bool.TryParse(jwtSection["RequireExpirationTime"], out var requireExpiration) && requireExpiration,
                ValidateAudience = bool.TryParse(jwtSection["ValidateAudience"], out var validateAudience) && validateAudience,
                ValidateIssuer = bool.TryParse(jwtSection["ValidateIssuer"], out var validateIssuer) && validateIssuer,
                ValidateIssuerSigningKey = bool.TryParse(jwtSection["ValidateIssuerSigningKey"], out var validateIssuerSigningKey) && validateIssuerSigningKey,
                ValidateLifetime = bool.TryParse(jwtSection["ValidateLifetime"], out var validateLifetime) && validateLifetime,
            };

            service.AddSingleton(jwtSetting);
            service.AddSingleton(new JwtSecurityTokenHandler());

            service.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = jwtSetting.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSetting.IssuerSigningKey)),
                    ValidateIssuer = jwtSetting.ValidateIssuer,
                    ValidIssuer = jwtSetting.ValidIssuer,
                    ValidateAudience = jwtSetting.ValidateAudience,
                    ValidAudience = jwtSetting.ValidAudience,
                    RequireExpirationTime = jwtSetting.RequireExpirationTime,
                    ValidateLifetime = jwtSetting.RequireExpirationTime,
                    ClockSkew = TimeSpan.FromDays(1),
                };
            });

        }

        public static void MigrationAndSyncEntities(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.TaxiInvokeAdmin((taxi, redis) =>
           {
               taxi.Migrate();

               //通过反射获取当前程序集中所有继承自BaseEntity的非抽象类
               var entityAssembly = typeof(APMBaseEntity).Assembly;
               var AssemblyEntities = entityAssembly
                    .GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(APMBaseEntity))
                                && t != typeof(EntityRecord))
                    .Select(t => new { t.Name, FullName = t.FullName ?? "", t.GetCustomAttribute<DescriptionAttribute>()?.Description })
                    .Where(t => !string.IsNullOrEmpty(t.FullName))
                    .ToList();
               var updateEntityRecords = new List<EntityRecord>();
               var recordsState = new Dictionary<Guid, EntityState>();

               //获取数据库中已有的记录
               var entityRecords = taxi.GetDataSetQuery<EntityRecord>(paging: false).ToList();

               //处理新增或重新启用的实体
               foreach (var entity in AssemblyEntities)
               {
                   var record = entityRecords.FirstOrDefault(r => r.FullName == entity.FullName);
                   if (record == null)
                   {
                       var id = Guid.NewGuid();
                       // 新增
                       updateEntityRecords.Add(new EntityRecord
                       {
                           Id = id,
                           EntityName = entity.Name,
                           FullName = entity.FullName!,
                           IsActive = true,
                           Description = entity.Description ?? "",
                       });
                       recordsState[id] = EntityState.Added;
                   }
                   else
                   {
                       var update = false;
                       if (record.IsActive == false)
                       {
                           record.IsActive = true;
                           update = true;
                       }

                       if (!string.IsNullOrEmpty(entity.Description) && !entity.Description.Equals(record.Description, StringComparison.CurrentCultureIgnoreCase))
                       {
                           record.Description = entity.Description;
                           update = true;
                       }

                       if (update == true)
                       {
                           record.EntityName = entity.Name;
                           record.FullName = entity.FullName;
                           updateEntityRecords.Add(record);
                           recordsState[record.Id] = EntityState.Modified;
                       }
                   }
               }

               //处理被删除或不再继承的实体
               foreach (var record in entityRecords)
               {
                   if (!AssemblyEntities.Any(e => e.FullName == record.FullName))
                   {
                       record.IsActive = false;
                       updateEntityRecords.Add(record);
                       recordsState[record.Id] = EntityState.Modified;
                   }
               }

               if (updateEntityRecords.Any() && recordsState.Any() && updateEntityRecords.Count == recordsState.Count)
                   taxi.Transaction(updateEntityRecords, recordsState);

               var entityRecord= taxi.GetDataSetQuery<EntityRecord>(paging: false).Select(er => new { er.Id, er.FullName }).ToList();
               redis?.Set(ConstDictionary.RedisCacheEntityRecord, entityRecord, TimeSpan.FromDays(365));
           });

        }

        public static void CreateAdministratorPromission(this IApplicationBuilder applicationBuilder, IConfiguration configuration)
        {
            if (bool.TryParse(configuration.GetSection("AdministratorPermissionInitail").Value, out var isInitial) && isInitial)
            {
                applicationBuilder.TaxiInvokeAdmin((taxi, _) =>
                {
                    var adminRole = taxi.FirstOrDefault<Role>(role => role.RoleName?.Equals("Administrator", StringComparison.CurrentCultureIgnoreCase) ?? false);
                    //为初始管理员创建所有实体的权限
                    if (adminRole != null)
                    {
                        //管理员现有权限
                        var adminPermissionEntities = taxi.GetDataSetQuery<RolePermission>(rp => rp.RoleId == adminRole.Id, paging: false).Select(rp => rp.EntityId).ToList();
                        //所有实体记录
                        var entities = taxi.GetDataSetQuery<EntityRecord>(where: er => er.IsActive, paging: false).ToList();
                        var updatePermissions = new List<RolePermission>();
                        foreach (var entity in entities)
                        {
                            if (!adminPermissionEntities.Contains(entity.Id))
                                updatePermissions.Add(new RolePermission
                                {
                                    RoleId = adminRole.Id,
                                    EntityId = entity.Id,
                                    CanCreate = true,
                                    CanRead = true,
                                    CanUpdate = true,
                                    CanDelete = true,
                                });
                        }

                        if (updatePermissions.Any())
                            taxi.Transaction(updatePermissions, EntityState.Added);
                    }
                });
            }
        }

        public static void RedisCacheRolePermission(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.TaxiInvokeAdmin((taxi, redis) =>
            {
                var permissions = taxi.GetDataSetQuery<RolePermission>(paging: false).Select(rp => new { rp.RoleId, rp.EntityId, rp.CanRead, rp.CanCreate, rp.CanUpdate, rp.CanDelete }).ToList();
                redis?.Set(ConstDictionary.RedisCacheRolePermission, permissions, TimeSpan.FromDays(365));
            });
        }
    }
}
