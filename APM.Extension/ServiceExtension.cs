using APM.ConTaxi.Bridger;
using APM.DbEntities;
using APM.DbEntities.Base;
using APM.UtilEntities;
using Bogus;
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

        public static void MigrationAndSyncEntities(this IApplicationBuilder app)
        {
            app.TaxiInvokeAdmin((taxi, redis) =>
           {
               taxi.Migrate();

               //通过反射获取当前程序集中所有继承自BaseEntity的非抽象类
               var entityAssembly = typeof(APMBaseEntity).Assembly;
               var assemblyEntities = entityAssembly
                    .GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false }
                                && t.IsSubclassOf(typeof(APMBaseEntity))
                                && t != typeof(EntityRecord))
                    .Select(t => new { t.Name, FullName = t.FullName ?? "", t.GetCustomAttribute<DescriptionAttribute>()?.Description })
                    .Where(t => !string.IsNullOrEmpty(t.FullName))
                    .ToList();
               var updateEntityRecords = new List<EntityRecord>();
               var recordsState = new Dictionary<Guid, EntityState>();

               //获取数据库中已有的记录
               var entityRecords = taxi.GetDataSetQuery<EntityRecord>().ToList();

               //处理新增或重新启用的实体
               foreach (var entity in assemblyEntities)
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
                       if (!record.IsActive)
                       {
                           record.IsActive = true;
                           update = true;
                       }

                       if (!string.IsNullOrEmpty(entity.Description) && !entity.Description.Equals(record.Description, StringComparison.CurrentCultureIgnoreCase))
                       {
                           record.Description = entity.Description;
                           update = true;
                       }

                       if (update)
                       {
                           record.EntityName = entity.Name;
                           record.FullName = entity.FullName;
                           updateEntityRecords.Add(record);
                           recordsState[record.Id] = EntityState.Modified;
                       }
                   }
               }

               //处理被删除或不再继承的实体
               foreach (var record in entityRecords.Where(record => assemblyEntities.All(e => e.FullName != record.FullName)))
               {
                   record.IsActive = false;
                   updateEntityRecords.Add(record);
                   recordsState[record.Id] = EntityState.Modified;
               }

               if (updateEntityRecords.Any() && recordsState.Any() && updateEntityRecords.Count == recordsState.Count)
                   taxi.Transaction(updateEntityRecords, recordsState);

               var entityRecord = taxi.GetDataSetQuery<EntityRecord>().Select(er => new { er.Id, er.FullName }).ToList();
               redis?.Set(ConstDictionary.RedisCacheEntityRecord, entityRecord, TimeSpan.FromDays(365));
           });

        }

        public static void CreateAdministratorPromission(this IApplicationBuilder app, IConfiguration configuration)
        {
            if (bool.TryParse(configuration.GetSection("AdministratorPermissionInitial").Value, out var isInitial) && isInitial)
            {
                app.TaxiInvokeAdmin((taxi, _) =>
                {
                    var adminRole = taxi.FirstOrDefault<Role>(role => role.RoleName.ToLower() == "Administrator".ToLower());
                    //为初始管理员创建所有实体的权限
                    if (adminRole == null) return;
                    //管理员现有权限
                    var adminPermissionEntities = taxi.GetDataSetQuery<RolePermission>(rp => rp.RoleId == adminRole.Id).Select(rp => rp.EntityId).ToList();
                    //所有实体记录
                    var entities = taxi.GetDataSetQuery<EntityRecord>(where: er => er.IsActive).ToList();
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
                });
            }
        }

        public static void CreateParts(this IApplicationBuilder app)
        {
            app.TaxiInvokeAdmin((taxi, _) =>
            {
                var count = taxi.Count<Part>();
                var createCount = 250 - count;
                var createLine = 250;
                if (createCount < createLine) return;
                //var categories = new List<PartCategory>()
                //{
                //    new PartCategory()
                //    {
                //        Name = "发动机系统",
                //        Description = "发动机系统",
                //    },
                //    new PartCategory()
                //    {
                //        Name = "制动系统",
                //        Description = "制动系统",
                //    },
                //    new PartCategory()
                //    {
                //        Name = "悬挂系统",
                //        Description = "悬挂系统",
                //    },
                //    new PartCategory()
                //    {
                //        Name = "外饰",
                //        Description = "外饰",
                //    },

                //};
                //var units = new List<PartUnit>()
                //{
                //    new PartUnit()
                //    {
                //        Name = "个",
                //    },
                //    new PartUnit()
                //    {
                //        Name = "片",
                //    },
                //    new PartUnit()
                //    {
                //        Name = "台",
                //    },
                //};
                //taxi.Transaction(categories, EntityState.Added);
                //taxi.Transaction(units, EntityState.Added);

                var categories = taxi.GetDataSetQuery<PartCategory>().ToList();
                var units = taxi.GetDataSetQuery<PartUnit>().ToList();

                if (!categories.Any() || !units.Any()) return;

                // 2. 定义配件名称的随机池，让数据看起来更像汽配
                var partNames = new[] { "滤清器", "制动片", "火花塞", "减震器", "雨刮片", "蓄电池", "正时皮带", "控制臂", "点火线圈", "发电机" };
                var brands = new[] { "博世(Bosch)", "德尔福(Delphi)", "马勒(Mahle)", "采埃孚(ZF)", "电装(Denso)" };

                // 3. 配置 Bogus 生成规则
                var partFaker = new Faker<Part>("zh_CN") // 使用中文数据
                    .RuleFor(p => p.Id, f => Guid.NewGuid())
                    .RuleFor(p => p.PartName, f => f.PickRandom(partNames) + " " + f.Commerce.ProductAdjective())
                    .RuleFor(p => p.OECode, f => f.Random.Replace("OE-##-?????-####").ToUpper()) // 生成像OE码的字符串
                    .RuleFor(p => p.Brand, f => f.PickRandom(brands))
                    .RuleFor(p => p.Model, f => f.Vehicle.Model())
                    .RuleFor(p => p.CategoryId, f => f.PickRandom(categories).Id)
                    .RuleFor(p => p.UnitId, f => f.PickRandom(units).Id)
                    .RuleFor(p => p.CostPrice, f => f.Finance.Amount(50, 500))
                    .RuleFor(p => p.SellingPrice, (f, p) => p.CostPrice * 1.5m) // 售价是进价的1.5倍
                    .RuleFor(p => p.MinStock, f => f.Random.Number(5, 20))
                    .RuleFor(p => p.MaxStock, f => f.Random.Number(100, 500))
                    .RuleFor(p => p.CreatedAt, f => f.Date.Past(1));

                // 4. 生成并保存
                var parts = partFaker.Generate(createCount);
                taxi.Transaction(parts, EntityState.Added);

            });
        }

        public static void RedisCacheRolePermission(this IApplicationBuilder app)
        {
            app.TaxiInvokeAdmin((taxi, redis) =>
            {
                var permissions = taxi.GetDataSetQuery<RolePermission>().Select(rp => new { rp.RoleId, rp.EntityId, rp.CanRead, rp.CanCreate, rp.CanUpdate, rp.CanDelete }).ToList();
                redis?.Set(ConstDictionary.RedisCacheRolePermission, permissions, TimeSpan.FromDays(365));
            });
        }

        public static void CreateSuppliers(this IApplicationBuilder app)
        {
            app.TaxiInvokeAdmin((taxi, _) =>
            {
                var count = taxi.Count<Supplier>();
                var createCount = 20 - count;
                if (createCount <= 0) return;

                var supplierFaker = new Bogus.Faker<Supplier>("zh_CN")
                    .RuleFor(s => s.Id, f => Guid.NewGuid())
                    .RuleFor(s => s.Name, f => f.Company.CompanyName())
                    .RuleFor(s => s.Contact, f => f.Name.FullName())
                    .RuleFor(s => s.Phone, f => f.Phone.PhoneNumber("1##########"))
                    .RuleFor(s => s.Address, f => f.Address.FullAddress())
                    .RuleFor(s => s.CreatedAt, f => f.Date.Past(1));

                var suppliers = supplierFaker.Generate((int)createCount);
                taxi.Transaction(suppliers, EntityState.Added);
            });
        }

        public static void CreateCustomer(this IApplicationBuilder app)
        {
            app.TaxiInvokeAdmin((taxi, _) =>
            {
                var count = taxi.Count<Customer>();
                var createCount = 20 - count;
                if (createCount <= 0) return;

                var customerFaker = new Faker<Customer>("zh_CN")
                    .RuleFor(c => c.Id, f => Guid.NewGuid())
                    .RuleFor(c => c.Name, f => f.Company.CompanyName())
                    .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("1##########"))
                    .RuleFor(c => c.Address, f => f.Address.FullAddress())
                    .RuleFor(c => c.ContactPerson, f => f.Name.FullName())
                    .RuleFor(c => c.Remark, f => f.Lorem.Sentence())
                    .RuleFor(c => c.CreatedAt, f => f.Date.Past(1));

                var customers = customerFaker.Generate((int)createCount);
                taxi.Transaction(customers, EntityState.Added);
            });
        }

        public static void CreateInboundAndOutboundOrders(this IApplicationBuilder app)
        {
            app.TaxiInvokeAdmin((taxi, redis) =>
            {
                const int inboundOrderCount = 20;
                const int itemsPerInbound = 5;
                var now = DateTime.UtcNow.AddDays(-1);
                var todayInCount = taxi.Count<InboundOrder>(where: i => i.InboundDate.DayOfYear == now.DayOfYear);
                var parts = taxi.GetDataSetQuery<Part>().ToList();
                var suppliers = taxi.GetDataSetQuery<Supplier>().ToList();
                var faker = new Faker("zh_CN");

                if (todayInCount < inboundOrderCount && parts.Any() && suppliers.Any())
                {
                    var inboundParts = new List<Part>();
                    // ---------- 生成入库单 ----------

                    var inboundOrders = new List<InboundOrder>();
                    var inboundItems = new List<InboundItem>();

                    for (var i = 0; i < inboundOrderCount - todayInCount; i++)
                    {
                        var order = new InboundOrder
                        {
                            Id = Guid.NewGuid(),
                            OrderNo = redis.AutoNumber(nameof(InboundOrder)),
                            SupplierId = faker.PickRandom(suppliers).Id,
                            TotalAmount = 0m,
                            Remark = "",
                            InboundDate = DateTime.UtcNow
                        };

                        // 随机选择明细配件（允许重复配件跨单）
                        var chosenParts = faker.PickRandom(parts.Where(p => !inboundParts.Select(p2 => p2.Id).Contains(p.Id)), itemsPerInbound);
                        inboundParts.AddRange(chosenParts);
                        foreach (var part in chosenParts)
                        {
                            var qty = Math.Max(part.MinStock, part.MaxStock / 2);
                            var price = part.CostPrice;
                            var item = new InboundItem
                            {
                                Id = Guid.NewGuid(),
                                InboundOrderId = order.Id,
                                PartId = part.Id,
                                Quantity = qty,
                                Price = price,
                                TotalAmount = price * qty,
                            };

                            order.TotalAmount += item.TotalAmount;
                            inboundItems.Add(item);

                        }

                        inboundOrders.Add(order);
                    }

                    // 保存入库单与入库明细并更新配件库存
                    if (inboundOrders.Any())
                        taxi.Transaction(inboundOrders, EntityState.Added);
                    if (inboundItems.Any())
                        taxi.Transaction(inboundItems, EntityState.Added);

                    // ---------- 生成出库单（在入库完成后） ----------
                }

                const int outboundOrderCount = 20;
                const int itemsPerOutbound = 5;
                var todayOutCount = taxi.Count<OutboundOrder>(where: i => i.OutboundDate.DayOfYear == now.DayOfYear);
                var customers = taxi.GetDataSetQuery<Customer>().ToList();
                parts = taxi.GetDataSetQuery<Part>().Where(p => p.Stockpiles > p.MinStock).ToList();

                if (todayOutCount < outboundOrderCount && parts.Any() && customers.Any())
                {
                    var outboundOrders = new List<OutboundOrder>();
                    var outboundItems = new List<OutboundItem>();
                    var outboundParts = new List<Part>();

                    for (var i = 0; i < outboundOrderCount - todayOutCount; i++)
                    {
                        var order = new OutboundOrder
                        {
                            Id = Guid.NewGuid(),
                            OrderNo = redis.AutoNumber(nameof(OutboundOrder)),
                            CustomerId = faker.PickRandom(customers).Id,
                            TotalAmount = 0m,
                            OutboundDate = DateTime.UtcNow,
                            Remark = ""
                        };

                        // 为每张出库单尝试生成指定数量的明细
                        var chosenParts = faker.PickRandom(parts.Where(p => !outboundParts.Select(p2 => p2.Id).Contains(p.Id)), itemsPerOutbound);
                        outboundParts.AddRange(chosenParts);
                        foreach (var part in chosenParts)
                        {
                            // 出库数量：1 到 maxAllowed（不超过当前库的可用量）
                            var quantity = faker.Random.Number(part.Stockpiles / 10, part.Stockpiles / 2);
                            var item = new OutboundItem
                            {
                                Id = Guid.NewGuid(),
                                OutboundOrderId = order.Id,
                                PartId = part.Id,
                                Quantity = quantity,
                                Price = part.SellingPrice,
                                TotalAmount = part.SellingPrice * quantity,
                            };

                            order.TotalAmount += item.TotalAmount;
                            outboundItems.Add(item);

                        }

                        // 只保存有明细的出库单
                        outboundOrders.Add(order);
                    }
                    // 保存出库单与出库明细并更新配件库存
                    if (outboundOrders.Any())
                        taxi.Transaction(outboundOrders, EntityState.Added);
                    if (outboundItems.Any())
                        taxi.Transaction(outboundItems, EntityState.Added);
                }


            });
        }
    }
}
