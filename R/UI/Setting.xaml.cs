using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WGPM.R.RoomInfo;
using WGPM.Properties;
using System.Windows.Threading;

namespace WGPM.R.UI
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// 如果班组没有确定，不允许跳转至其他界面
    /// </summary>
    public partial class Setting : UserControl
    {
        public Setting()
        {
            InitializeComponent();
            Loaded += Setting_Loaded;
        }
        /// <summary>
        /// true 表示1/2#炉区在用；false表示3/4#炉区在用
        /// </summary>
        public static bool AreaFlag { get; set; }
        public static bool IsServer { get; set; }
        //public static bool AreaFlag { get { return false; } }
        Settings conn = new Settings();
        public static string ConnectionStr { get; set; }
        public static bool ScheduleFlag { get; set; }//修改Schedule
        private bool burnTimeFlag;//修改BurnTime
        private bool day;
        private bool night;
        public static int Hour { get { return BurnTime / 60; } }
        public static int Min { get { return BurnTime % 60; } }
        public static int BurnTime;
        public static string StrBurnTime
        {
            get
            {
                return Hour.ToString("00") + ":" + Min.ToString("00");
            }
        }
        DispatcherTimer timer = new DispatcherTimer();
        List<Schedule> scheduleLst;
        private void Setting_Loaded(object sender, RoutedEventArgs e)
        {
            #region 加载界面时，使班组和按钮处于可修改状态
            burnTimeFlag = true;
            btnSchedule.IsEnabled = false;
            btnBurnTime.IsEnabled = false;
            #endregion
            //引入班组Lst
            scheduleLst = CokeRoom.ScheduleLst;
            #region 根据推焦计划PushPlan得到当前班组；20171219 (白班，甲班)
            BurnTime = 19 * 60;//程序启动时默认规定结焦时间为19:00
            txtMins.Text = BurnTime.ToString();
            txtHour.Text = Hour.ToString("00");
            txtMin.Text = Min.ToString("00");
            int h = DateTime.Now.Hour;
            bool b1 = (h >= 8 && h < 20) ? true : false;//白班为true
            NowGroup = GetNowGroup();
            cboGroup.SelectedIndex = NowGroup - 1;
            cboPeriod.SelectedIndex = b1 ? 0 : 1;
            #endregion
            #region 设置bool变量day 、night
            int h1 = DateTime.Now.Hour;
            int min1 = DateTime.Now.Minute;
            day = (h1 >= 8 && h < 20) ? false : true;
            night = !day;
            #endregion
            #region 班组自动更新计时器
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += Timer_Tick;
            //timer.Start();
            #endregion
            ConnectionStr = AreaFlag ? conn.WGPM_CokeArea12 : conn.WGPM_CokeArea34;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            int h = DateTime.Now.Hour;
            if (h != 8 && h != 20) return;
            int min = DateTime.Now.Minute;
            //if (h != 8 && h != 20 || (h == 8 || h == 20 && min >= 3)) return;//本条语句似乎能提升效率？20170912
            if (h == 8 && min < 2 && day)
            {//夜班到白班 更新当前班组和当前
                day = false;
                night = true;
                GetNextScedule();
                return;
            }
            if (h == 20 && min < 2 && night)
            {//白班到夜班  更新为夜班的班次
                night = false;
                day = true;
                GetNextScedule();
            }
        }
        private int Group
        {
            get
            {
                return cboGroup.SelectedIndex;
            }
        }
        private int Period
        {
            get
            {
                return cboPeriod.SelectedIndex;
            }
        }
        public static int NowPeriod
        {
            get
            {
                int h = DateTime.Now.Hour;
                return (h >= 8 && h < 20) ? 1 : 2;
            }
        }
        public static int AnotherPeriod
        {
            get { return NowPeriod == 1 ? 2 : 1; }
        }

        public static String StrPeriod
        {
            get
            {
                return NowPeriod == 1 ? "白班" : "夜班";
            }
        }
        public static int LastGroup { get; set; }
        public static int NowGroup { get; set; }
        public static int NextGroup
        {
            get
            {
                NowGroup = GetNowGroup();
                int index = CokeRoom.ScheduleLst.FindIndex(x => x.period == NowPeriod && x.Group == NowGroup);
                if (index < 0) return -1;
                index = index < 7 ? index + 1 : 0;
                return CokeRoom.ScheduleLst[index].Group;
            }
        }
        public static string StrGroup
        {
            get
            {
                return NowGroup == 1 ? "甲班" : (NowGroup == 2 ? "乙班" : (NowGroup == 3 ? "丙班" : "丁班"));
            }
        }
        private void btnSchedule_Click(object sender, RoutedEventArgs e)
        {
            btnSchedule.IsEnabled = false;
            if (IsPeriodRight())
            {
                NowGroup = Group + 1;
            }
            else
            {//上个班的工作人员提前到，当时时间还是属于上次的
                Schedule s = new Schedule(Period + 1, Group + 1);//==>由所选的时段班组得到上一时段的班组
                int index = CokeRoom.ScheduleLst.FindIndex(x => x == s);
                if (index > 0)
                {
                    NowGroup = CokeRoom.ScheduleLst[index - 1].Group;
                }
                else
                {
                    NowGroup = scheduleLst[7].Group;
                }
            }
            MessageBox.Show("当前班次已更新！", "提示", MessageBoxButton.OK);
        }
        /// <summary>
        /// 判断修改Schedule时的时间和所选的时段是否相符
        /// 如果不相符一般情况为：下一班组提前到达所选其工作时间和班组
        /// </summary>
        /// <returns></returns>
        private bool IsPeriodRight()
        {
            return (NowPeriod == (Period + 1)) ? true : false;
        }
        private void GetNextScedule()
        {
            int index = scheduleLst.FindIndex(x => (x.period == (NowPeriod == 1 ? 2 : 1)) && (x.group == NowGroup));
            index = index < 7 ? (index + 1) : 0;
            NowGroup = scheduleLst[index].group;
        }
        private void Change_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                if (btn.Name == "btnChangeSchedule")
                {
                    btnSchedule.IsEnabled = true;
                }
                else
                {
                    burnTimeFlag = true;
                    btnBurnTime.IsEnabled = true;
                }
            }
        }

        private new void PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null) txt.SelectAll();
        }
        private new void KeyUp(object sender, KeyEventArgs e)
        {
            if (!burnTimeFlag) return;
            TextBox txt = sender as TextBox;
            if (txt == null) return;
            if (txt.Name == "txtMins")
            {//按分钟设置结焦时间
                BurnTime = Convert.ToInt32(txt.Text);
                txtHour.Text = Hour.ToString("00");
                txtMin.Text = Min.ToString("00");
            }
            else
            {
                if (txtHour.Text == null || txtMin.Text == null) return;
                int h = Convert.ToInt32(txtHour.Text);
                int m = Convert.ToInt32(txtMin.Text);
                BurnTime = h * 60 + m;
                txtMins.Text = BurnTime.ToString();
            }
        }

        private void btnBurnTime_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            burnTimeFlag = false;
            MessageBox.Show("结焦时间为：" + Hour.ToString("00") + ":" + Min.ToString("00"), "提示", MessageBoxButton.OK);
        }

        public static int GetNowGroup()
        {
            //每天分为三个阶段：hour<8 为前一天的夜班；hour>=20 为当天的夜班；其他时间为当天的白班；
            //(夜丁,白甲,夜乙：7,0,1)->(夜乙,白丙,夜甲：1,2,3,)->(夜甲,白丁,夜丙：3,4,5)->(夜丙,白乙,夜丁：5,6,7)->(夜丁,白甲,夜乙：7,0,1)
            DateTime startTime = Convert.ToDateTime("2017-12-19 00:00");
            int h = DateTime.Now.Hour;
            int days = (DateTime.Now - startTime).Days;
            int dIndex = days % 4;
            int index = 0;
            if (dIndex == 0)
            {
                index = h < 8 ? 7 : (h >= 20 ? 1 : 0);
            }
            else if (dIndex == 1)
            {
                index = h < 8 ? 1 : (h >= 20 ? 3 : 2);
            }
            else if (dIndex == 2)
            {
                index = h < 8 ? 3 : (h >= 20 ? 5 : 4);
            }
            else
            {
                index = h < 8 ? 5 : (h >= 20 ? 7 : 6);
            }
            return CokeRoom.ScheduleLst[index].Group;
        }
    }
}
