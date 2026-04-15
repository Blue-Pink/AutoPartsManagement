using APM.DbEntities;
using APM.UtilEntities;
using APM.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using APM.IBusiness;
using System.ComponentModel.DataAnnotations;

namespace APM.Application.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class UserController : APMController
    {
        private readonly IUserRoleService _userRoleService;

        public UserController(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

        /// <summary>
        /// 获取指定用户的角色信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet, Route("[action]/{userId:guid}")]
        public UsualApiData<UserRoleView> GetUserRoles(Guid userId)
        {
            var userRoles = _userRoleService.GetUserRoles(userId);

            return UsualResult(userRoles);
        }

        /// <summary>
        /// 获取所有用户的角色信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("[action]")]
        public UsualApiData<UserRoleView> GetAllUserRole()
        {
            var userRoleView = _userRoleService.GetAllUserRole();

            return UsualResult(userRoleView);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        [HttpPost, Route("[action]"), AllowAnonymous]
        public UsualApiData<string> UserLogin(string username, string password)
        {
            var token = _userRoleService.AuthenticateUser(username, password);
            return UsualResult(token);
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        [HttpPost, Route("[action]"), AllowAnonymous]
        public UsualApiData<List<UserRole>> CreateUser(User user)
        {
            user = _userRoleService.CreateUser(user);

            return UsualResult(_userRoleService.AsignRolesForUser(user.Id, user.UserRoles.Select(ur => ur.RoleId).ToList()));
        }
    }
}
