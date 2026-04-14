using APM.ConTaxi.Permission;
using APM.ConTaxi.Taxi;
using APM.DbEntities;
using APM.IBusiness;
using APM.IServices;
using APM.UtilEntities;
using Microsoft.EntityFrameworkCore;

namespace APM.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IConTaxiService taxi;
        private readonly IJsonWebTokenService _jsonWebTokenService;
        private readonly ITaxiPermission _permission;

        public UserRoleService(IConTaxiService context, IJsonWebTokenService jsonWebTokenService, ITaxiPermission permission)
        {
            taxi = context;
            _jsonWebTokenService = jsonWebTokenService;
            _permission = permission;
        }

        public User CreateUser(User user)
        {
            //检查用户数据
            if (user is null)
                throw new APMException("未找到此用户");

            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.RealName))
                throw new APMException("用户名、真名、密码不可为空");

            user.Id = Guid.NewGuid();
            user.PasswordHash = EncryptUserPassword(user.Password);
            taxi.Transaction(user, EntityState.Added);

            return user;
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

        public UserRole AsignRolesForUser(Guid userId, IEnumerable<Guid> roles)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Role> GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public Role CreateRole(Role role)
        {
            throw new NotImplementedException();
        }
    }
}
