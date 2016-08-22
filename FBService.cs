using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;

namespace GetFBContent
{
    public class FBService
    {
        public static Dictionary<string, int> FilterKeys;

        /// <summary>
        /// 讀取過濾關鍵字
        /// </summary>
        public static void getFilterKey()
        {
            var cmd = new SqlCommand();
            cmd.CommandText = @"
                select * from Keyword
            ";
            DataTable rssFilter = Persister.Execute(cmd);

            FilterKeys = new Dictionary<string, int>();

            foreach (DataRow keyRow in rssFilter.Rows)
            {
                FilterKeys.Add(keyRow["KW_Keyword"].ToString(), Int32.Parse(keyRow["KW_ID"].ToString()));
            }
        }

        /// <summary>
        /// 記錄 FB 資料
        /// </summary>
        public static void insertFB(int dfg, string place_id, string msg, DateTime time, int kwid)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = @"Insert into FB_message (id,place_id, Message,time, KWID,catch_time) values(@id,@place_id,@Message,@time,@KWID,getDate())";

            cmd.Parameters.Add("@id", SqlDbType.VarChar).Value = DateTime.Now.ToString("yyyyMMddhhmmssfff") + dfg.ToString();
            cmd.Parameters.Add("@place_id", SqlDbType.VarChar).Value = place_id;
            cmd.Parameters.Add("@Message", SqlDbType.NVarChar).Value = msg;
            cmd.Parameters.Add("@time", SqlDbType.DateTime).Value = time;
            cmd.Parameters.Add("@KWID", SqlDbType.Int).Value = kwid;
            Persister.ExecuteNonQuery(cmd);
        }
        /// <summary>
        /// 更新 FB 資料
        /// </summary>
        /// <returns></returns>
        public static void updFB(int kwid, string id)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = @"
                UPDATE FB_message SET KWID =@KWID WHERE id =@id
            ";
            cmd.Parameters.Add("@KWID", SqlDbType.Int).Value = kwid;
            cmd.Parameters.Add("@id", SqlDbType.NVarChar).Value = id;
            Persister.ExecuteNonQuery(cmd);
        }
        /// <summary>
        /// 取得 FB ID
        /// </summary>
        /// <param name="num">用id的第一碼去分類</param>
        /// <returns></returns>
        public static DataTable getFBNewID(int num)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = @"
                select top 1 * from FB_place where (complete is null or complete = '' )
                    and substring(id,0,2)=@num
                    --and id not in (select id from FB_place_Nantou)
                    order by id asc
            ";
            cmd.Parameters.Add("@num", SqlDbType.Int).Value = num;
            DataTable theTable = Persister.Execute(cmd);
            if (theTable.Rows.Count > 0)
            {
                return theTable;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 更新 FB place 狀態
        /// </summary>
        /// <param name="type">1: 成功；-1: 失敗(抓html失敗)；-2: 失敗(無法取得html高度)；-3: 失敗(HtmlDocument錯誤)；-99失敗(記憶體不足)</param>
        /// <param name="place_id">FB_place 點位id</param>
        /// <returns></returns>
        public static void updFBplace(int type, string place_id)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = @"
                UPDATE FB_place SET complete =@type, complete_time=getDate() WHERE id =@id
            ";
            cmd.Parameters.Add("@type", SqlDbType.Int).Value = type;
            cmd.Parameters.Add("@id", SqlDbType.NVarChar).Value = place_id;
            Persister.ExecuteNonQuery(cmd);
        }
        /// <summary>
        /// 檢查 FB 文章內容是否在過濾清單中
        /// </summary>
        /// <returns></returns>
        public static int matchTitle(string message)
        {
            foreach (string aStr in FilterKeys.Keys)
            {

                if (message.IndexOf(aStr) >= 0)
                    return FilterKeys[aStr];
            }
            return -1;
        }

        /// <summary>
        /// 檢查該筆 FB 是否已經接收過
        /// </summary>
        /// <param name="id"></param>
        /// <param name="place_id"></param>
        /// <returns></returns>
        private static bool isExist(string id, string place_id)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = @"
                select top 1 id from FB_message where id=@id and place_id=@place_id
            ";
            cmd.Parameters.Add("@id", SqlDbType.NVarChar).Value = id;
            cmd.Parameters.Add("@place_id", SqlDbType.NVarChar).Value = place_id;
            DataTable theTable = Persister.Execute(cmd);

            if (theTable.Rows.Count > 0)
                return true;
            else
                return false;
        }
    }
}
