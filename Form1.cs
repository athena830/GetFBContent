using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.IO;
using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;
using System.Threading;
using System.Data.SqlClient;
using Microsoft.Win32;

namespace GetFBContent
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.backgroundWorker1.WorkerReportsProgress = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            InitializeComponent();

            webBrowser1.ScriptErrorsSuppressed = true;
            DataTable dt = FBService.getFBNewID();
            place_id = dt.Rows[0]["id"].ToString();
            webBrowser1.Navigate("https://www.facebook.com/pages/" + dt.Rows[0]["name"].ToString() + "/" + place_id);

            waitTillLoad(this.webBrowser1);

        }

        private static DateTime StampToDateTime(string timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime;
            try
            {
                lTime = long.Parse(timeStamp + "0000000");
            }
            catch (Exception ex)
            {
                lTime = long.Parse(timeStamp.Substring(0, timeStamp.Length - 2) + "0000000");
            }
            TimeSpan toNow = new TimeSpan(lTime);

            return dateTimeStart.Add(toNow);
        }

        private void waitTillLoad(WebBrowser webBrControl)
        {
            WebBrowserReadyState loadStatus;
            int waittime = 100000;
            int counter = 0;
            while (true)
            {
                loadStatus = webBrControl.ReadyState;
                Application.DoEvents();
                if ((counter > waittime) || (loadStatus == WebBrowserReadyState.Uninitialized) || (loadStatus == WebBrowserReadyState.Loading) || (loadStatus == WebBrowserReadyState.Interactive))
                {
                    break;
                }
                counter++;
            }

            counter = 0;
            while (true)
            {
                loadStatus = webBrControl.ReadyState;
                Application.DoEvents();
                if (loadStatus == WebBrowserReadyState.Complete && webBrControl.IsBusy != true)
                {
                    break;
                }
                counter++;
            }
        }

        public int rt = 0;
        public string place_id = "";

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.HtmlDocument document = this.webBrowser1.Document;
            document.Window.ScrollTo(0, 1000000000);
            FBService.getFilterKey();
            //if (document.Window.WindowFrameElement != null)
            //{
                if (rt == document.Window.Size.Height)
                {
                    timer1.Stop();
                    try
                    {
                        System.IO.StreamReader getReader = new System.IO.StreamReader(this.webBrowser1.DocumentStream, System.Text.Encoding.Default);
                        string HTML = getReader.ReadToEnd();
                        string Message_Condition_start = "<DIV class=\"_5pbx userContent\"";
                        string Message_Condition_finish = "</P>";
                        string Paragraph_start = "<P>";
                        string time_start = "data-utime=\"";
                        string time_finish = "\"";
                        int time_Length = 0;
                        int dfg = 0;
                        int KeywordID = -1; //關鍵字ID
                        //從0開始搜尋time_start
                        while (HTML.IndexOf(time_start, time_Length) != -1)
                        {
                            int timeStart = HTML.IndexOf(time_start, time_Length);

                            //從0開始搜尋Message_Condition_start
                            //int htmlStart = HTML.IndexOf(Message_Condition_start, time_Length);
                            //從0開始搜尋time_start位置再找time_finish的位置
                            //int htmlend = HTML.IndexOf(time_finish, HTML.IndexOf(time_start, time_Length));

                            //換個想法，從time_start位置找Message_Condition_start
                            int htmlStart = HTML.IndexOf(Message_Condition_start, time_Length);
                            //從Message_Condition_start去找Message_Condition_finish
                            if (htmlStart != -1)
                            {
                                int htmlend = HTML.IndexOf(Message_Condition_finish, htmlStart);
                                if (htmlStart < htmlend)
                                {
                                    DateTime time = StampToDateTime(HTML.Substring(timeStart + time_start.Length, HTML.IndexOf(time_finish, timeStart) - time_finish.Length - timeStart));
                                    string Message = HTML.Substring(htmlStart + Message_Condition_start.Length, HTML.IndexOf(Message_Condition_finish, htmlStart) - Message_Condition_finish.Length - htmlStart);
                                    string msg = Message.Substring(Message.IndexOf(Paragraph_start) + 3, Message.IndexOf(Message_Condition_finish) - Message.IndexOf(Paragraph_start) - 3);

                                    KeywordID = FBService.matchTitle(msg);
                                    FBService.insertFB(dfg, place_id, msg, time, KeywordID);
                                    dfg++;
                                }
                                time_Length = htmlend;
                            }
                            else
                            {
                                break;
                            }
                        }
                        FBService.updFBplace(1, place_id);
                    }
                    catch (Exception ex)
                    {
                        FBService.updFBplace(-1, place_id);
                    }
                    DataTable dt = FBService.getFBNewID();
                    place_id = dt.Rows[0]["id"].ToString();
                    webBrowser1.Navigate("https://www.facebook.com/pages/" + dt.Rows[0]["name"].ToString() + "/" + place_id);
                    timer1.Start();
                }
                else
                {
                    rt = document.Window.Size.Height;
                }
            //}
            //else
            //{
            //    FBService.updFBplace(-1, place_id);
            //    DataTable dt = FBService.getFBNewID();
            //    place_id = dt.Rows[0]["id"].ToString();
            //    webBrowser1.Navigate("https://www.facebook.com/pages/" + dt.Rows[0]["name"].ToString() + "/" + place_id);
            //    timer1.Start();
            //}

        }
    }
}
