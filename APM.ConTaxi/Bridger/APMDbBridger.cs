using APM.ConTaxi.Permission;
using APM.ConTaxi.Taxi;
using APM.IServices;
using APM.UtilEntities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using APM.DbEntities;

namespace APM.ConTaxi.Bridger
{
    public static class APMDbBridger
    {
        public static void ConnectAPMDbContext(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> action)
        {
            services.AddDbContext<APMDbContext>(action);
        }

        public static void GetInAPMConTaxi(this IServiceCollection services)
        {
            services.AddScoped<IConTaxiService, ConTaxiService>();
            services.AddScoped<ITaxiPermission, TaxiPermission>();
        }

        public static void TaxiInvoke(this IApplicationBuilder applicationBuilder, Action<IConTaxiService>? action = null)
        {
            var serviceProvider = applicationBuilder.ApplicationServices;
            using var scope = serviceProvider.CreateScope();
            var taxi = scope.ServiceProvider.GetRequiredService<IConTaxiService>();

            if (action != null)
                action.Invoke(taxi);
        }

        public static void TaxiInvokeAdmin(this IApplicationBuilder applicationBuilder, Action<IConTaxiService, IRedisService>? action = null)
        {
            var serviceProvider = applicationBuilder.ApplicationServices;
            using var scope = serviceProvider.CreateScope();
            var taxi = scope.ServiceProvider.GetRequiredService<IConTaxiService>();
            if (taxi is ConTaxiService conTaxiService)
                conTaxiService.UseAdministration = true;

            var redis = serviceProvider.GetRequiredService<IRedisService>();
            var user = taxi.Get<User>(ConstDictionary.AdministratorId);
            var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
            if (user is not null)
            {
                userContext.Impersonation(user);
            }
            
            action?.Invoke(taxi, redis);

        }

        public static void TaxiInvokeAdmin(this IServiceProvider serviceProvider, Action<IConTaxiService, IRedisService>? action = null)
        {
            using var scope = serviceProvider.CreateScope();
            var taxi = scope.ServiceProvider.GetRequiredService<IConTaxiService>();
            if (taxi is ConTaxiService conTaxiService)
                conTaxiService.UseAdministration = true;

            var redis = serviceProvider.GetRequiredService<IRedisService>();
            var user = taxi.Get<User>(ConstDictionary.AdministratorId);
            var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
            if (user is not null)
            {
                userContext.Impersonation(user);
            }

            action?.Invoke(taxi, redis);

        }
    }
}
