using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using APM.IServices;
using APM.UtilEntities;

namespace APM.Services;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        RoleIds = JsonSerializer.Deserialize<IEnumerable<Guid>>(
            _httpContextAccessor.HttpContext?.User?.FindFirst(ConstDictionary.JwtClaimsRoleIds)?.Value ?? "[]");
    }

    // 动态从当前 HTTP 上下文的 Claims 中抓取 ID
    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ConstDictionary.JwtClaimsUserId)?.Value;
            return Guid.TryParse(userIdClaim, out var id) ? id : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public string? Realname => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Anonymous)?.Value;

    public IEnumerable<Guid>? RoleIds { get; }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}