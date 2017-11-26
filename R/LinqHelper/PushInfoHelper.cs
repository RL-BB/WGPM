using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WGPM.R.OPCCommunication;
using WGPM.R.RoomInfo;
using System.Data;
using WGPM.R.Vehicles;
using System.Reflection;
using System.Collections;
using WGPM.R.UI;
using WGPM.R.Logger;

namespace WGPM.R.LinqHelper
{
    class PushInfoHelper
    {
        public PushInfoHelper() { }
        public PushInfoHelper(DbAppDataContext db, PsInfo pHelper, bool insert)
        {
            this.db = db;
            this.insert = insert;
            this.pHelper = pHelper;
            AssignmentInsertInfo();
        }
        private PsInfo pHelper;
        private bool insert;
        DbAppDataContext db;
        /// <summary>
        /// 从DB中按条件查询联锁信息
        /// </summary>
        /// <param name="start">查询起始时间</param>
        /// <param name="end">查询终止时间</param>
        /// <param name="room">按炉号查询时，room>0</param>
        /// <returns></returns>
        public DataTable QueryRec(DateTime start, DateTime end, byte room)
        {
            //记录查询：
            //0日期,1计划炉号,2实际炉号,3联锁,***T-L-X车号,4推物理地址,5拦物理地址,6熄物理地址,7煤物理地址,8实际推,9预定推,10实际装,11实际结焦,12Max推焦电流,13Avg推焦电流,14Avg平煤电流,15时段,16班组
            DataTable table = new DataTable("Rec");
            SetCloumnsName(table, true);
            using (db = new DbAppDataContext(Setting.ConnectionStr))
            {
                if (!db.DatabaseExists()) return null;
                // 判断是否为按炉号查询 :(room > 0 ? (pInfo.计划炉号 == room || rec.实际炉号 == room) : 1 == 1) 条件表达式返回值为一条语句
                var recs = from rec in db.PushInfo
                           where rec.预定出焦时间 >= start && rec.预定出焦时间 < end && (room > 0 ? (rec.实际炉号 == room || rec.计划炉号 == room) : 1 == 1)
                           select rec;
                if (recs == null || recs.Count() <= 0) return null;
                foreach (var p in recs)
                {
                    DataRow r = table.NewRow();
                    int index = 0;
                    r[index++] = p.预定出焦时间.Value.Date.ToString("d");//0日期
                    r[index++] = p.计划炉号.Value.ToString("000");//1计划炉号
                    r[index++] = p.T炉号 != null ? p.T炉号.Value.ToString("000") : "0";
                    r[index++] = p.L炉号 != null ? p.L炉号.Value.ToString("000") : "0";
                    r[index++] = p.X炉号 != null ? p.X炉号.Value.ToString("000") : "0";
                    r[index++] = p.TAddr != null ? p.TAddr.Value.ToString() : "0";
                    r[index++] = p.LAddr != null ? p.LAddr.Value.ToString() : "0";
                    r[index++] = p.XAddr != null ? p.XAddr.Value.ToString() : "0";
                    r[index++] = p.实际推焦时间 != null ? p.实际推焦时间.Value.ToString("G") : " ";//8实际推焦时间
                    r[index++] = p.预定出焦时间 != null ? p.预定出焦时间.Value.ToString("g") : " ";//9预定出焦时间
                    r[index++] = p.实际推焦时间 != null ? IntToTimeFormat(((int)(p.实际推焦时间.Value - p.上次装煤时间.Value).TotalMinutes)) : "19:00";//实际结焦时间
                    r[index++] = p.Max推焦电流 != null ? p.Max推焦电流.Value.ToString("000") : "0";//Max推焦电流
                    r[index++] = ByteToPeriod(p.时段.Value);//时段：白班，夜班
                    r[index++] = ByteToGroup(p.班组.Value);//班组：甲，乙，丙，丁
                    r[index++] = ByteToLockStatus(p.联锁.Value);//3联锁
                    r[index++] = p.Push工作车序列 != null ? p.Push工作车序列.Value.ToString("0-0-0") : "0-0-0";
                    r[index++] = p.PlanIndex != null ? p.PlanIndex.Value : -1;
                    r[index++] = p.PlanCount != null ? p.PlanCount.Value : 0;
                    table.Rows.Add(r);
                }
            }
            return table;
        }
        private string IntToTimeFormat(int? time)
        {
            string str = null;
            if (time != null)
            {
                str = (time / 60).Value.ToString("00") + ":" + (time % 60).Value.ToString("00");
            }
            return str;
        }
        /// <summary>
        /// 给DataTable的列名赋值
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="flag">true表示要返回推焦记录的列名，false返回联锁数据的列名</param>
        private void SetCloumnsName(DataTable dt, bool flag)
        {
            string[] columnsName1 = { "日期", "炉号", "T炉号", "L炉号", "X炉号", "T物理地址", "L物理地址", "X物理地址", "实际推焦时间", "预定出焦时间", "实际结焦时间", "Max电流", "时段", "班组", "联锁", "车号:T-L-X", "PlanIndex", "PlanCount" };
            string[] columnsName2 = { "日期", "炉号", "预定出焦时间", "实际出焦时间", "计划装煤时间", "实际装煤时间", "Max推焦电流", "Avg平煤电流", "T炉号Push", "L炉号Push", "X炉号Push", "Push联锁数据", " M炉号", "Ping联锁数据" };
            string[] columnsName = flag ? columnsName1 : columnsName2;
            //Type[] types = { typeof(string), typeof(byte), typeof(byte), typeof(byte), typeof(int), typeof(int), typeof(int), typeof(int), typeof(DateTime), typeof(DateTime), typeof(DateTime), typeof(byte), typeof(byte), typeof(byte) };
            for (int i = 0; i < columnsName.Length; i++)
            {
                if (!flag)
                {
                    if (i == (columnsName.Length - 1 - 2))
                    {//Push联锁数据
                        dt.Columns.Add(columnsName1[i], typeof(int));
                        continue;
                    }
                    if (i == (columnsName.Length - 1))
                    {//Ping联锁数据
                        dt.Columns.Add(columnsName1[i], typeof(int));
                        continue;
                    }
                }
                //dt.Columns.Add(columnsName[i], types[i]);
                dt.Columns.Add(columnsName1[i], typeof(string));

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
        private string ByteToLockStatus(byte l)
        {
            return l == 1 ? "联锁" : "解锁";
        }
        private void BeginRec(PushInfo pInfo)
        {
            pInfo = new PushInfo();
            pInfo.计划炉号 = pHelper.PlanRoom;
            pInfo.实际炉号 = pHelper.ActualRoom;
            pInfo.实际推焦时间 = pHelper.ActualPushTime == Convert.ToDateTime("0001-1-1 00:00:00") ? DateTime.Now : pHelper.ActualPushTime;
            pInfo.预定出焦时间 = pHelper.PlanPushTime;
            pInfo.上次装煤时间 = pHelper.LastStokingTime;//默认值
            pInfo.规定结焦时间 = pHelper.StandardBurnTime;
            pInfo.计划结焦时间 = pHelper.PlanBurnTime;
            pInfo.实际结焦时间 = pHelper.ActualBurnTime;
            pInfo.时段 = pHelper.Period;
            pInfo.班组 = pHelper.Group;
            pInfo.Push联锁信息 = pHelper.LockInfo;
            pInfo.联锁 = pHelper.Lock;
            pInfo.Push工作车序列 = pHelper.Cars;
            pInfo.Push对中序列 = pHelper.CarsReady;
            pInfo.TAddr = pHelper.TAddr;
            pInfo.LAddr = pHelper.LAddr;
            pInfo.XAddr = pHelper.XAddr;
            pInfo.T炉号 = pHelper.TRoom;
            pInfo.L炉号 = pHelper.LRoom;
            pInfo.X炉号 = pHelper.XRoom;
            pInfo.Push车辆通讯状态 = pHelper.CommStatus;//20170601 通讯状态暂未处理
            pInfo.PlanIndex = pHelper.PlanIndex;
            pInfo.PlanCount = pHelper.PlanCount;
            pInfo.BeginTime = pHelper.BeginTime;
            db.PushInfo.InsertOnSubmit(pInfo);
        }
        private void EndRec(PushInfo pInfo)
        {
            pInfo.Max推焦电流 = pHelper.MaxCur;
            pInfo.Avg推焦电流 = pHelper.AvgCur;
            pInfo.推焦电流 = pHelper.CurArr;
            pInfo.推焦杆长 = pHelper.PoleArr;
            pInfo.EndTime = pHelper.EndTime;
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
            catch (Exception er)
            {
                Log.LogErr.Info("类：PushInfoHelper.cs，方法：RECToDB()；" + er.ToString());
            }

        }
        private void AssignmentInsertInfo()
        {
            PushInfo pInfo = null;
            if (insert)
            {
                BeginRec(pInfo);
            }
            else
            {
                pInfo = db.PushInfo.FirstOrDefault(p => p.BeginTime == pHelper.BeginTime && p.预定出焦时间 == pHelper.PlanPushTime);
                if (pInfo == null)
                {
                    BeginRec(pInfo);
                }
                EndRec(pInfo);
            }
        }
        /// <summary>
        /// 20170910把返回值Datatable改为List<LockInfoHelper> .别删除注释的代码
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        public List<LockInfoHelper> QueryLockInfo(DateTime start, DateTime end, byte room)
        {

            //DataTable dt = new DataTable();
            //SetCloumnsName(dt, false);
            using (db = new DbAppDataContext(Setting.ConnectionStr))
            {
                if (!db.DatabaseExists()) return null;
                var recs = from rec in db.PushInfo
                           where rec.预定出焦时间 >= start && rec.预定出焦时间 < end && (room > 0 ? (rec.计划炉号 == room) : 1 == 1)
                           select new
                           {
                               //<!--6联锁信息：日期，炉号，预定出焦时间，实际推焦时间，计划装煤时间，实际装煤时间，Max推焦电流，Avg平煤电流,时段，班组，
                               //T炉号Push，L炉号Push，X炉号Push，Push联锁数据,M炉号，Ping联锁数据,T物理地址，L物理地址,X物理地址-->
                               rec.计划炉号,
                               rec.预定出焦时间,
                               rec.实际推焦时间,
                               rec.Max推焦电流,
                               rec.时段,
                               rec.班组,
                               rec.T炉号,
                               rec.L炉号,
                               rec.X炉号,
                               rec.TAddr,
                               rec.LAddr,
                               rec.XAddr,
                               rec.Push对中序列,
                               rec.Push联锁信息,
                           };
                if (recs == null || recs.Count() <= 0) return null;
                var recs1 = from rec1 in db.PingInfo
                            where rec1.预定出焦时间 >= start && rec1.预定出焦时间 < end && (room > 0 ? (rec1.预定装煤炉号 == room) : 1 == 1)
                            select new
                            {
                                rec1.预定出焦时间,
                                rec1.Plan平煤时间,
                                rec1.Begin平煤时间,
                                rec1.Avg平煤电流,
                                rec1.M炉号,
                                rec1.Ping联锁信息

                            };
                List<LockInfoHelper> locks = new List<LockInfoHelper>();
                foreach (var r in recs)
                {
                    //<!--6联锁信息：1日期，2炉号，3预定出焦时间，4实际推焦时间，5计划装煤时间，6实际装煤时间，7Max推焦电流，8Avg平煤电流,9时段，10班组，
                    //11T炉号Push，12L炉号Push，13X炉号Push，14对中信号，15Push联锁数据,16M炉号，17Ping联锁数据-->
                    //DataRow row = dt.NewRow();
                    LockInfoHelper helper = new LockInfoHelper();
                    helper.Date = r.预定出焦时间.Value.ToString("d");//1日期
                    helper.Room = r.计划炉号 != null ? r.计划炉号.Value.ToString("000") : "000";//2炉号
                    helper.PushTime = r.预定出焦时间 != null ? r.预定出焦时间.Value.ToString() : "0";//3预定出焦时间
                    helper.ActualPushTime = r.实际推焦时间 != null ? r.实际推焦时间.Value.ToString() : null;//4实际推焦时间
                    helper.MaxCur = r.Max推焦电流 != null ? r.Max推焦电流.Value.ToString("000") : "0";//7Max推焦电流
                    helper.Period = r.时段 != null ? ByteToPeriod(r.时段.Value) : null;//9时段
                    helper.Group = r.班组 != null ? ByteToGroup(r.班组.Value) : null;//10班组
                    helper.TRoom = r.T炉号 != null ? r.T炉号.Value.ToString("000") : "0";//11T炉号Push
                    helper.LRoom = r.L炉号 != null ? r.L炉号.Value.ToString("000") : "0";//12L炉号Push
                    helper.XRoom = r.X炉号 != null ? r.X炉号.Value.ToString("000") : "0";//13X炉号Push14
                    helper.TAddr = r.TAddr != null ? r.TAddr.Value.ToString() : "0";
                    helper.LAddr = r.LAddr != null ? r.LAddr.Value.ToString() : "0";
                    helper.XAddr = r.XAddr != null ? r.XAddr.Value.ToString() : "0";
                    helper.Ready = r.Push对中序列 != null ? r.Push对中序列.Value.ToString() : "0";//14对中信号
                    helper.PushInfo = r.Push联锁信息 != null ? r.Push联锁信息.Value : 0;//15Push联锁数据
                    locks.Add(helper);
                    //dt.Rows.Add(GetRow(row, helper));
                }
                if (recs1.Count() > 0)
                {
                    foreach (var r in recs1)
                    {
                        int index = locks.FindIndex(x => x.PushTime == r.预定出焦时间.Value.ToString());
                        if (index > 0)
                        {
                            locks[index].StokingTime = r.Plan平煤时间 != null ? r.Plan平煤时间.Value.ToString() : null;//5计划装煤时间
                            locks[index].ActualStokingTime = r.Begin平煤时间 != null ? r.Begin平煤时间.Value.ToString() : null;//6实际装煤时间
                            locks[index].AvgCur = r.Avg平煤电流 != null ? r.Avg平煤电流.Value.ToString() : "0";//8Avg平煤电流
                            locks[index].MRoom = r.M炉号 != null ? r.M炉号.Value.ToString("000") : "0";//16M炉号
                            locks[index].PingInfo = r.Ping联锁信息 != null ? r.Ping联锁信息.Value : 0;//17Ping联锁数据
                        }
                    }
                }
                return locks;
            }
        }
        private DataRow GetRow(DataRow row, LockInfoHelper helper)
        {
            int index = 0;
            row[index++] = helper.Date;//1日期
            row[index++] = helper.Room;//2炉号
            row[index++] = helper.PushTime;//3预定出焦时间
            row[index++] = helper.ActualPushTime;//4实际推焦时间
            row[index++] = helper.StokingTime;//5计划装煤时间
            row[index++] = helper.ActualStokingTime;//6实际装煤时间
            row[index++] = helper.MaxCur;//7Max推焦电流
            row[index++] = helper.AvgCur;//8Avg平煤电流
            row[index++] = helper.Period;//9时段
            row[index++] = helper.Group;//10班组
            row[index++] = helper.TRoom;//11T炉号Push
            row[index++] = helper.LRoom;//12L炉号Push
            row[index++] = helper.XRoom;//13X炉号Push14
            row[index++] = helper.Ready;//14对中信号
            row[index++] = helper.PushInfo;//15Push联锁数据
            row[index++] = helper.MRoom;//16M炉号
            row[index++] = helper.PingInfo;//17Ping联锁数据
            return row;
        }
        public List<CurHelper> QueryCurInfo(DateTime start, DateTime end, byte room)
        {
            using (db = new DbAppDataContext(Setting.ConnectionStr))
            {
                if (!db.DatabaseExists()) return null;
                var recs = from rec in db.PushInfo
                           where rec.预定出焦时间 >= start && rec.预定出焦时间 < end && (room > 0 ? (rec.计划炉号 == room) : 1 == 1)
                           select new
                           {
                               //<!--6联锁信息：日期，炉号，预定出焦时间，Max推焦电流，Avg平煤电流,时段，班组，
                               rec.计划炉号,
                               rec.预定出焦时间,
                               rec.Max推焦电流,
                               rec.Avg推焦电流,
                               rec.时段,
                               rec.班组,
                               rec.推焦杆长,
                               rec.推焦电流,
                           };
                var recs1 = from rec1 in db.PingInfo
                            where rec1.预定出焦时间 >= start && rec1.预定出焦时间 < end && (room > 0 ? (rec1.预定装煤炉号 == room) : 1 == 1)
                            select new
                            {
                                rec1.预定出焦时间,
                                rec1.Avg平煤电流,
                                rec1.Pole平煤杆长,
                                rec1.Cur平煤电流
                            };
                if (recs == null || recs.Count() == 0) return null;
                List<CurHelper> cur = new List<CurHelper>();
                foreach (var r in recs)
                {
                    CurHelper c = new CurHelper();
                    c.Date = r.预定出焦时间 != null ? r.预定出焦时间.Value.ToString("d") : "0000";
                    c.Room = r.计划炉号 != null ? r.计划炉号.Value.ToString("000") : "000";
                    c.PushTime = r.预定出焦时间 != null ? r.预定出焦时间.Value.ToString("g") : "000";
                    c.MaxCur = r.Max推焦电流 != null ? r.Max推焦电流.Value.ToString("000") : "000";
                    c.AvgPushCur = r.Avg推焦电流 != null ? r.Avg推焦电流.Value.ToString("000") : "000";
                    c.PushPole = r.推焦杆长;
                    c.PushCur = r.推焦电流;
                    cur.Add(c);
                }
                if (recs1.Count() > 0)
                {
                    foreach (var r in recs1)
                    {
                        int index = cur.FindIndex(x => x.PushTime == ((DateTime)r.预定出焦时间).ToString("g"));
                        if (index >= 0)
                        {
                            cur[index].AvgPingCur = r.Avg平煤电流 != null ? r.Avg平煤电流.Value.ToString("000") : "000";
                            cur[index].PingPole = r.Pole平煤杆长;
                            cur[index].PingCur = r.Cur平煤电流;
                        }
                    }
                }
                return cur;
            }
        }
        #region 统计相关
        public List<SumHelper> QuerySumInfo(DateTime start, int period)
        {
            using (db = new DbAppDataContext(Setting.ConnectionStr))
            {
                if (!db.DatabaseExists()) return null;
                DateTime[] time = PeriodToTime(start, period);
                var recs = from rec in db.PushInfo
                           from rec1 in db.PingInfo
                           where rec.预定出焦时间 >= time[0] && rec.预定出焦时间 < time[1]
                            && rec1.预定出焦时间 >= time[0] && rec1.预定出焦时间 < time[1]
                            && (rec.预定出焦时间 == rec1.预定出焦时间)
                           select new
                           {
                               rec.计划炉号,
                               rec.预定出焦时间,
                               rec.计划结焦时间,
                               rec.规定结焦时间,
                               rec.实际结焦时间,
                               rec.实际推焦时间,
                               rec1.Begin平煤时间,//装煤时间
                               rec.Max推焦电流,
                           };
                if (recs == null || recs.Count() <= 0) return null;
                List<SumHelper> sum = new List<SumHelper>();
                int index = 1;
                foreach (var r in recs)
                {
                    SumHelper s = new SumHelper();
                    s.Num = index++;//炉数
                    s.RoomNum = r.计划炉号 != null ? (int)r.计划炉号 : 0;
                    s.PushTime = r.预定出焦时间 != null ? (DateTime)r.预定出焦时间 : DateTime.Now;
                    s.ActualPushTime = r.实际推焦时间 != null ? (DateTime)r.实际推焦时间 : DateTime.Now;
                    s.ActualStokingTime = r.Begin平煤时间 != null ? (DateTime)r.Begin平煤时间 : DateTime.Now;
                    s.StandardBurnTime = r.规定结焦时间 != null ? (int)r.规定结焦时间 : 0;
                    s.BurnTime = r.计划结焦时间 != null ? (int)r.计划结焦时间 : 0;
                    s.ActualBurnTime = r.实际结焦时间 != null ? (int)r.实际结焦时间 : 0;
                    s.PushCur = r.Max推焦电流 != null ? (int)r.Max推焦电流 : 0;
                    sum.Add(s);
                }
                return sum;
            }
        }
        public DataTable ListToDataTable(List<SumHelper> lst)
        {
            using (DataTable dt = new DataTable())
            {
                //SetCloumnsProperty(dt);
                //AddLastRow(dt, lst);
                return dt;
            }
        }
        private DateTime[] PeriodToTime(DateTime start, int period)
        {
            DateTime s = DateTime.Now;//时段对应的开始时间
            DateTime e = DateTime.Now;//时段对应的结束时间
            switch (period)
            {
                case 0:
                    s = start;
                    e = start.AddHours(8);
                    break;
                case 1:
                    s = start.AddHours(8);
                    e = start.AddHours(16);
                    break;
                case 2:
                    s = start.AddHours(16);
                    e = start.AddHours(24);
                    break;
                default:
                    s = start;
                    e = start.AddHours(8);
                    break;
            }
            return new DateTime[] { s, e };
        }
        private void SetCloumnsProperty(DataTable dt)
        {
            string[] columnsName = { "预定出焦炉数", "实际出焦炉数", "K1", "K2", "K3", "平均结焦时间", "最长结焦时间", "最短结焦时间", "平均推焦电流" };
            //Type[] types = { typeof(string), typeof(byte), typeof(byte), typeof(byte), typeof(int), typeof(int), typeof(int), typeof(int), typeof(DateTime), typeof(DateTime), typeof(DateTime), typeof(byte), typeof(byte), typeof(byte) };
            for (int i = 0; i < columnsName.Length; i++)
            {
                //dt.Columns.Add(columnsName[i], types[i]);
                dt.Columns.Add(columnsName[i], typeof(string));
            }
        }
        private double GetK1(List<SumHelper> lst)
        {
            int M = lst.Count;
            int A1 = 0;
            for (int i = 0; i < M; i++)
            {
                A1 = lst[i].A1 ? A1 + 1 : A1;
            }
            double K1 = (M - A1) / (double)M;
            return ((int)(K1 * 100)) / (double)100;
        }
        private double GetK2(List<SumHelper> lst, out int N)
        {
            int M = lst.Count;
            N = 0;
            int A2 = 0;
            int m = 60 - lst.Last().PushTime.Minute;
            DateTime t = lst.Last().PushTime.AddMinutes(m);
            for (int i = 0; i < M; i++)
            {
                A2 = lst[i].A2 ? A2 + 1 : A2;
                if (lst[i].PushTime < t) N++;
            }
            double K2 = (N - A2) / (double)M;
            return ((int)(K2 * 100)) / (double)100;
        }
        /// <summary>
        /// 0:最长；1:最短；2:平均结焦；3:平均电流
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        private int[] CalcValue(List<SumHelper> lst)
        {
            int max = 0; int min = 0;
            int sumTime = 0; int sumCur = 0;
            int invalidCount = 0;
            for (int i = 0; i < lst.Count; i++)
            {
                bool invalid = lst[i].ActualBurnTime > Setting.BurnTime + 60 * 5;
                if (invalid) invalidCount++;
                if (max == 0) max = invalid ? 0 : lst[i].ActualBurnTime;
                max = Math.Max(invalid ? max : lst[i].ActualBurnTime, max);
                if (i == 0) min = lst.First().ActualBurnTime;
                min = Math.Min(invalid ? min : lst[i].ActualBurnTime, min);
                sumTime += !invalid ? lst[i].ActualBurnTime : 0;
                sumCur += lst[i].PushCur;
            }
            return new int[] { max, min, sumTime / lst.Count - invalidCount, sumCur / lst.Count };
        }
        private void GetRows(DataTable dt, List<SumHelper> lst)
        {
            for (int i = 0; i < lst.Count; i++)
            {
                DataRow row = dt.NewRow();
                int index = 0;
                row[index++] = lst[i].RoomNum.ToString("000");
                row[index++] = lst[i].PushTime.ToString("g");//预定出焦时间
                row[index++] = lst[i].ActualPushTime.ToString("g");//实际出焦时间
                row[index++] = lst[i].ActualStokingTime.ToString("g");//实际装煤时间
                row[index++] = lst[i].StrStandardBurnTime;//规定结焦时间
                row[index++] = lst[i].StrBurnTime;//计划结焦时间
                row[index++] = lst[i].StrActualBurnTime;//实际结焦时间
                row[index] = lst[i].PushCur;//推焦电流
                dt.Rows.Add(row);
            }
        }
        public DataTable AddLastRow(List<SumHelper> lst)
        {
            //GetRows(dt, lst);
            DataTable dt = new DataTable();
            SetCloumnsProperty(dt);
            DataRow r = dt.NewRow();
            //K值的计算,实际出焦炉数N
            int N = 0;
            double K1 = GetK1(lst);//数字100的出现是为了实现保留两位小数点
            double K2 = GetK2(lst, out N);//数字100的出现是为了实现保留两位小数点
            double K3 = (int)(K1 * K2 * 10000) / (double)10000;
            //平均推焦电流,最大/最小/平均结焦时间
            int[] calc = CalcValue(lst);
            int index = 0;
            r[index++] = lst.Count;
            r[index++] = N.ToString();
            r[index++] = K1.ToString();
            r[index++] = K2.ToString();
            r[index++] = K3.ToString();
            r[index++] = ToTimeFormat(calc[2]).ToString();
            r[index++] = ToTimeFormat(calc[0]).ToString();
            r[index++] = ToTimeFormat(calc[1]).ToString();
            r[index] = calc[3].ToString();
            dt.Rows.Add(r);
            return dt;
        }
        private string ToTimeFormat(int time)
        {
            if (time == 0) return "00:00";
            return (time / 60).ToString("00") + ":" + (time % 60).ToString("00");
        }
        #endregion

    }
    /// <summary>
    /// Ps->Push,PsInfo->PushInfo
    /// </summary>
    class PsInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pushTime"> 开始推焦时间</param>
        /// <param name="job">是否为工作车</param>
        public PsInfo(int index, string pushTime, bool job)
        {
            List<Vehicle> lstVehical = job ? Communication.TJobCarLst : Communication.NonJobCarLst;
            bool valid = index >= 0 ? true : false;
            DateTime invalidTime = Convert.ToDateTime("2012-04-03 13:14");
            TPushPlan pNull = new TPushPlan(1, 1, 1, invalidTime);
            TPushPlan p = new TPushPlan();
            p = valid ? CokeRoom.PushPlan[index] : (CokeRoom.PushPlan.Count > 0 ? new TPushPlan(CokeRoom.PushPlan[0]) : pNull);//20170923 PushPlan.Count==0时如何处理？
            PlanRoom = p.RoomNum;//计划炉号
            ActualRoom = (byte)lstVehical[0].RoomNum;//pInfo.实际炉号 = (byte)lst[0].RoomNum;
            //pInfo.实际推焦时间 = Convert.ToDateTime(now);
            ActualPushTime = Convert.ToDateTime(pushTime);
            //pInfo.预定出焦时间 = p.PushTime;
            PlanPushTime = p.PushTime;
            //pInfo.上次装煤时间 = p.StokingTime;//默认值
            LastStokingTime = p.StokingTime;
            //pInfo.规定结焦时间 = p.standardBurnTime;
            StandardBurnTime = p.StandardBurnTime;
            //pInfo.计划结焦时间 = p.BurnTime;
            PlanBurnTime = p.BurnTime;
            //pInfo.实际结焦时间 = (int)(Convert.ToDateTime(now) - p.StokingTime).TotalMinutes;
            ActualBurnTime = (int)(Convert.ToDateTime(pushTime) - p.StokingTime).TotalMinutes;
            //pInfo.时段 = (byte)p.Period;
            Period = (byte)p.Period;
            //pInfo.班组 = (byte)p.Group;
            Group = (byte)p.Group;
            DwTogetherInfo tInfo = job ? Communication.JobCarTogetherInfo : Communication.NonJobCarTogetherInfo;
            //pInfo.Push联锁信息 = tInfo.InfoToInt;
            LockInfo = tInfo.InfoToInt;
            //pInfo.联锁 = tInfo.PushTogether ? (byte?)1 : 0;
            Lock = (byte)(tInfo.PushTogether ? 1 : 0);
            //pInfo.Push工作车序列 = (short)(lst[0].CarNum * 100 + lst[1].CarNum * 10 + lst[2].CarNum);
            Cars = (short)(lstVehical[0].CarNum * 100 + lstVehical[1].CarNum * 10 + lstVehical[2].CarNum);
            //推拦熄煤箭头:20170601 暂设置为固定值16;当工作推焦车出焦时，有三工作车得到箭头；非工作车时，置零
            //pInfo.Push对中序列 = null;
            CarsReady = ArrowsToInt(lstVehical[0].Arrows, lstVehical[1].Arrows, lstVehical[2].Arrows);//箭头指示：前四位bit为推焦车的箭头，接下来四位拦焦车箭头，最后四位熄焦车箭头；共占12bit
            //pInfo.Push推焦车地址 = lst[0].DataRead.PhysicalAddr;
            TAddr = lstVehical[0].DataRead.PhysicalAddr;
            //pInfo.Push拦焦车地址 = lst[1].DataRead.PhysicalAddr;
            LAddr = lstVehical[1].DataRead.PhysicalAddr;
            //pInfo.Push熄焦车地址 = lst[2].DataRead.PhysicalAddr;
            XAddr = lstVehical[2].DataRead.PhysicalAddr;
            //pInfo.Push推焦车炉号 = lst[0].RoomNum;
            TRoom = lstVehical[0].RoomNum;
            //pInfo.Push拦焦车炉号 = lst[1].RoomNum;
            LRoom = lstVehical[1].RoomNum;
            //pInfo.Push熄焦车炉号 = lst[2].RoomNum;
            XRoom = lstVehical[2].RoomNum;
            //pInfo.Push车辆通讯状态 = null;//20170601 通讯状态暂未处理；设备的顺序：PLC，触摸屏，无线模块，解码器,每辆车占4bit，共计8辆车，
            CommStatus = 0;//20170601 通讯状态暂未处理
            PlanIndex = index;
            PlanCount = CokeRoom.PushPlan.Count;
            BeginTime = DateTime.Now;
        }
        public byte PlanRoom { get; set; }
        public byte ActualRoom { get; set; }
        public byte Period { get; set; }
        public byte Group { get; set; }
        public byte Lock { get; set; }
        public DateTime ActualPushTime { get; set; }
        public DateTime PlanPushTime { get; set; }
        public DateTime LastStokingTime { get; set; }
        public int PlanBurnTime { get; set; }
        public int StandardBurnTime { get; set; }
        public int ActualBurnTime { get; set; }
        public int LockInfo { get; set; }
        public short Cars { get; set; }//工作车序列
        public int CarsReady { get; set; }//工作车对中序列（箭头指示）
        public int TAddr { get; set; }
        public int LAddr { get; set; }
        public int XAddr { get; set; }
        public int TRoom { get; set; }
        public int LRoom { get; set; }
        public int XRoom { get; set; }
        public int CommStatus { get; set; }//按推拦熄顺序，低字节到高字节：无线网桥、PLC、解码器、触摸屏
        public byte MaxCur { get; set; }
        public byte AvgCur { get; set; }
        public string CurArr { get; set; }
        public string PoleArr { get; set; }//推焦杆Convert.TosBase64String()
        public int PlanIndex { get; set; }
        public int PlanCount { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        private int ArrowsToInt(ushort T, ushort L, ushort X)
        {
            int result = 0;
            byte[] arr = { (byte)T, (byte)L, (byte)X };
            List<bool> lst = new List<bool>();
            for (int i = 0; i < arr.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    lst.Add(Convert.ToBoolean(arr[i] & (byte)Math.Pow(2, j)));
                }
            }
            for (int i = 0; i < lst.Count; i++)
            {
                int a = lst[i] ? (int)Math.Pow(2, i) : 0;
                result += a;
            }
            return result;
        }
    }
    class LockInfoHelper
    {
        public LockInfoHelper() { }
        //<!--6联锁信息：1日期，2炉号，3预定出焦时间，4实际推焦时间，5计划装煤时间，6实际装煤时间，7Max推焦电流，8Avg平煤电流,9时段，10班组，
        //11T炉号Push，12L炉号Push，13X炉号Push，14对中信号，15Push联锁数据,16M炉号，17Ping联锁数据-->
        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }
        /// <summary>
        /// 炉号
        /// </summary>
        public string Room { get; set; }
        public string PushTime { get; set; }
        public string ActualPushTime { get; set; }
        public string StokingTime { get; set; }
        public string ActualStokingTime { get; set; }
        public string MaxCur { get; set; }
        public string AvgCur { get; set; }
        public String Period { get; set; }
        public String Group { get; set; }
        public String TRoom { get; set; }
        public String TAddr { get; set; }
        public String LAddr { get; set; }
        public String XAddr { get; set; }
        public string LRoom { get; set; }
        public string XRoom { get; set; }
        //<!--6联锁信息：1日期，2炉号，3预定出焦时间，4实际推焦时间，5计划装煤时间，6实际装煤时间，7Max推焦电流，8Avg平煤电流,9时段，10班组，
        //11T炉号Push，12L炉号Push，13X炉号Push，14对中信号，15Push联锁数据,16M炉号，17Ping联锁数据-->
        /// <summary>
        /// 对中信号
        /// </summary>
        public string Ready { get; set; }
        public int PushInfo { get; set; }
        public string MRoom { get; set; }
        public int PingInfo { get; set; }
    }
    class CurHelper
    {
        public string Date { get; set; }
        public string Room { get; set; }
        public string PushTime { get; set; }
        public string MaxCur { get; set; }
        public string AvgPushCur { get; set; }
        public string AvgPingCur { get; set; }
        public string PushPole { get; set; }
        public string PushCur { get; set; }
        public string PingPole { get; set; }
        public string PingCur { get; set; }
        public string Period { get; set; }
        public String Group { get; set; }
    }
    /// <summary>
    /// 生产统计
    /// M：本班计划出焦炉数；N本班实际出焦炉数
    /// A1=计划结焦时间BurnTime-规定结焦时间StandardBurnTime
    /// A2=实际出焦时间ActualPushTime-计划出焦时间PushTime 
    /// K1=(M-A1)/M; K2=(N-A2)/M;  K3=K1*K2;
    /// </summary>
    class SumHelper
    {
        public int Num { get; set; }
        public int RoomNum { get; set; }
        /// <summary>
        /// 计划出焦时间
        /// </summary>
        public DateTime PushTime { get; set; }
        public string StrPushTime { get { return PushTime.ToString("g"); } }
        /// <summary>
        /// 计划结焦时间
        /// </summary>
        public int BurnTime { get; set; }
        public string StrBurnTime { get { return ToTimeFormat(BurnTime); } }
        /// <summary>
        /// 结焦时间明显不合理
        /// </summary>
        public bool Invalid { get { return BurnTime > Setting.BurnTime + 60 * 4; } }
        /// <summary>
        /// 规定结焦时间
        /// </summary>
        public int StandardBurnTime { get; set; }
        public string StrStandardBurnTime { get { return ToTimeFormat(StandardBurnTime); } }
        /// <summary>
        /// 实际出焦时间
        /// </summary>
        public DateTime ActualPushTime { get; set; }
        public string StrActualPushTime { get { return ActualPushTime.ToString("g"); } }
        /// <summary>
        /// 实际装煤时间
        /// </summary>
        public DateTime ActualStokingTime { get; set; }
        public string StrActualStokingTime { get { return ActualStokingTime.ToString("g"); } }
        /// <summary>
        /// 实际结焦时间
        /// </summary>
        public int ActualBurnTime { get; set; }
        public string StrActualBurnTime { get { return ToTimeFormat(ActualBurnTime); } }
        public int PushCur { get; set; }
        public bool A1
        {
            get
            {
                return Math.Abs(BurnTime - StandardBurnTime) > 5 ? true : false;
            }
        }
        public bool A2
        {
            get
            {
                return Math.Abs((int)(ActualPushTime - PushTime).TotalMinutes) > 5 ? true : false;
            }
        }
        private string ToTimeFormat(int time)
        {
            if (time == 0) return "00:00";
            return (time / 60).ToString("00") + ":" + (time % 60).ToString("00");
        }
    }
}


