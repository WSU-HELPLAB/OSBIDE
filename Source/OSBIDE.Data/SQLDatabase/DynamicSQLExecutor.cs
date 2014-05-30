using System;
using System.Configuration;
using System.Data.SqlClient;
using OSBIDE.Library;

namespace OSBIDE.Data.SQLDatabase
{
    public class DynamicSQLExecutor
    {
        public static void Execute(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return;

            using (var sqlConnection = new SqlConnection(StringConstants.WebConnectionString))
            {
                sqlConnection.Open();
                try
                {
                (new SqlCommand(sql, sqlConnection)).ExecuteNonQuery();
            }
                catch (SqlException ex)
                {
                    throw new Exception(string.Format("Error updating database, please check schema. Error Code: {0}{1}", ex.ErrorCode, ex.InnerException == null ? string.Empty : string.Format(". {0}", ex.InnerException.Message)));
                }
            }
        }
    }
}
