using tnki_line_sale_api.Entities;
using System.Data.SqlClient;
using Dapper;
using tnki_line_sale_api.Constant;

namespace tnki_line_sale_api.Repositories
{
    public class UserRepository
    {
        public m_user getUserByNameAndPass(SqlConnection dbCon, string user, string password)
        {
            const string sql = "select * from m_user where user_username  = @user_username and user_password = @user_password and user_status = @user_status";

            return dbCon.QuerySingleOrDefault<m_user>(sql, new
            {
                user_username = user,
                user_password = password,
                user_status = ENUM_STATUS.active
            });
        }
    }
}
