using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace GetFBContent
{
    public class Persister
    {
        public static string DefaultDataSource { get; private set; }

        static Persister()
        {
            DefaultDataSource = "Data Source=ATHENA;Initial Catalog=NLSCFB;User ID=sa;Password=athena@6786";
        }

        public static int ExecuteNonQuery(SqlCommand command)
        {
            return ExecuteNonQuery(command, DefaultDataSource);
        }

        public static int ExecuteNonQuery(SqlCommand command, string connectionString)
        {
            int affect = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                command.Connection = connection;

                connection.Open();
                affect = command.ExecuteNonQuery();
            }
            return affect;
        }

        public static DataTable Execute(SqlCommand command)
        {
            return Execute(command, DefaultDataSource);
        }

        public static DataTable Execute(SqlCommand command, string connectionString)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                command.Connection = connection;

                connection.Open();
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    dt.BeginLoadData();
                    adapter.Fill(dt);
                    dt.EndLoadData();
                }
            }
            return dt;
        }

        public static int ExecuteScalar(SqlCommand command)
        {
            return Execute_int(command, DefaultDataSource);
        }

        //public static int ExecuteScalar(SqlCommand command, string connectionString)
        //{
        //    var dt = Execute(command, connectionString);
        //    return dt.Rows[0][0].ToInt();
        //}

        public static int Execute_int(SqlCommand command, string connectionString)
        {
            int dataTable = new int();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                command.Connection = sqlConnection;
                sqlConnection.Open();
                dataTable = command.ExecuteNonQuery();
            }
            return dataTable;
        }

    }
}
