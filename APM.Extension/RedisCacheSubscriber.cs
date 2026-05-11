using APM.DbEntities;
using APM.IServices;
using APM.UtilEntities;
using Microsoft.Extensions.Hosting;
using APM.ConTaxi.Bridger;

namespace APM.Extensions
{
    public class RedisCacheSubscriber(IRedisService redisService) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 角色权限订阅
            redisService.Subscribe(ConstDictionary.RedisCacheRolePermission + ConstDictionary.RedisChannelPrefix, (channel, message) =>
            {
                if (message == ConstDictionary.RedisCacheRolePermission + ConstDictionary.RedisChannelMessage)
                {
                   
                }
            });

            return Task.CompletedTask;
        }
    }
}
