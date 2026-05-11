using System.Security.Claims;
using System.Text.Json;
using APM.DbEntities;
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

    public Guid UserId => Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirst(ConstDictionary.JwtClaimsUserId)?.Value, out var id) ? id : ImpersonatedUser?.Id ?? Guid.Empty;

    public string? Username => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? ImpersonatedUser?.Username;

    public string? Realname => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Anonymous)?.Value ?? ImpersonatedUser?.Realname;

    public IEnumerable<Guid>? RoleIds { get; }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    private User? ImpersonatedUser { get; set; }

    public void Impersonation(User user)
    {
        ImpersonatedUser = user;
    }
}