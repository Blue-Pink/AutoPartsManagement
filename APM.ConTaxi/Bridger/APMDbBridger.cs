using APM.ConTaxi.Permission;
using APM.ConTaxi.Taxi;
using APM.IServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace APM.ConTaxi.Bridger
{
    public static class APMDbBridger
    {
        public static void ConnectAPMDbContext(this IServiceCollection services, Action<DbContextOptionsBuilder> action)
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
        public static void TaxiInvokeAdmin(this IApplicationBuilder applicationBuilder, Action<IConTaxiService, IRedisService?>? action = null)
        {
            var serviceProvider = applicationBuilder.ApplicationServices;
            using var scope = serviceProvider.CreateScope();
            var taxi = scope.ServiceProvider.GetRequiredService<IConTaxiService>();
            var redis = serviceProvider.GetService<IRedisService>();
            if (taxi is ConTaxiService conTaxiService)
                conTaxiService.UseAdministration = true;
            action?.Invoke(taxi, redis);
        }

    }
}
