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
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using APM.DbEntities.DTOs;
using APM.DbEntities.Views;

namespace APM.Services;

public partial class UserRoleService(
    IConTaxiService taxi,
    IJsonWebTokenService jsonWebTokenService,
    IRedisService redis)
    : IUserRoleService
{

    public UserDTO EditUser(UserDTO userDTO)
    {
        if (userDTO.Id is null || userDTO.Id.Value == Guid.Empty)
        {
            if (userDTO?.IsActive is null
                || string.IsNullOrEmpty(userDTO.Username)
                || string.IsNullOrEmpty(userDTO.Realname)
                || string.IsNullOrEmpty(userDTO.Password))
                throw new APMException($"请检查用户信息完整性");

            MatchUserInfo(userDTO.Username, userDTO.Password);

            //判断用户名是否已存在
            var user = taxi.FirstOrDefault<User>(u => u.Username == userDTO.Username);
            if (user != null)
                throw new APMException($"用户名 {userDTO.Username} 已存在");

            if (userDTO.Roles is null || !userDTO.Roles.Any())
                throw new APMException($"新用户必须指派至少一个角色");

            user = new User
            {
                Username = userDTO.Username,
                Realname = userDTO.Realname,
                PasswordHash = EncryptUserPassword(userDTO.Password),
            };

            user = taxi.Create(user);

            AsignRolesForUser(user.Id, userDTO.Roles.Select(r => r.Id ?? Guid.Empty).Where(id => id != Guid.Empty));

            var roleDTOs = taxi.GetDataSetQuery<UserRoleView>(where: urv => urv.UserId == user.Id, paging: false)
                .Select(urv => new RoleDTO { RoleName = urv.RoleName, Id = urv.RoleId, Description = urv.RoleDescription }).ToList();

            return new UserDTO
            {
                IsActive = user.IsActive,
                Realname = user.Realname,
                CreatedAt = user.CreatedAt,
                ModifiedAt = user.ModifiedAt,
                Roles = roleDTOs,
            };
        }
        else
        {
            if (userDTO?.IsActive is null
                || string.IsNullOrEmpty(userDTO.Username)
                || string.IsNullOrEmpty(userDTO.Realname))
                throw new APMException($"请检查用户信息完整性");

            var user = taxi.Get<User>(userDTO.Id.Value);

            if (user is null)
                throw new APMException($"用户数据丢失");

            user.Realname = userDTO.Realname;
            user.IsActive = userDTO.IsActive.Value;
            if (!userDTO.IsActive.Value)
                redis.Delete(user.Id.ToString());
            if (!string.IsNullOrEmpty(userDTO.Password))
                user.PasswordHash = EncryptUserPassword(userDTO.Password);

            user = taxi.Update(user);

            if (userDTO.Roles != null && userDTO.Roles.Any())
                AsignRolesForUser(user.Id, userDTO.Roles.Select(r => r.Id ?? Guid.Empty).Where(id => id != Guid.Empty));

            var roleDTOs = taxi.GetDataSetQuery<UserRoleView>(where: urv => urv.UserId == user.Id, paging: false)
                .Select(urv => new RoleDTO { RoleName = urv.RoleName, Id = urv.RoleId, Description = urv.RoleDescription }).ToList();

            return new UserDTO
            {
                IsActive = user.IsActive,
                Realname = user.Realname,
                CreatedAt = user.CreatedAt,
                ModifiedAt = user.ModifiedAt,
                Roles = roleDTOs,
            };
        }
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

    public void AsignRolesForUser(Guid userId, IEnumerable<Guid> roles)
    {
        if (!roles.Any())
            return;

        var user = taxi.FirstOrDefault<User>(u => u.Id == userId) ?? throw new APMException($"用户不存在");

        //先删除用户的所有角色
        taxi.Delete<UserRole>(ur => ur.UserId == userId);

        //重新分派角色至用户
        roles = taxi.GetDataSetQuery<Role>(r => roles.Contains(r.Id), paging: false).Select(r => r.Id).ToList();

        var userRoles = roles.Select(r => new UserRole { UserId = user.Id, RoleId = r }).ToList();
        taxi.Create(userRoles);
    }

    public PagingData<RoleDTO> GetRoles(int pageIndex, int pageSize, string sortField = "", bool sortDesc = false,
        string search = "")
    {
        var roles = taxi.GetDataSetQuery<Role>(pageIndex: pageIndex, pageSize: pageSize).Select(r => new RoleDTO
        {
            RoleName = r.RoleName,
            Id = r.Id,
            Description = r.Description,
            CreatedAt = r.CreatedAt,
            ModifiedAt = r.ModifiedAt,
        }).ToList();
        var total = taxi.Total<Role>();
        return new PagingData<RoleDTO>(roles, total, pageIndex, pageSize);
    }

    public void MatchUserInfo(string username, string password)
    {
        if (string.IsNullOrEmpty(username)
            || !UsernameRegex().IsMatch(username)
            || string.IsNullOrEmpty(password)
            || !PasswordRegex().IsMatch(password))
            throw new APMException($"用户名必填8-30位仅字母数字与-的组合,密码必填8-30位字母数字英文标点符的组合");
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

    public UserDTO GetCurrentUser(string token)
    {
        var userId = jsonWebTokenService.GetCurrentUserId(token);
        return taxi.GetCurrentUser(userId);
    }

    public PagingData<UserDTO> GetUsers(int pageIndex, int pageSize, string sortField = "", bool sortDesc = false,
        string search = "")
    {
        Expression<Func<User, bool>> where = !string.IsNullOrEmpty(search)
            ? u => u.IsActive && (u.Username.Contains(search) || u.Realname.Contains(search))
            : u => u.IsActive;
        var total = taxi.Total(where);
        var users = taxi.GetDataSetQuery(where: where, pageIndex, pageSize)
            .Include(u => u.UserRoles!)
            .ThenInclude(ur => ur.Role)
            .Select(u => new UserDTO
            {
                Id = u.Id,
                Username = u.Username,
                Realname = u.Realname,
                Roles = u.UserRoles.Select(ur => new RoleDTO
                {
                    RoleName = ur.Role == null ? "" : ur.Role.RoleName,
                    Description = ur.Role == null ? "" : ur.Role.Description,
                    Id = ur.RoleId
                }).ToList()
            })
            .ToList();

        return new PagingData<UserDTO>(users, total, pageIndex, pageSize);
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

    [GeneratedRegex(@"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]{8,30}$")]
    private static partial Regex PasswordRegex();
    [GeneratedRegex(@"^[a-zA-Z0-9\-]{8,30}$")]
    private static partial Regex UsernameRegex();
}