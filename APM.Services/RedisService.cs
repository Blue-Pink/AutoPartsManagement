using APM.IServices;
using APM.UtilEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace APM.Services
{
    public class RedisService : IRedisService
    {
        private readonly IConfiguration _configuration;
        private readonly ConfigurationOptions _redisConfigurationOptions;
        private volatile ConnectionMultiplexer _redisConnectionMultiplexer;
        private readonly Lock _redisConnectionLock = new();
        private readonly ILogger<IRedisService> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            //允许不转义特殊字符（如引号、大于号等）
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true // 读取时忽略大小写
        };

        public RedisService(IConfiguration configuration, ILogger<IRedisService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _redisConfigurationOptions = GetRedisConfiguration();
            _redisConnectionMultiplexer = ConnectionRedis();
            //_logger = logger;
        }

        private ConnectionMultiplexer ConnectionRedis()
        {
            //已连接则直接使用
            if (_redisConnectionMultiplexer is { IsConnected: true })
                return _redisConnectionMultiplexer;

            lock (_redisConnectionLock)
            {
                if (_redisConnectionMultiplexer is { IsConnected: true })
                {
                    _redisConnectionMultiplexer.Dispose();
                }

                try
                {
                    _redisConnectionMultiplexer = ConnectionMultiplexer.Connect(_redisConfigurationOptions);
                }
                catch (Exception exception)
                {
                    _logger.Log(LogLevel.Error, exception, "Error occurred while connecting to Redis.");
                    throw new APMException("连接Redis时发生异常");
                }
            }

            return _redisConnectionMultiplexer;
        }

        private ConfigurationOptions GetRedisConfiguration()
        {
            try
            {
                var sections = _configuration.GetSection("RedisConfiguration");
                var redisConfiguration = new RedisConfiguration()
                {
                    Name = sections.GetSection("Name").Value ?? "",
                    IP = sections.GetSection("IP").Value ?? "",
                    Port = Convert.ToInt32(sections.GetSection("Port").Value ?? "0"),
                    Password = sections.GetSection("Password").Value ?? "",
                    Timeout = Convert.ToInt32(sections.GetSection("Timeout").Value ?? "0"),
                    Db = Convert.ToInt32(sections.GetSection("Db").Value ?? "0"),
                };

                var options = new ConfigurationOptions
                {
                    EndPoints = { { redisConfiguration.IP, redisConfiguration.Port } },
                    ClientName = redisConfiguration.Name,
                    Password = redisConfiguration.Password,
                    ConnectTimeout = redisConfiguration.Timeout,
                    DefaultDatabase = redisConfiguration.Db,
                    AbortOnConnectFail = false
                };

                return options;
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, "Error occurred while getting Redis configuration.");
                throw new APMException("获取Redis配置时发生异常");
            }
        }

        public string Get(string key)
        {
            return _redisConnectionMultiplexer.GetDatabase().StringGet(key).ToString() ?? string.Empty;
        }

        public T? Get<T>(string key)
        {
            var value = Get(key);
            if (string.IsNullOrEmpty(value))
                return default;
            try
            {
                return JsonSerializer.Deserialize<T>(value, JsonOptions);
            }
            catch (JsonException)
            {
                //反序列化失败，返回默认值
                return default;
            }
        }

        public List<T>? GetList<T>(string key)
        {
            var value = Get(key);
            if (string.IsNullOrEmpty(value))
                return null;
            try
            {
                return JsonSerializer.Deserialize<List<T>>(value, JsonOptions);
            }
            catch (JsonException exception)
            {
                _logger.Log(LogLevel.Error, exception, "Error occurred while deserializing Redis value.");
                //反序列化失败，返回默认值
                return null;
            }
        }

        public void Set(string key, dynamic value, TimeSpan timeSpan)
        {
            var db = _redisConnectionMultiplexer.GetDatabase();
            db.Ping();
            if (value != null)
                _redisConnectionMultiplexer.GetDatabase().StringSet(key, JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions), timeSpan);
        }
    }
}
