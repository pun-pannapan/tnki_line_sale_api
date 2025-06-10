using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace tnki_line_sale_api.Utilities
{
    public class ConnectionHandle
    {
        public static Guid getCustomerFromTokenIfNull(IHttpContextAccessor httpContextAccessor)
        {
            var identity = (List<ClaimsIdentity>)httpContextAccessor.HttpContext.User.Identities;
            if (identity == null || identity.Count() == 0)
            {
                return Guid.Empty;
            }
            IEnumerable<Claim> claims = identity[identity.Count() - 1].Claims;
            if (claims == null)
            {
                return Guid.Empty;
            }
            Claim cRole = claims.Where(s => s.Type == "role").SingleOrDefault();
            if (cRole == null)
            {
                return Guid.Empty;
            }
            if (!cRole.Value.ToString().Equals("user"))
            {
                throw new UnauthorizedAccessException("unauthorized");
            }
            Claim c = claims.Where(s => s.Type == ClaimTypes.NameIdentifier).SingleOrDefault();
            Guid cust_guid = new Guid(c.Value);
            return cust_guid;
        }
        public static Guid getCustomerFromToken(IHttpContextAccessor httpContextAccessor)
        {
            var identity = (List<ClaimsIdentity>)httpContextAccessor.HttpContext.User.Identities;
            if (identity == null || identity.Count() == 0)
            {
                throw new Exception("unauthorized");
            }
            IEnumerable<Claim> claims = identity[identity.Count() - 1].Claims;
            Claim cRole = claims.Where(s => s.Type == "role").SingleOrDefault();
            if (!cRole.Value.ToString().Equals("user"))
            {
                throw new UnauthorizedAccessException("unauthorized");
            }
            Claim c = claims.Where(s => s.Type == ClaimTypes.NameIdentifier).SingleOrDefault();
            Guid cust_guid = new Guid(c.Value);
            return cust_guid;
        }

        public static Guid getAdminFromToken(IHttpContextAccessor httpContextAccessor)
        {
            var identity = (List<ClaimsIdentity>)httpContextAccessor.HttpContext.User.Identities;
            if (identity == null || identity.Count() == 0)
            {
                throw new Exception("unauthorized");
            }
            IEnumerable<Claim> claims = identity[identity.Count() - 1].Claims;
            Claim cRole = claims.Where(s => s.Type == "role").SingleOrDefault();
            if (!cRole.Value.ToString().Equals("admin"))
            {
                throw new UnauthorizedAccessException("unauthorized");
            }
            Claim c = claims.Where(s => s.Type == ClaimTypes.NameIdentifier).SingleOrDefault();
            Guid userGuid = new Guid(c.Value);
            return userGuid;
        }
        public static void closeConnection(SqlConnection dbCon)
        {
            if (dbCon.State == ConnectionState.Open)
            {
                dbCon.Close();
            }
        }
        public static void openConnection(SqlConnection dbCon)
        {
            if (dbCon.State != ConnectionState.Open)
            {
                dbCon.Open();
            }
        }
    }
}
