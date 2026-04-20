using APM.ConTaxi.Permission;
using APM.ConTaxi.Taxi;
using APM.DbEntities;
using APM.IBusiness;
using APM.IServices;
using APM.UtilEntities;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;

namespace APM.Services;

public class UserRoleService(
    IConTaxiService taxi,
    IJsonWebTokenService jsonWebTokenService,
    IRedisService redis)
    : IUserRoleService
{

    public User CreateUser(User user)
    {
        if (taxi.FirstOrDefault<User>(u => u.Username == user.Username) != null)
            throw new APMException($"用户名 {user.Username} 已存在");

        user.PasswordHash = EncryptUserPassword(user.PasswordHash);
        user = taxi.Create(user);
        return user;
    }

    public IEnumerable<UserRoleView> GetAllUserRole()
    {
        var userRoleView = taxi.GetDataSetQuery<UserRoleView>(paging: false).ToList();

        return userRoleView;
    }

    public IEnumerable<UserRoleView> GetUserRoles(Guid userId)
    {
        var userRoles = taxi.GetDataSetQuery<UserRoleView>()
            .Where(user => user.UserId == userId);

        return userRoles;
    }

    public string AuthenticateUser(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            throw new APMException($"用户名或密码不可为空");
        var dic = taxi.UserLogin(username);
        var user = dic.Keys.FirstOrDefault();
        if (user is null || string.IsNullOrEmpty(user.PasswordHash) || !VerifyUserPassword(password, user.PasswordHash))
            throw new APMException($"用户信息验证失败");
        var roles = dic[user].Select(role => role.RoleId).ToList();
        var token = jsonWebTokenService.GetUserToken(user, roles);
        redis.Set(user.Id.ToString(), token, TimeSpan.FromDays(30));
        return token;
    }

    public List<UserRole> AsignRolesForUser(Guid userId, IEnumerable<Guid> roles)
    {
        var user = taxi.FirstOrDefault<User>(u => u.Id == userId) ?? throw new APMException($"用户不存在");

        //先删除用户的所有角色
        taxi.Delete<UserRole>(ur => ur.UserId == userId);

        //重新分派角色至用户
        roles = taxi.GetDataSetQuery<Role>(r => roles.Contains(r.Id), paging: false).Select(r => r.Id).ToList();

        var userRoles = roles.Select(r => new UserRole { UserId = user.Id, RoleId = r }).ToList();
        taxi.Create(userRoles);
        return taxi.GetDataSetQuery<UserRole>(ur => ur.UserId == userId, paging: false).ToList();
    }

    public IEnumerable<Role> GetAllRoles()
    {
        throw new NotImplementedException();
    }

    public Role CreateRole(Role role)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 检查 token 是否有效：解析 JWT 检查过期时间 && 与 Redis 中保存的 token 一致
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public bool CheckUserToken(string token)
    {
        return jsonWebTokenService.CheckUserToken(token);
    }

    public User? GetCurrentUser(string token)
    {
        var userId = jsonWebTokenService.GetCurrentUserId(token);
        return taxi.Get<User>(userId);
    }

    private string EncryptUserPassword(string password)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        //加密用户密码
        return BCrypt.Net.BCrypt.HashPassword(password, salt);
    }

    private bool VerifyUserPassword(string password, string passwordHash)
    {
        //验证用户密码
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}