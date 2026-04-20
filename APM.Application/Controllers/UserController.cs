using APM.DbEntities;
using APM.UtilEntities;
using APM.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using APM.IBusiness;
using System.ComponentModel.DataAnnotations;
using APM.DbEntities.DTOs;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace APM.Application.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class UserController(IUserRoleService userRoleService) : APMController
    {
        /// <summary>
        /// 获取指定用户的角色信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet, Route("[action]/{userId:guid}")]
        public UsualApiData<UserRoleView> GetUserRoles(Guid userId)
        {
            var userRoles = userRoleService.GetUserRoles(userId);

            return UsualResult(userRoles);
        }

        /// <summary>
        /// 获取所有用户的角色信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("[action]")]
        public UsualApiData<UserRoleView> GetAllUserRole()
        {
            var userRoleView = userRoleService.GetAllUserRole();

            return UsualResult(userRoleView);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns></returns>
        [HttpPost, Route("[action]"), AllowAnonymous]
        public UsualApiData<string?> UserLogin(UserDTO user)
        {
            //用户名必填8-20为仅字母数字与-的组合,密码必填8-20为字母数字英文标点符的组合
            if (string.IsNullOrEmpty(user.Username)
                || !new Regex(@"^[a-zA-Z0-9\-]{8,20}$").IsMatch(user.Username)
                || string.IsNullOrEmpty(user.Password)
                || !new Regex(@"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]{8,20}$").IsMatch(user.Password))
                throw new APMException($"用户名必填8-20为仅字母数字与-的组合,密码必填8-20为字母数字英文标点符的组合");


            var token = userRoleService.AuthenticateUser(user.Username, user.Password);
            return UsualResult(token);
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost, Route("[action]"), AllowAnonymous]
        public UsualApiData<List<UserRole>?> CreateUser(UserDTO user)
        {
            if (user is null
                || string.IsNullOrEmpty(user.Username)
                || string.IsNullOrEmpty(user.Realname)
                || string.IsNullOrEmpty(user.Password)
                || user.UserRoleIds is null
                || !user.UserRoleIds.Any())
                throw new APMException($"请保证用户信息完整并为其分配角色");

            var createUser = new User
            {
                Username = user.Username,
                Realname = user.Realname,
                PasswordHash = user.Password,
            };

            createUser = userRoleService.CreateUser(createUser);

            return UsualResult(userRoleService.AsignRolesForUser(createUser.Id, user.UserRoleIds));
        }

        /// <summary>
        /// 前端用于周期性检查 token 是否有效（不包含鉴权中间件对签名的深度验证，仅检查过期与与服务器存储的一致性）
        /// 前端请在请求头中传递 Authorization: Bearer &lt;token&gt;
        /// </summary>
        [HttpGet, Route("[action]")]
        public UsualApiData<bool> CheckUserToken()
        {
            var state = userRoleService.CheckUserToken(UserToken);
            return UsualResult(state);
        }

        /// <summary>
        /// 获取当前已认证用户的信息
        /// </summary>
        [HttpGet, Route("[action]")]
        public UsualApiData<User?> GetCurrentUser()
        {
            var user = userRoleService.GetCurrentUser(UserToken);
            return UsualResult(user);
        }

    }
}
