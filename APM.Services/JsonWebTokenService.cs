using APM.DbEntities;
using APM.IServices;
using APM.UtilEntities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace APM.Services
{
    public class JsonWebTokenService(
        JsonWebTokenSetting setting,
        JwtSecurityTokenHandler jwtSecurityTokenHandler,
        IRedisService redis)
        : IJsonWebTokenService
    {
        //private readonly ILogger<JsonWebTokenService> _logger;

        //_logger = logger;

        public IEnumerable<Claim> GetClaims(User user, List<Guid> roles)
        {
            if (user.Id == Guid.Empty || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Realname) || string.IsNullOrEmpty(user.PasswordHash))
                throw new APMException("User's Id,Username,Realname,PasswordHash cannot be null or empty.");

            IEnumerable<Claim> claims = [
                new Claim("Id", user.Id.ToString()),
                new Claim("Roles", JsonSerializer.Serialize(roles)),
                new Claim(ClaimTypes.Name, $"{user.Id}"),
                new Claim(ClaimTypes.Anonymous, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddDays(30).ToString("MMM ddd dd yyyy HH:mm:ss tt"))
            ];
            return claims;
        }

        public string GetUserToken(User user, List<Guid> roles)
        {
            var key = Encoding.ASCII.GetBytes(setting.IssuerSigningKey);
            var expireTime = DateTime.UtcNow.AddDays(30);
            var token = new JwtSecurityToken(issuer: setting.ValidIssuer,
             audience: setting.ValidAudience,
              claims: GetClaims(user, roles),
               notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                expires: new DateTimeOffset(expireTime).DateTime,
                 signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));

            return jwtSecurityTokenHandler.WriteToken(token);
        }

        public bool CheckUserToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var jwt = jwtSecurityTokenHandler.ReadJwtToken(token);

                // 检查过期时间（使用 UTC）
                if (jwt.ValidTo < DateTime.UtcNow)
                    return false;

                // 尝试获取用户 Id（生成 token 时设置了 ClaimTypes.NameIdentifier 为用户 Id）
                var idClaim = jwt.Claims.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier or "Id");
                if (idClaim is null || string.IsNullOrEmpty(idClaim.Value))
                    return false;

                var userId = idClaim.Value;
                var stored = redis.Get(userId);

                // Redis 中可能以 JSON 字符串形式存储（如 "\"token...\""），尝试去掉包裹的引号
                if (stored.Length >= 2 && stored.StartsWith("\"") && stored.EndsWith("\""))
                {
                    stored = stored.Substring(1, stored.Length - 2);
                }

                // 与 Redis 中保存的 token 比较
                return !string.IsNullOrEmpty(stored) && stored == token;
            }
            catch
            {
                // 解析异常或其它问题，视为无效
                return false;
            }
        }

        public Guid GetCurrentUserId(string token)
        {
            var jwt = jwtSecurityTokenHandler.ReadJwtToken(token);
            var idClaim = jwt.Claims.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier or "Id");
            if (idClaim is null || string.IsNullOrEmpty(idClaim.Value))
                return Guid.Empty;

            return Guid.TryParse(idClaim.Value, out var userId) ? userId : Guid.Empty;
        }
    }
}
