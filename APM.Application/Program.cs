using APM.Business;
using APM.ConTaxi.Bridger;
using APM.UtilEntities;
using APM.Extensions;
using APM.Extensions.Interceptor;
using APM.IServices;
using APM.Services;
using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using APM.IBusiness;
using APM.Extensions.Filter;

namespace APM.Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var allowedHosts = builder.Configuration.GetSection("AllowedHosts").Value ?? "";
            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: allowedHosts,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:5173") // Vue 的默认端口
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.AllowTrailingCommas = true;
                }); ;
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.ConnectAPMDbContext(optionsBuilder =>
            {
                optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
            });
            builder.Services.GetInAPMConTaxi();

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(ConstDictionary.Bearer, document)] = []
                });
            });

            builder.Services.AddSingleton(new ProxyGenerator());
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddSingleton<IRedisService, RedisService>();
            builder.Services.AddScoped<IInterceptor, APMExtensionInterceptor>();

            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddLogger());

            builder.Services.AddJsonWebTokenService(builder.Configuration);

            builder.Services.AddProxiedScoped<IJsonWebTokenService, JsonWebTokenService>();

            builder.Services.AddScoped<APMActionFilter>();

            builder.Services.AddProxiedScoped<IUsualEntityService, UsualEntityService>();
            builder.Services.AddProxiedScoped<IUserRoleService, UserRoleService>();
            builder.Services.AddProxiedScoped<IPartService, PartService>();

            var app = builder.Build();

            app.UseGlobalExceptionHandler();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {

            }

            app.UseHttpsRedirection();

            app.UseCors(allowedHosts);

            app.UseAuthorization();

            app.UseAuthentication();

            app.MapControllers();

            app.MigrationAndSyncEntities();

            app.CreateAdministratorPromission(builder.Configuration);

            app.RedisCacheRolePermission();

            app.CreateParts();

            app.Run();
        }
    }
}
