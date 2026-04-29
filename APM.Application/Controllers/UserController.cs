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
using APM.DbEntities.Views;

namespace APM.Application.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class UserController(IUserRoleService userRoleService) : APMController
    {
        [HttpGet, Route("[action]")]
        public UsualApiData<UserDTO> GetUsers(int pageIndex, int pageSize, string sortField = "", bool sortDesc = false, string search = "")
        {
            return UsualResult(userRoleService.GetUsers(pageIndex, pageSize, sortField, sortDesc, search));
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns></returns>
        [HttpPost, Route("[action]"), AllowAnonymous]
        public UsualApiData<string?> UserLogin(UserDTO user)
        {
            userRoleService.MatchUserInfo(user.Username ?? "", user.Password ?? "");
            var token = userRoleService.AuthenticateUser(user.Username ?? "", user.Password ?? "");
            return UsualResult(token);
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [HttpPost, Route("[action]"), AllowAnonymous]
        public UsualApiData<UserDTO?> EditUser(UserDTO userDTO)
        {
            var user = userRoleService.EditUser(userDTO);
            return UsualResult(user);
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
        public UsualApiData<UserDTO?> GetCurrentUser()
        {
            var user = userRoleService.GetCurrentUser(UserToken);
            return UsualResult(user);
        }

        [HttpGet, Route("[action]")]
        public UsualApiData<RoleDTO> GetRoles(int pageIndex, int pageSize, string sortField = "", bool sortDesc = false, string search = "")
        {
            return UsualResult(userRoleService.GetRoles(pageIndex, pageSize, sortField, sortDesc, search));
        }

    }
}
