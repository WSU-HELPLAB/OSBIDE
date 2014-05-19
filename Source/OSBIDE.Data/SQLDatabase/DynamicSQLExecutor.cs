using System.Configuration;
using System.Data.SqlClient;

namespace OSBIDE.Data.SQLDatabase
{
    public class DynamicSQLExecutor
    {
        public static void Execute(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return;

            using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["OsbideAdmin"].ConnectionString))
            {
                // should add retries
                sqlConnection.Open();
                (new SqlCommand(sql, sqlConnection)).ExecuteNonQuery();
            }
        }
    }
}
