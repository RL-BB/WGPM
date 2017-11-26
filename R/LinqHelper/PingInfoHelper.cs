using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WGPM.R.RoomInfo;
using WGPM.R.Logger;
using WGPM.R.UI;

namespace WGPM.R.LinqHelper
{
    /// <summary>
    /// 平煤相关信息记录到DB的帮助类
    /// </summary>
    class PingInfoHelper
    {
        public PingInfoHelper() { }
        public PingInfoHelper(DbAppDataContext db, PgInfo pInfo, bool beginRec)
        {
            this.db = db;
            if (!db.DatabaseExists()) return;
            this.pInfo = pInfo;
            AssignmentInsertInfo(beginRec);
        }
        private PgInfo pInfo;
        private DbAppDataContext db;
        private void AssignmentInsertInfo(bool begin)
        {
            PingInfo ping = null;
            if (begin)
            {
                BeginRec(ping);
            }
            else
            {
                ping = db.PingInfo.SingleOrDefault(p => p.T平煤炉号 == pInfo.TPRoom && p.Begin平煤时间 == pInfo.BeginTime);
                if (ping == null)
                {
                    BeginRec(ping);
                }
                EndRec(ping);
            }
        }
        public void RecToDB()
        {
            try
            {
                using (db)
                {
                    if (!db.DatabaseExists()) return;
                    db.SubmitChanges();
                }
            }
            catch (Exception err)
            {
                Log.LogErr.Info("类：PingInfoHelper.cs，方法：RECToDB()；" + err.ToString());
            }

        }
        private void BeginRec(PingInfo ping)
        {
            ping = new PingInfo();
            ping.预定装煤炉号 = pInfo.Room;//计划装煤炉号
            ping.预定出焦时间 = pInfo.PlanPushTime;//计划推焦时间 ，标识位
            ping.Plan平煤时间 = pInfo.PlanPingTime;//计划装煤时间
            ping.T实际炉号 = pInfo.TRoom;
            ping.T平煤炉号 = pInfo.TPRoom;
            ping.M炉号 = pInfo.MRoom;
            ping.T物理地址 = pInfo.TAddr;
            ping.M物理地址 = pInfo.MAddr;
            ping.Begin平煤时间 = pInfo.BeginTime;
            ping.Ping联锁信息 = pInfo.LockInfo;
            ping.P时段 = pInfo.Period;
            ping.G班组 = pInfo.Group;
            ping.PlanIndex = pInfo.PlanIndex;
            ping.PlanCount = pInfo.PlanCount;
            db.PingInfo.InsertOnSubmit(ping);//Insert到数据库对应PingInfo表中
        }
        private void EndRec(PingInfo ping)
        {
            ping.End平煤时间 = pInfo.EndTime;
            ping.Max平煤电流 = pInfo.MaxCur;
            ping.Avg平煤电流 = pInfo.AvgCur;
            ping.Cur平煤电流 = pInfo.CurArr;
            ping.Pole平煤杆长 = pInfo.PoleArr;
        }
        #region 查询相关
        public DataTable Query(DateTime start, DateTime end, byte room)
        {
            //记录查询：
            //1"日期",2 "炉号", 3"T炉号", 4"T炉号", 5"T物理地址",6 "M物理地址", 7"预定装煤时间", 8"实际装煤时间", 9"装煤结束时间", 10"Avg平煤电流",11 "班组", 12"时段" 
            DataTable table = new DataTable("Rec");
            SetCloumnsProperty(table);
            using (db = new DbAppDataContext(Setting.ConnectionStr))
            {
                // 判断是否为按炉号查询 :(room > 0 ? (pInfo.计划炉号 == room || rec.实际炉号 == room) : 1 == 1) 条件表达式返回值为一条语句
                var recs = from rec in db.PingInfo
                           where rec.Begin平煤时间 >= start && rec.Begin平煤时间 < end && (room > 0 ? (rec.预定装煤炉号 == room || rec.T平煤炉号 == room) : 1 == 1)
                           select rec;
                if (recs.Count() <= 0) return null;
                foreach (var p in recs)
                {
                    DataRow r = table.NewRow();
                    int index = 0;
                    //1"日期",2 "炉号", 3"T炉号",5"T物理地址,4"M炉号"",6 "M物理地址", 7"预定装煤时间", 8"实际装煤时间", 9"装煤结束时间", 10"Avg平煤电流",11 "班组", 12"时段" 
                    r[index++] = p.Begin平煤时间.Value.Date.ToString("d");//1日期
                    r[index++] = p.预定装煤炉号.Value.ToString("000");//2炉号
                    r[index++] = p.T平煤炉号 != null ? p.T平煤炉号.Value.ToString("000") : "000";//3 T平煤炉号
                    r[index++] = p.T实际炉号.Value.ToString("000");//4T实际炉号炉号
                    r[index++] = p.T物理地址 != null ? p.T物理地址.Value.ToString() : "0";//6T物理地址
                    r[index++] = p.M炉号.Value.ToString("000");//4M炉号
                    r[index++] = p.M物理地址.Value;//6M物理地址
                    r[index++] = p.Begin平煤时间 != null ? p.Begin平煤时间.Value.ToString("G") : "0";//8实际装煤时间
                    r[index++] = p.End平煤时间 != null ? p.End平煤时间.Value.ToString("G") : "0";//9装煤结束时间
                    r[index++] = p.Plan平煤时间 != null ? p.Plan平煤时间.Value.ToString() : "0";//7预定装煤时间
                    r[index++] = p.Avg平煤电流 != null ? p.Avg平煤电流.Value.ToString("g") : "0";//10Avg平煤电流
                    r[index++] = ByteToPeriod(p.P时段.Value);//12时段：白班，夜班
                    r[index++] = ByteToGroup(p.G班组.Value);//11班组：甲，乙，丙，丁
                    r[index++] = p.PlanIndex != null ? p.PlanIndex.Value : -1;
                    r[index] = p.PlanCount != null ? p.PlanCount.Value : 0;
                    table.Rows.Add(r);
                }
            }
            return table;
        }
        private void SetCloumnsProperty(DataTable dt)
        {
            string[] columnsName = { "日期", "炉号", "T装煤炉号", "T实际炉号", "T物理地址", "M炉号", "M物理地址", "实际装煤时间", "装煤结束时间", "计划装煤时间", "Avg平煤电流", "时段", "班组", "PlanIndex", "PlanCount" };
            //Type[] types = { typeof(string), typeof(byte), typeof(byte), typeof(byte), typeof(int), typeof(int), typeof(int), typeof(int), typeof(DateTime), typeof(DateTime), typeof(DateTime), typeof(byte), typeof(byte), typeof(byte) };
            for (int i = 0; i < columnsName.Length; i++)
            {
                //dt.Columns.Add(columnsName[i], types[i]);
                dt.Columns.Add(columnsName[i], typeof(string));
            }
        }
        private string ByteToPeriod(byte period)
        {//1白，2夜
            string strValue = period <= 1 ? "白班" : "夜班";
            return strValue;
        }
        private string ByteToGroup(byte group)
        {
            string strValue = null;
            switch (group)
            {
                case 1:
                    strValue = "甲班";
                    break;
                case 2:
                    strValue = "乙班";
                    break;
                case 3:
                    strValue = "丙班";
                    break;
                case 4:
                    strValue = "丁班";
                    break;
                default:
                    break;
            }
            return strValue;
        }
        #endregion
    }
    /// <summary>
    /// Pg->Ping,PgInfo->PingInfo
    /// </summary>
    class PgInfo
    {
        public PgInfo() { }
        /// <summary>
        /// 计划装煤炉号
        /// </summary>
        public byte Room { get; set; }//
        public byte TRoom { get; set; }//推焦车实际炉号
        /// <summary>
        /// 推焦车对应的平煤炉号
        /// </summary>
        public byte TPRoom { get; set; }
        public byte MRoom { get; set; }//炉号
        public DateTime PlanPushTime { get; set; }//预定出焦时间
        public DateTime PlanPingTime { get; set; }//预定出焦时间
        public byte Period { get; set; }//时段
        public byte Group { get; set; }//班组
        public int TAddr { get; set; }//平煤时推物理地址
        public int MAddr { get; set; }//煤车物理地址
        public DateTime BeginTime { get; set; }//平煤时间
        public DateTime EndTime { get; set; }//平煤时间
        public int LockInfo { get; set; }
        public byte MaxCur { get; set; }
        public byte AvgCur { get; set; }
        public string CurArr { get; set; }
        public string PoleArr { get; set; }
        public int PlanIndex { get; set; }
        public int PlanCount { get; set; }
    }
}
