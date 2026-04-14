using APM.DbEntities;
using APM.UtilEntities;
using System.Security.Claims;

namespace APM.IServices
{
    public interface IJsonWebTokenService
    {
        public IEnumerable<Claim> GetClaims(User user, List<Guid> roles);

        public string GetUserToken(User user, List<Guid> roles);
    }
}
