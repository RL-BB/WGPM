using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGPM.R.RoomInfo;
using System.Data;
using WGPM.R.UI;

namespace WGPM.R.LinqHelper
{
    class EditPlanHelper
    {
        public EditPlanHelper() { }
        public EditPlanHelper(List<TPushPlan> editPlan)
        {
            this.editPlan = editPlan;
        }
        private List<TPushPlan> editPlan;
        DbAppDataContext db;
        PushPlan plan = new PushPlan();
        public void AddToDbPlan(List<TPushPlan> plan)
        {
            if (plan == null || plan.Count == 0) return;
            List<PushPlan> lstPlan = new List<PushPlan>();
            DateTime wrtTime = DateTime.Now;
            for (int i = 0; i < plan.Count; i++)
            {
                TPushPlan p = plan[i];
                lstPlan.Add(new PushPlan
                {
                    炉号 = p.RoomNum,
                    预定出焦时间 = p.PushTime,
                    上次装煤时间 = p.StokingTime,
                    计划结焦时间 = (short)p.BurnTime,
                    规定结焦时间 = (short)p.StandardBurnTime,
                    时段 = (byte)p.Period,
                    班组 = (byte)p.Group,
                    计划写入时间 = wrtTime
                });
            }
            db.PushPlan.InsertAllOnSubmit(lstPlan);
        }
        /// <summary>
        /// 从数据库中删除计划
        /// </summary>
        /// <param name="plan"></param>
        public void DeleteFromDbPlan(List<TPushPlan> plan)
        {
            if (plan == null || plan.Count == 0) return;
            DateTime endtime;
            DateTime startTime = GetPeriodTimeByPeriod(plan[0].PushTime.Date, plan[0].Period, out endtime);
            var plans = from p in db.PushPlan
                        where p.预定出焦时间 >= startTime && p.预定出焦时间 < endtime
                        select p;
            if (plans.Count() <= 0) return;
            db.PushPlan.DeleteAllOnSubmit(plans);
        }
        public void RecToPlanDB()
        {
            using (db = new DbAppDataContext(Setting.ConnectionStr))
            {
                if (!db.DatabaseExists()) return;
                DeleteFromDbPlan(editPlan);
                AddToDbPlan(editPlan);
                db.SubmitChanges();
            }

        }
        public DataTable QueryPlanDB(DateTime start, DateTime end)
        {
            //计划查询：
            //1炉号，2预定出焦时间，3实际推焦时间（删除0829），4上次装煤时间，5实际结焦时间（删除0829），6计划结焦时间，7规定结焦时间，8实际结焦时间
            //起始时间：Start，结束时间End
            DataTable table = new DataTable("Plan");
            string[] columnsName = { "日期", "炉号", "预定出焦时间", "上次装煤时间", "规定结焦时间", "计划结焦时间" };
            Type[] types = { typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string) };
            for (int i = 0; i < columnsName.Length; i++)
            {
                table.Columns.Add(columnsName[i], types[i]);
            }
            using (db = new DbAppDataContext(Setting.ConnectionStr))
            {
                if (!db.DatabaseExists()) return null;
                var plans = from p in db.PushPlan
                            where p.预定出焦时间 >= start && p.预定出焦时间 < end
                            select p;
                if (plans.Count() <= 0) return null;
                List<PushPlan> lstPlan = plans.ToList();
                plans = plans.OrderBy(p => p.预定出焦时间);//按预定出焦时间排序
                foreach (var p in plans)
                {
                    DataRow r = table.NewRow();
                    r[0] = p.预定出焦时间.Value.ToShortDateString();//日期
                    r[1] = p.炉号.Value.ToString("000");//炉号
                    r[2] = p.预定出焦时间.Value.ToString("g");//预定出焦时间
                    r[3] = p.上次装煤时间.Value.ToString("g");//装煤时间
                    r[4] = ToTimeFormat(p.规定结焦时间.Value);//规定结焦时间
                    r[5] = ToTimeFormat(p.计划结焦时间.Value);//计划结焦时间
                    table.Rows.Add(r);
                }
                return table;
            }
        }
        private string ToTimeFormat(int time)
        {
            return (time / 60).ToString("00") + ":" + (time % 60).ToString("00");
        }
        public DateTime GetPeriodTimeByPeriod(DateTime date, int period, out DateTime endTime)
        {
            DateTime startTime = DateTime.Now;
            endTime = startTime;
            string start = string.Empty;
            string end = string.Empty;
            switch (period)
            {
                case 1:
                    start = "08:00";
                    end = "19:59";
                    break;
                case 2:
                    start = "20:00";
                    end = "07:59";
                    break;
                //case 3:
                //    start = "20:00";
                //    end = "23:59";
                //    break;
                //case 4:
                //    start = "00:00";
                //    end = "07:59";
                //    break;
                default:
                    break;
            }
            startTime = Convert.ToDateTime(date.ToString("d") + " " + start);
            endTime = period == 1 ? Convert.ToDateTime(date.ToString("d") + " " + end) : Convert.ToDateTime(date.ToString("d") + " " + end).AddDays(1);
            return startTime;
        }
    }
}
