using APM.DbEntities;
using APM.UtilEntities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;
using APM.DbEntities.DTOs;

namespace APM.IBusiness
{
    public interface IUserRoleService
    {
        /// <summary>
        /// 指派角色至用户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public void AsignRolesForUser(Guid userId, IEnumerable<Guid> roles);

        /// <summary>
        /// 验证用户身份
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>Token</returns>
        public string AuthenticateUser(string userName, string password);

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <returns></returns>
        public UserDTO EditUser(UserDTO userDTO);

        /// <summary>
        /// 检查前端传入的 token 是否仍然有效（未过期且与服务器端 Redis 中保存的 token 一致）
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool CheckUserToken(string token);

        public UserDTO GetCurrentUser(string token);

        public PagingData<UserDTO> GetUsers(int pageIndex, int pageSize, string sortField = "", bool sortDesc = false, string search = "");

        public PagingData<RoleDTO> GetRoles(int pageIndex, int pageSize, string sortField = "", bool sortDesc = false,
            string search = "");

        /// <summary>
        /// 用户名必填8-30位仅字母数字与-的组合,密码必填8-30位字母数字英文标点符的组合
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void MatchUserInfo(string username, string password);
    }
}
