using APM.ConTaxi.Permission;
using APM.ConTaxi.Taxi;
using APM.DbEntities;
using APM.IBusiness;
using APM.IServices;
using APM.UtilEntities;
using Microsoft.EntityFrameworkCore;

namespace APM.Services;

public class UserRoleService : IUserRoleService
{
    private readonly IConTaxiService taxi;
    private readonly IJsonWebTokenService _jsonWebTokenService;
    private readonly ITaxiPermission _permission;

    public UserRoleService(IConTaxiService context, IJsonWebTokenService jsonWebTokenService,
        ITaxiPermission permission)
    {
        taxi = context;
        _jsonWebTokenService = jsonWebTokenService;
        _permission = permission;
    }

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
        var userRoleView = taxi.GetDataSetQuery<UserRoleView>();

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
        var token = _jsonWebTokenService.GetUserToken(user, roles);

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