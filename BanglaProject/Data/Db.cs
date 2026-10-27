using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BanglaProject.Data
{
    // All database work of the project goes through this one class.
    // The connection string is written only once, in App.config ("FarmDb").
    // Every method opens a new connection, runs the query and closes it again,
    // and all values are sent as SqlParameters (no string joining = no SQL injection).
    public static class Db
    {
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["FarmDb"].ConnectionString;

        // SELECT query -> rows in a DataTable (used to fill grids and combo boxes)
        public static DataTable GetTable(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = Command(sql, parameters))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Connection = con;
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        // INSERT / UPDATE / DELETE -> number of rows changed
        public static int Execute(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = Command(sql, parameters))
            {
                cmd.Connection = con;
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // Query that returns a single value (COUNT, MAX ...)
        public static object Scalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = Command(sql, parameters))
            {
                cmd.Connection = con;
                con.Open();
                return cmd.ExecuteScalar();
            }
        }

        // Builds a command with its parameters (used together with ExecuteInTransaction)
        public static SqlCommand Command(string sql, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(sql);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            return cmd;
        }

        // Runs several commands together. If one of them fails, nothing is saved.
        // Used when saving or deleting an invoice (details + items + stock).
        public static void ExecuteInTransaction(List<SqlCommand> commands)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        foreach (SqlCommand cmd in commands)
                        {
                            cmd.Connection = con;
                            cmd.Transaction = transaction;
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Makes the next ID for a table, for example "FIM-021", "INV-011" or "PC-011".
        // It reads the biggest number after the prefix and adds 1.
        // numberStart = position where the number begins (default: right after the prefix).
        public static string NextId(string table, string column, string prefix, string likePattern = null, int numberStart = 0)
        {
            if (likePattern == null) likePattern = prefix + "%";
            if (numberStart == 0) numberStart = prefix.Length + 1;

            string sql = "SELECT ISNULL(MAX(CAST(SUBSTRING(" + column + ", " + numberStart + ", 10) AS INT)), 0) " +
                         "FROM " + table + " WHERE " + column + " LIKE @pattern";

            int last = Convert.ToInt32(Scalar(sql, new SqlParameter("@pattern", likePattern)));
            return prefix + (last + 1).ToString("D3");
        }

        // "server / database" from the connection string, shown in error messages
        public static string ServerAndDatabase()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
            return "server: " + builder.DataSource + ", database: " + builder.InitialCatalog;
        }

        // Short way to make a parameter: Db.P("@ID", id)
        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }
    }
}
