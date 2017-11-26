using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WGPM.R.XML;
using System.Xml;
using WGPM.R.UI;
using System.Windows;

namespace WGPM.R.RoomInfo
{
    /// <summary>
    ///定义（在config中的简写）
    ///Room,[0]R(炭化室);PushPlanInvalid,[1]I(有效);StokingPlanInvalid[2];
    /// Period,[3]P(时段); Team,[4]T(班组);
    ///PushTime,[5]PT（推焦时间）; PlannedTimeToPush[6] PlndTP(计划推焦时间);
    ///StokingTime,[7]ST(装煤时间); PlannedTimeToStoking,[8]PlndTS(计划装煤时间);
    ///BurnTime,[9]BT(结焦时间);PrescriptiveBurnTime,[10]PBT(规定结焦时间);
    /// </summary>
    class Schedule
    {
        public Schedule(int period, int group)
        {
            this.period = period;
            this.group = group;
        }
        //时段
        public string Period
        {
            get
            {
                return period <= 1 ? "白班" : "夜班";
            }
        }
        public int period;
        //班组
        public int Group { get { return group; } }
        public int group;
    }
    interface IPlan
    {
        byte RoomNum { get; set; }
    }
    public class TPushPlan : DependencyObject, IPlan
    {
        public TPushPlan()
        {
        }
        public TPushPlan(int room, int period, int group, DateTime pushTime)
        {
            RoomNum = (byte)room;
            this.Period = period;
            this.Group = group;
            this.PushTime = pushTime;
            StokingTime = CokeRoom.BurnStatus[RoomNum].StokingTime;
            BurnTime = GetBurnTime();
        }
        public TPushPlan(TPushPlan t)
        {
            RoomNum = t.RoomNum;
            Period = t.Period;
            Group = t.Group;
            PushTime = t.PushTime;
            StokingTime = CokeRoom.BurnStatus[RoomNum].StokingTime;
            BurnTime = GetBurnTime();
        }
        public int Group { get; set; }
        public string StrGroup
        {
            get
            {
                string str = null;
                switch (Group)
                {
                    case 1:
                        str = "甲班";
                        break;
                    case 2:
                        str = "乙班";
                        break;
                    case 3:
                        str = "丙班";
                        break;
                    case 4:
                        str = "丁班";
                        break;
                    default:
                        str = "NO";
                        break;
                }
                return str;
            }
        }
        public int Period { get; set; }
        public string StrPeriod
        {
            get
            {
                string str = null;
                switch (Period)
                {
                    case 1:
                        str = "白班";
                        break;
                    case 2:
                        str = "夜班";
                        break;
                    default:
                        str = "NO";
                        break;
                }
                return str;
            }
        }
        public byte RoomNum
        {
            get { return (byte)GetValue(RoomNumProperty); }
            set { SetValue(RoomNumProperty, value); }
        }
        // Using a DependencyProperty as the backing store for RoomNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RoomNumProperty =
            DependencyProperty.Register("RoomNum", typeof(byte), typeof(TPushPlan), new PropertyMetadata((byte)0));

        public DateTime PushTime
        {
            get { return (DateTime)GetValue(PushTimeProperty); }
            set { SetValue(PushTimeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for PushTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PushTimeProperty =
            DependencyProperty.Register("PushTime", typeof(DateTime), typeof(TPushPlan), new PropertyMetadata(DateTime.Now));
        /// <summary>
        /// 计划结焦时间
        /// </summary>
        public int BurnTime
        {
            get { return (int)GetValue(BurnTimeProperty); }
            set { SetValue(BurnTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BurnTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BurnTimeProperty =
            DependencyProperty.Register("BurnTime", typeof(int), typeof(TPushPlan), new PropertyMetadata(0));

        public int StandardBurnTime
        {
            get { return Setting.BurnTime; }
        }

        public DateTime StokingTime
        {
            get { return (DateTime)GetValue(StokingTimeProperty); }
            set { SetValue(StokingTimeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for StokingTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StokingTimeProperty =
            DependencyProperty.Register("StokingTime", typeof(DateTime), typeof(TPushPlan), new PropertyMetadata(DateTime.Now));
        public string BurnTimeToString()
        {
            return (BurnTime / 60).ToString("00") + ":" + (BurnTime % 60).ToString("00");
        }
        public int GetBurnTime()
        {
            return (int)(PushTime - StokingTime).TotalMinutes;
        }
        public void GetCopyFrom(TPushPlan plan)
        {
            RoomNum = plan.RoomNum;
            PushTime = plan.PushTime;
        }
        public static int CompareByTime(TPushPlan p1, TPushPlan p2)
        {
            return p1.PushTime.CompareTo(p2.PushTime);
        }
    }
    class MStokingPlan : DependencyObject, IPlan
    {
        public MStokingPlan() { }
        public MStokingPlan(TPushPlan plan)
        {
            RoomNum = plan.RoomNum;
            //数字5的意义：计划装煤时间应在计划出焦时间之后的第五分钟
            StokingTime = plan.PushTime.AddMinutes(5);
            Period = (byte)plan.Period;
            Group = (byte)plan.Group;
        }
        public MStokingPlan(MStokingPlan m)
        {
            RoomNum = m.RoomNum;
            StokingTime = m.StokingTime;
            Period = m.Period;
            Group = m.Group;
        }
        public MStokingPlan(int room, DateTime time, int period, int group)
        {
            RoomNum = (byte)room;
            StokingTime = time;
            Period = (byte)period;
            Group = (byte)group;
        }
        /// <summary>
        /// 炉号
        /// </summary>
        public byte RoomNum
        {
            get { return (byte)GetValue(RoomNumProperty); }
            set { SetValue(RoomNumProperty, value); }
        }
        // Using a DependencyProperty as the backing store for RoomNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RoomNumProperty =
            DependencyProperty.Register("RoomNum", typeof(byte), typeof(MStokingPlan), new PropertyMetadata((byte)0));

        public byte Period { get; set; }
        public string StrPeriod { get { return Period == 1 ? "白班" : "夜班"; } }
        public byte Group { get; set; }
        public string StrGroup { get { return Group == 1 ? "甲班" : (Group == 2 ? "乙班" : (Group == 3 ? "丙班" : "丁班")); } }
        /// <summary>
        /// 计划装煤时间
        /// </summary>
        public DateTime StokingTime
        {
            get { return (DateTime)GetValue(PlanToStokingTimeProperty); }
            set { SetValue(PlanToStokingTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlanToStokingTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlanToStokingTimeProperty =
            DependencyProperty.Register("StokingTime", typeof(DateTime), typeof(MStokingPlan), new PropertyMetadata(Convert.ToDateTime("2012-04-03 13:14")));
        public void GetCopyFrom(MStokingPlan plan)
        {
            RoomNum = plan.RoomNum;
            StokingTime = plan.StokingTime;
        }
        public static int CompareByTime(MStokingPlan p1, MStokingPlan p2)
        {
            return p1.StokingTime.CompareTo(p2.StokingTime);
        }
    }
}
