using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using WGPM.Properties;
using WGPM.R.UI;

namespace WGPM.R.LinqHelper
{
    class ActivateInfo
    {
        public ActivateInfo(DateTime time, bool active)
        {
            this.time = time;
            this.active = active;
        }
        public ActivateInfo(DateTime start, DateTime end)
        {
            this.start = start;
            this.end = end;
        }
        public static string errStr;
        DateTime time;
        DateTime start;
        DateTime end;
        bool active;
        DbAppDataContext db;
        Activate act = new Activate();
        public void RecToDB()
        {
            //true为启动,false为退出
            act.启动 = active ? (byte)1 : (byte)0;
            act.退出 = active ? (byte)0 : (byte)1;
            act.时间 = time;
            act.IP地址 = GetLocalIP();
            try
            {
                using (db = new DbAppDataContext(Setting.ConnectionStr))
                {
                    if (!db.DatabaseExists()) return;
                    db.Activate.InsertOnSubmit(act);
                    db.CommandTimeout = 30;
                    db.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                errStr = ex.ToString();
                return;
            }
        }
        public DataTable QueryLogIn()
        {
            DataTable table = new DataTable();
            string[] columnsName = { "日期", "记录", "时间", "IP地址" };
            for (int i = 0; i < columnsName.Length; i++)
            {
                table.Columns.Add(columnsName[i], typeof(string));
            }
            using (db = new DbAppDataContext(Setting.ConnectionStr))
            {
                if (!db.DatabaseExists()) return null;
                var logs = from p in db.Activate
                           where p.时间 >= start && p.时间 < end
                           select p;
                if (logs.Count() <= 0) return null;
                List<Activate> lstLog = logs.ToList();
                logs = logs.OrderBy(p => p.时间);//按预定出焦时间排序
                foreach (var p in logs)
                {
                    DataRow r = table.NewRow();
                    r[0] = p.时间.Value.ToShortDateString();//日期
                    r[1] = p.启动.Value == 1 ? "登陆" : "退出";//登陆or退出
                    r[2] = p.时间.Value.ToString("G");//实际发生时间
                    r[3] = p.IP地址.ToString();
                    table.Rows.Add(r);
                }
                return table;
            }

        }
        private string GetLocalIP()
        {
            string ip = string.Empty;
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            for (int i = 0; i < ips.Length; i++)
            {
                if (ips[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ip = ips[i].ToString();
                    break;
                }
            }
            return ip;
        }
    }
}
