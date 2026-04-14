using APM.DbEntities;
using APM.Extensions.Interceptor;
using System;
using System.Collections.Generic;
using System.Text;

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
        public UserRole AsignRolesForUser(Guid userId, IEnumerable<Guid> roles);

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Role> GetAllRoles();

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public Role CreateRole(Role role);

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
        public User CreateUser(User user);

        /// <summary>
        /// 获取指定用户的角色信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<UserRoleView> GetUserRoles(Guid userId);

        /// <summary>
        /// 获取所有用户的角色信息
        /// </summary>
        /// <returns></returns>
        [APMExtensionInterceptor.Monitor]
        public IEnumerable<UserRoleView> GetAllUserRole();


    }
}
