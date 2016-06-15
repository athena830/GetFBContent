using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;

namespace WindowsForms
{
    class Mysql_Class
    {
        public static MySqlConnection MyOpenConn(string Server, string Database, string dbuid, string dbpwd)
        {
            string cnstr = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180; CharSet=utf8", Server, Database, dbuid, dbpwd);
            MySqlConnection icn = new MySqlConnection();
            icn.ConnectionString = cnstr;
            if (icn.State == ConnectionState.Open) icn.Close();
            icn.Open();
            return icn;
        }

        public static DataTable GetMyDataTable(string Server, string Database, string dbuid, string dbpwd, string SqlString)
        {
            DataTable myDataTable = new DataTable();
            MySqlConnection icn = null;
            icn = MyOpenConn(Server, Database, dbuid, dbpwd);
            MySqlCommand isc = new MySqlCommand();
            MySqlDataAdapter da = new MySqlDataAdapter(isc);
            isc.Connection = icn;
            isc.CommandText = SqlString;
            isc.CommandTimeout = 600;
            DataSet ds = new DataSet();
            ds.Clear();
            da.Fill(ds);
            myDataTable = ds.Tables[0];
            if (icn.State == ConnectionState.Open) icn.Close();
            return myDataTable;
        }

        public static void MySqlInsertUpdateDelete(string Server, string Database, string dbuid, string dbpwd, string SqlSelectString)
        {
            MySqlConnection icn = MyOpenConn(Server, Database, dbuid, dbpwd);
            MySqlCommand cmd = new MySqlCommand(SqlSelectString, icn);
            MySqlTransaction mySqlTransaction = icn.BeginTransaction();
            try
            {
                cmd.Transaction = mySqlTransaction;
                cmd.ExecuteNonQuery();
                mySqlTransaction.Commit();
            }
            catch (Exception ex)
            {
                mySqlTransaction.Rollback();
                throw (ex);
            }
            if (icn.State == ConnectionState.Open) icn.Close();
        }
    }
}
