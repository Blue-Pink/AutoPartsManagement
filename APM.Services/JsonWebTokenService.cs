using APM.DbEntities;
using APM.UtilEntities;
using APM.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace APM.Services
{
    public class JsonWebTokenService : IJsonWebTokenService
    {
        //private readonly ILogger<JsonWebTokenService> _logger;
        private readonly JsonWebTokenSetting _setting;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

        public JsonWebTokenService(JsonWebTokenSetting setting, JwtSecurityTokenHandler jwtSecurityTokenHandler)
        {
            //_logger = logger;
            _setting = setting;
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
        }

        public IEnumerable<Claim> GetClaims(User user, List<Guid> roles)
        {
            if (user.Id == Guid.Empty || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.RealName) || string.IsNullOrEmpty(user.PasswordHash))
                throw new APMException("User's Id,Username,RealName,PasswordHash cannot be null or empty.");

            IEnumerable<Claim> claims = [
                new Claim("Id", Guid.NewGuid().ToString()),
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
            var key = Encoding.ASCII.GetBytes(_setting.IssuerSigningKey);
            var expireTime = DateTime.UtcNow.AddDays(30);
            var token = new JwtSecurityToken(issuer: _setting.ValidIssuer,
             audience: _setting.ValidAudience,
              claims: GetClaims(user, roles),
               notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                expires: new DateTimeOffset(expireTime).DateTime,
                 signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));

            return _jwtSecurityTokenHandler.WriteToken(token);
        }
    }
}
