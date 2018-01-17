using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using WGPM.R.XML;
using System.Xml;
using WGPM.R.UI;

namespace WGPM.R.RoomInfo
{
    enum PlanIndex
    {
        Push, Stoking, Burn
    }
    /// <summary>
    /// CokeRoom：炭化室，看到单词Room应联想到炭化室；Room对应CokeRoom，是CokeRoom的简写；
    /// 各CokeRoom对应的生产时间：包括推焦、装煤、结焦的计划（规定）时间和实际时间
    /// 推焦车（分推焦位和平煤位），拦焦车，熄焦车（分1#电机车的1#焦罐和2#焦罐，2#熄焦车的干熄罐和水熄罐），装煤车；
    /// </summary>
    class CokeRoom
    {
        public CokeRoom()
        {
            BurnStatus = new Dictionary<int, BurnTime>();
            BurnStatus = GetRoomsBurnStatus();
            PushPlan = GetPushPlan();
            StokingPlan = new List<MStokingPlan>();
            StokingPlan = GetStokingPlan(PushPlan);
            //StokingPlan12.Add(new MStokingPlan(88, Convert.ToDateTime("2017-09-21 15:38"), 1, 1));
            GetScheduleLst();
            RoomDoorLst = new List<RoomDoor>();
            for (int i = 0; i < 110; i++)
            {
                RoomDoor d = new RoomDoor();
                RoomDoorLst.Add(d);
            }
        }
        //定义一个计时器来处理计划：当白班和夜班交替时，删除上一个班的计划；
        public OperateConfig Config { get; set; }
        /// <summary>
        /// 索引+1=RoomNum；
        /// </summary>
        public static List<RoomDoor> RoomDoorLst { get; set; }
        public static Dictionary<int, BurnTime> BurnStatus { get; set; }
        /// <summary>
        /// 1#、2#炉区的推焦计划
        /// </summary>
        public static List<TPushPlan> PushPlan { get; set; }
        public static List<MStokingPlan> StokingPlan { get; set; }
        public static List<Schedule> ScheduleLst { get; set; }
        /// <summary>
        /// 得到推焦计划和装煤计划
        /// </summary>
        /// <param name="stokingPlan">装煤计划</param>
        /// <returns>推焦计划</returns>
        private List<TPushPlan> GetPushPlan()
        {
            List<TPushPlan> plan = new List<TPushPlan>();
            //if (Setting.IsServer) return plan;
            TPushPlan currentTPlan;
            Dictionary<int, BurnTime> dic = BurnStatus;
            string path = @"Config\RoomPlanInfo.config";
            OperateConfig Config = new OperateConfig(path);
            foreach (XmlNode xmlNode in Config.XmlNodeList)
            {
                int valid = Convert.ToInt32(xmlNode.Attributes.Item(1).Value);
                bool planValid = Convert.ToBoolean(valid);
                if (!planValid)
                {
                    continue;
                }
                currentTPlan = new TPushPlan();
                //计划有效，执行下列语句
                currentTPlan.Period = Convert.ToInt32(xmlNode.Attributes.Item(2).Value);
                currentTPlan.Group = Convert.ToInt32(xmlNode.Attributes.Item(3).Value);
                currentTPlan.RoomNum = Convert.ToByte(xmlNode.Attributes.Item(0).Value);
                currentTPlan.PushTime = Convert.ToDateTime(xmlNode.Attributes.Item(5).Value);
                currentTPlan.StokingTime = BurnStatus[currentTPlan.RoomNum].StokingTime;
                DateTime time = Convert.ToDateTime(xmlNode.Attributes.Item(7).Value);
                //currentTPlan.StandardBurnTime = (short)(time.Hour * 60 + time.Minute);
                currentTPlan.BurnTime = currentTPlan.GetBurnTime();
                DateTime t = currentTPlan.PushTime;
                plan.Add(currentTPlan);
            }
            //Array.Sort()
            plan.Sort(TPushPlan.CompareByTime);
            return plan;
        }
        private List<MStokingPlan> GetStokingPlan(List<TPushPlan> push)
        {
            List<MStokingPlan> plan = new List<MStokingPlan>();
            if (PushPlan.Count == 0) return plan;
            for (int i = 0; i < push.Count; i++)
            {
                plan.Add(new MStokingPlan(push[i]));
            }
            return plan;
        }
        /// <summary>
        /// 根据DateTime.Now和装煤时间的差值（Timespan.TotalHours）和规定结焦时间作除法
        /// 得到一比率，用来描述结焦状态
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, BurnTime> GetRoomsBurnStatus()
        {
            //规定结焦时间
            Dictionary<int, BurnTime> burnStatus = new Dictionary<int, BurnTime>();
            int roomNum = 1;
            string path = @"Config\RoomPlanInfo.config";
            Config = new OperateConfig(path);
            foreach (XmlNode xmlNode in Config.XmlNodeList)
            {
                //装煤时间
                DateTime stokingTime = Convert.ToDateTime(xmlNode.Attributes.Item(4).Value);
                BurnTime burnTime = new BurnTime(stokingTime);
                burnStatus.Add(roomNum++, burnTime);
            }
            return burnStatus;
        }
        public static Brush GetRoomsColor(double burnRatio)
        {
            SolidColorBrush color = new SolidColorBrush();
            if (burnRatio <= 0)
            {
                color = Brushes.Black;
            }
            if (burnRatio >= 1)
            {
                color = Brushes.Red;
            }
            if (burnRatio > 0 && burnRatio < 1)
            {
                color = new SolidColorBrush { Color = new Color { R = (byte)(burnRatio * 255), G = 0, B = 0, A = 255 } };
            }
            return color;
        }
        public static double GetRoomsHeight(double burnRatio)
        {
            double height = 0;
            if (burnRatio <= 0)
            {
                height = 10;
            }
            else if (burnRatio > 1)
            {
                height = 185;
            }
            else
            {
                height = 175 * burnRatio + 10;
            }
            return height;
        }
        /// <summary>
        /// 判断是否可以保存计划：关键在于炉号是否有重复
        /// </summary>
        /// <param name="displayPlan">如果displayPlan.Count!=0,则意味着有部分计划重新编辑了，判断重复时，应从PushPlan中删除该部分</param>
        /// <param name="editingPlan">将要添加的计划</param>
        /// <param name="repeatRoom">重复的炉号</param>
        /// <returns></returns>
        public static bool IsPlanRepetitive(List<TPushPlan> anotherPeriod, List<TPushPlan> editingPlan, out int repeatRoom)
        {
            bool repeat = false;
            repeatRoom = 0;
            //判断PushPlan中的炉号和editingTPlan的炉号是否存在重复，若有返回true，否则保存至PushPlan计划中；
            for (int i = editingPlan.Count - 1; i >= 0; i--)
            {//因为在editingPlan中和PushPlan重复的炉号一般比较靠后，所以如果有重复的，则从后向前寻找比较快，找到当即返回结果，结束循环查找
                int index = anotherPeriod.FindIndex(x => x.RoomNum == editingPlan[i].RoomNum);
                if (index >= 0)
                {
                    repeat = true;
                    repeatRoom = anotherPeriod[index].RoomNum;
                    break;
                }
            }
            return repeat;
        }
        /// <summary>
        /// 由编辑计划得到新的出焦计划和装煤计划
        /// </summary>
        /// <param name="editingPlan"></param>
        public static void SaveToPlan(List<TPushPlan> editingPlan)
        {
            //删除重复的计划炉号
            if (editingPlan.Count > 0)
            {
                //重复编辑同一时段的计划时，删除该时段之前的计划，更新为当前的计划
                if (PushPlan.Count > 0) PushPlan.RemoveAll(p => p.Period == editingPlan[0].Period);
                if (StokingPlan.Count > 0) StokingPlan.RemoveAll(s => s.Period == editingPlan[0].Period);

                for (int i = 0; i < editingPlan.Count; i++)
                {
                    StokingPlan.Add(new MStokingPlan(editingPlan[i]));
                }
            }
            PushPlan.AddRange(editingPlan);//更新PushPlan
            PushPlan.Sort(TPushPlan.CompareByTime);
            StokingPlan.Sort(MStokingPlan.CompareByTime);
        }
        private void GetScheduleLst()
        {
            ScheduleLst = new List<Schedule>();
            //白甲→夜乙→白丙→夜甲→白丁→夜丙→白乙→夜丁→白甲
            ScheduleLst.Add(new Schedule(1, 1));
            ScheduleLst.Add(new Schedule(2, 2));
            ScheduleLst.Add(new Schedule(1, 3));
            ScheduleLst.Add(new Schedule(2, 1));
            ScheduleLst.Add(new Schedule(1, 4));
            ScheduleLst.Add(new Schedule(2, 3));
            ScheduleLst.Add(new Schedule(1, 2));
            ScheduleLst.Add(new Schedule(2, 4));
        }
    }
    class BurnTime
    {
        public BurnTime(DateTime time)
        {
            stokingTime = time;
        }
        public DateTime StokingTime
        {
            get { return stokingTime; }
            set { stokingTime = value; }
        }
        private DateTime stokingTime;
        public DateTime ActualPushTime;
        public double StandardTime { get { return 19; } }
        public double BurnStatus { get { return (DateTime.Now - StokingTime).TotalHours / StandardTime; } }
    }
    class RoomDoor
    {
        public RoomDoor() { }
        /// <summary>
        /// 炉门已关
        /// </summary>
        public bool TDoorOpen { get; set; }
        /// <summary>
        /// 小炉门打开
        /// </summary>
        public bool TMDoorOpen { get; set; }
        /// <summary>
        /// 焦侧炉门已关
        /// </summary>
        public bool LDoorOpen { get; set; }
        /// <summary>
        /// 炉盖打开
        /// </summary>
        public bool LidOpen { get; set; }
    }
}
