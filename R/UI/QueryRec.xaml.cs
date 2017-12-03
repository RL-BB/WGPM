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
using WGPM.R.LinqHelper;
using System.Data;
using Microsoft.Expression.Shapes;
using System.Globalization;

namespace WGPM.R.UI
{
    public delegate DataTable QueryItemDelegate();
    /// <summary>
    /// QueryRec.xaml 的交互逻辑
    /// </summary>
    public partial class QueryRec : UserControl
    {
        private QueryItemDelegate QueryItems;
        public QueryRec()
        {

            InitializeComponent();
            dpStart.SelectedDate = DateTime.Now.Date;
            dpEnd.SelectedDate = DateTime.Now.Date;
        }
        private List<LockInfoHelper> lockInfoSource;
        private List<CurHelper> curSource;
        private DataTable table;
        private int SelectTag;
        public DataTable Table
        {
            get
            {
                return table;
            }

            set
            {
                table = value;
            }
        }
        private DateTime Start
        {
            get
            {
                return dpStart.SelectedDate.Value.AddHours(8);
            }
        }
        private DateTime End
        {
            get
            {
                return dpEnd.SelectedDate.Value.AddDays(1).AddHours(8);
            }
        }
        private string Group
        {
            get
            {
                int i = cboGroup.SelectedIndex;
                string s = i == 0 ? "" : (ByteToGroup((byte)i));
                return s;
            }
        }
        private string Period
        {
            get
            {
                int i = cboPeriod.SelectedIndex;
                string s = null;
                s = i == 0 ? null : (i == 1 ? "白班" : "夜班");
                return s;
            }
        }

        private bool Lock
        {
            get { return chkLock.IsChecked.Value; }
        }
        private bool Unlock
        {
            get { return chkUnlock.IsChecked.Value; }
        }
        private byte LockStatus
        {
            get
            {
                int l = 0;
                if (Lock && !Unlock)
                {
                    l = 1;
                }
                else if (!Lock && Unlock)
                {
                    l = 2;
                }
                return (byte)l;
            }
        }
        DataTable queryTable = new DataTable();
        DataTable prdTable = new System.Data.DataTable();
        DataTable grpTable = new System.Data.DataTable();
        DataTable sourceTable = new System.Data.DataTable();
        List<SumHelper> sumHelper = new List<SumHelper>();

        private void LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if ((bool)rbtnSum.IsChecked)
            {
                SumHelper sum = (SumHelper)e.Row.DataContext;
                //if (sum.PushCur>=250)
                //{
                //    e.Row.Background = Brushes.Red;
                //}
                //else if (sum.PushCur>=220)
                //{
                //    e.Row.Background = Brushes.Green;
                //}
                if (sum.BurnTime > Setting.BurnTime + 60 * 4)
                {
                    e.Row.Background = Brushes.Red;
                }
            }
            else
            {
                int index = e.Row.GetIndex() + 1;
                e.Row.Header = index;
            }
        }
        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            //计划查询：
            //日期，1炉号，3实际推焦时间，时段，班组2预定出焦时间，，4上次装煤时间，5实际结焦时间，6计划结焦时间，7规定结焦时间，8实际结焦时间
            //起始时间：Start，结束时间End
            //dgQuery.DataContext = helper.QueryPlanDB(start, end).DefaultView;
            byte room = GetQueryRoom();
            if (room < 0 || room > 110)
            {
                MessageBox.Show("炉号应在001-110之间，请确保输入的炉号正确！");
                return;
            }
            ResetOptions();
            //QueryItems = QueryItem ? (QueryItemDelegate)GetPlan : GetRec;
            if (SelectTag < 6 && SelectTag != 3)
            {
                //table = QueryItems();
                Dispatcher.BeginInvoke(new Action(ResultDataTable), null);
            }
            else
            {
                dgLockInfo.ItemsSource = null;
                if (SelectTag == 6)
                {
                    Dispatcher.BeginInvoke(new Action(ResultLockInfo), null);
                }
                else if (SelectTag == 7)
                {
                    //curSource = QueryCur();
                    Dispatcher.BeginInvoke(new Action(ResultCurData), null);
                }
                else
                {//tag=3  统计数据
                    Dispatcher.BeginInvoke(new Action(ResultSumData), null);
                }
            }
        }
        /// <summary>
        /// 查询：推焦记录，平煤记录，推焦计划，登陆信息
        /// </summary>
        private void ResultDataTable()
        {
            DataTable table = QueryItems();
            if (table != null)
            {
                dgQuery.ItemsSource = null;
                dgQuery.ItemsSource = table.DefaultView;
            }
            else
            {
                dgQuery.ItemsSource = null;
                MessageBox.Show("查询结果为空");
            }
        }
        private void ResultSumData()
        {
            PushInfoHelper helper = new PushInfoHelper();
            List<SumHelper> lst = helper.QuerySumInfo(Start, cboPeriod.SelectedIndex);
            dgSum.ItemsSource = lst;
            if (lst == null || lst.Count == 0) return;
            dgSum1.ItemsSource = helper.AddLastRow(lst).DefaultView;
        }
        private DataTable QueryPushInfo()
        {
            PushInfoHelper helper = new PushInfoHelper();
            return helper.QueryRec(Start, End, GetQueryRoom());
        }
        private DataTable QuerySumInfo()
        {
            PushInfoHelper helper = new PushInfoHelper();
            sumHelper = helper.QuerySumInfo(Start, cboPeriod.SelectedIndex);
            return helper.ListToDataTable(sumHelper);
        }
        private DataTable QueryPingInfo()
        {
            PingInfoHelper helper = new PingInfoHelper();
            return helper.Query(Start, End, GetQueryRoom());
        }
        private void ResultLockInfo()
        {
            dgLockInfo.ItemsSource = null;
            PushInfoHelper helper = new PushInfoHelper();
            dgLockInfo.ItemsSource = helper.QueryLockInfo(Start, End, GetQueryRoom());
        }
        private void ResultCurData()
        {
            PushInfoHelper helper = new PushInfoHelper();
            curSource = helper.QueryCurInfo(Start, End, GetQueryRoom());
            dgPlotter.ItemsSource = null;
            dgPlotter.ItemsSource = curSource;
        }
        private DataTable QueryPlanDB()
        {
            EditPlanHelper helper = new EditPlanHelper();
            return helper.QueryPlanDB(Start, End);
        }
        private DataTable QueryLogInfo()
        {
            DateTime time = DateTime.Now;
            DateTime start = time;
            DateTime end = time;
            if (dpStart.SelectedDate != null)
            {
                start = dpStart.SelectedDate.Value;
            }
            if (dpEnd.SelectedDate != null)
            {
                end = dpEnd.SelectedDate.Value.AddDays(1).AddMilliseconds(-1);
            }
            ActivateInfo act = new ActivateInfo(start, end);
            return act.QueryLogIn();
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
                    strValue = "全部";
                    break;
            }
            return strValue;
        }
        private void ResetOptions()
        {
            cboGroup.SelectedIndex = 0;
            if (!(bool)rbtnSum.IsChecked) cboPeriod.SelectedIndex = 0;
            chkLock.IsChecked = false;
            chkUnlock.IsChecked = false;
        }
        private void cbo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rbtnPlan == null) return;
            ComboBox cbo = sender as ComboBox;
            if (cbo == null) return;
            DataTable table = new DataTable();
            //通过不同表的不同列数来判断是否为统计表，如果是，return；
            //rbtnPlan.IsChecked;
            DataTable dt = rbtnPlan.IsChecked.Value ? queryTable : GetTableByLockStatus(queryTable);
            table = cbo.Name == "cboGroup" ? GetTableByGroup(GetTableByPeriod(dt)) : GetTableByPeriod(GetTableByGroup(dt));
            dgQuery.ItemsSource = null;
            dgQuery.ItemsSource = table.DefaultView;
        }
        private DataTable GetTableByPeriod(DataTable dt)
        {
            DataTable table = new DataTable();
            if (cboPeriod.SelectedIndex == 0)
            {
                table = dt.Copy();
            }
            else
            {
                table = dt.Clone();
                foreach (DataRow row in dt.Rows)
                {
                    DataRow r = table.NewRow();
                    if (((string)row[3]) == Period)
                    {
                        r.ItemArray = row.ItemArray;
                        table.Rows.Add(r);
                    }
                }
            }
            return table;
        }
        private DataTable GetTableByGroup(DataTable dt)
        {
            DataTable table = new DataTable();
            if (cboGroup.SelectedIndex == 0)
            {
                table = dt.Copy();
            }
            else
            {
                table = dt.Clone();
                foreach (DataRow row in dt.Rows)
                {
                    DataRow r = table.NewRow();
                    if (((string)row[4]) == Group)
                    {
                        r.ItemArray = row.ItemArray;
                        table.Rows.Add(r);
                    }
                }
            }
            return table;
        }
        private DataTable GetTableByLockStatus(DataTable dt)
        {
            DataTable table = new DataTable();
            if (LockStatus == 1)
            {
                table = dt.Copy();
            }
            else
            {
                table = dt.Clone();
                foreach (DataRow row in dt.Rows)
                {
                    DataRow r = table.NewRow();
                    if (((byte)row[5]) == LockStatus)
                    {
                        r.ItemArray = row.ItemArray;
                        table.Rows.Add(r);
                    }
                }
            }
            return table;
        }
        private void chk_Checked(object sender, RoutedEventArgs e)
        {//联锁，解锁
            //根据queryTable的Columns（列数）来判断DataGrid显示的为哪个！如果是Plan，则return；
            CheckBox chk = sender as CheckBox;
            if (queryTable.Rows.Count == 0) return;//还未查询
            if (queryTable.Columns.Count != 13) return;
            DataTable table = GetTableByLockStatus(GetTableByGroup(GetTableByPeriod(queryTable)));
            dgQuery.ItemsSource = null;
            dgQuery.ItemsSource = table.DefaultView;
        }
        private void rbtn_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rbtn = sender as RadioButton;
            if (rbtn != null)
            {
                SelectTag = Convert.ToInt32(rbtn.Tag);
                ChooseQueryItem(SelectTag);
                //并不是每次都需要执行 第一个if 注意考虑优化问题；
                if (SelectTag != 3)
                {
                    cboPeriod.Items.Clear();
                    cboPeriod.Items.Add(new ComboBoxItem { Content = "全部时段", IsSelected = true });
                    cboPeriod.Items.Add(new ComboBoxItem { Content = "白班" });
                    cboPeriod.Items.Add(new ComboBoxItem { Content = "夜班" });
                }
                else
                {
                    cboPeriod.Items.Clear();
                    cboPeriod.Items.Add(new ComboBoxItem { Content = "白班", IsSelected = true });
                    cboPeriod.Items.Add(new ComboBoxItem { Content = "中班" });
                    cboPeriod.Items.Add(new ComboBoxItem { Content = "夜班" });
                }
            }
        }
        private void ChooseQueryItem(int tag)
        {
            switch (tag)
            {
                case 1://推焦
                    tab.SelectedIndex = 0;
                    QueryItems = QueryPushInfo;
                    break;
                case 2://平煤
                    tab.SelectedIndex = 0;
                    QueryItems = QueryPingInfo;
                    break;
                case 3://统计(未做);20171019 已做
                    tab.SelectedIndex = 1;
                    break;
                case 4://计划
                    tab.SelectedIndex = 0;
                    QueryItems = QueryPlanDB;
                    break;
                case 5://登陆
                    tab.SelectedIndex = 0;
                    QueryItems = QueryLogInfo;
                    break;
                case 6://联锁(未做)
                    tab.SelectedIndex = 2;
                    break;
                case 7://电流
                    tab.SelectedIndex = 3;
                    break;
                default:
                    tab.SelectedIndex = 0;
                    QueryItems = QueryPushInfo;
                    break;
            }
        }

        #region ForLockInfoAndCurQuery
        private void GetSysInfo()
        {
            //系统时间
            //together.txtSysTime.Text = DateTime.Now.ToString("d");
            //// 计划推焦炉号和时间
            //together.txtPushPlanRoomNum.Text = null;
            //together.txtPushPlanTime.Text = null;
            ////计划装煤炉号和时间
            //together.txtStokingPlanRoomNum.Text = "000";
            //together.txtStokingPlanTime.Text = "00:00";
        }
        #endregion

        private void txtRoom_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null)
            {
                txt.SelectAll();
            }
        }

        private void SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker dp = sender as DatePicker;
            if (dp != null && IsDateSelected())
            {
                DateTime date = dp.SelectedDate.Value;
                if (dp.Name == "dpStart")
                {
                    DateTime dEnd = dpEnd.SelectedDate.Value;
                    dpEnd.SelectedDate = date > dEnd ? (DateTime?)date : dEnd;
                }
                else
                {
                    DateTime dStart = dpStart.SelectedDate.Value;
                    dpStart.SelectedDate = dStart < date ? (DateTime?)dStart : date;
                }
            }
        }
        private bool IsDateSelected()
        {
            return dpStart.SelectedDate != null && dpEnd.SelectedDate != null;
        }
        #region 联锁信息数据显示
        private void dgLockInfo_Selected(object sender, SelectionChangedEventArgs e)
        {
            LockInfoHelper helper = dgLockInfo.SelectedItem as LockInfoHelper;
            if (helper == null) return;
            TimeInfo(helper);
            RoomInfo(helper);
            ReadyInfo(helper);
            LockInfo(helper);
        }
        private void TimeInfo(LockInfoHelper helper)
        {
            together.txtSysTime.Text = helper.ActualPushTime != null ? Convert.ToDateTime(helper.ActualPushTime).ToString("t") : "00:00";
            together.txtPushPlanTime.Text = helper.PushTime != null ? Convert.ToDateTime(helper.PushTime).ToString("t") : "00:00";
            together.txtPushPlanRoomNum.Text = helper.Room != null ? helper.Room : "000";
            //together.txtStokingPlanRoomNum.Text = together.txtPushPlanRoomNum.Text;
            //together.txtStokingPlanTime.Text = helper.PushTime != null ? Convert.ToDateTime(helper.PushTime).AddMinutes(5).ToString("t") : "00:00";
        }
        private void RoomInfo(LockInfoHelper helper)
        {
            int area = Setting.AreaFlag ? 0 : 2;
            together.txtTJobCar.Text = helper.TRoom != null ? (Convert.ToInt32(helper.TRoom) <= 55 ? 1 : 2) + area + "#" + helper.TRoom : "#000";
            together.txtLJobCar.Text = helper.LRoom != null ? (Convert.ToInt32(helper.LRoom) <= 55 ? 1 : 2) + area + "#" + helper.LRoom : "#000";
            together.txtXJobCar.Text = helper.XRoom != null ? (Convert.ToInt32(helper.XRoom) <= 55 ? 1 : 2) + area + "#" + helper.XRoom : "#000";
            //together.txtMJobCar.Text = helper.MRoom != null ? (Convert.ToInt32(helper.MRoom) <= 55 ? 1 : 2) + area + "#" + helper.MRoom : "#000";
        }
        private void ReadyInfo(LockInfoHelper helper)
        {
            GetAllJobCarArrows(helper);
        }
        private void GetAllJobCarArrows(LockInfoHelper helper)
        {
            //BlockArrow[] tArrows = { together.tArrow01, together.tArrow011, together.tArrow02, together.tArrow03, together.tArrow04, together.tArrow041 };
            //BlockArrow[] lArrows = { together.lArrow01, together.lArrow011, together.lArrow02, together.lArrow03, together.lArrow04, together.lArrow041 };
            //BlockArrow[] xArrows = { together.xArrow01, together.xArrow011, together.xArrow02, together.xArrow03, together.xArrow04, together.xArrow041 };
            //together.tReady.Fill = GetJobCarArrows(helper.Ready, 0, tArrows) ? Brushes.Red : Brushes.Gray;
            //together.lReady.Fill = GetJobCarArrows(helper.Ready, 1, lArrows) ? Brushes.Red : Brushes.Gray;
            //together.xReady.Fill = GetJobCarArrows(helper.Ready, 2, xArrows) ? Brushes.Red : Brushes.Gray;
        }
        private bool GetJobCarArrows(string arrow, int index, BlockArrow[] arrows)
        {
            int a = Convert.ToInt32(arrow);
            int ready = 0;
            for (int i = 0; i < 4; i++)//每四bit为一车的对中情况：TLXM
            {
                bool b = Convert.ToBoolean(a & (int)Math.Pow(2, i + index * 4));
                ready = ready + (b ? (int)Math.Pow(2, i) : 0);
            }
            switch (ready)
            {
                case 3://左俩
                    for (int i = 0; i < arrows.Length; i++)
                    {
                        if (i <= 2)
                        {
                            arrows[i].Fill = Brushes.Red;
                        }
                        else
                        {
                            arrows[i].Fill = Brushes.Gray;
                        }
                    }
                    break;
                case 2://左1
                    for (int i = 0; i < arrows.Length; i++)
                    {
                        if (i == 2)
                        {
                            arrows[2].Fill = Brushes.Red;
                        }
                        else
                        {
                            arrows[i].Fill = Brushes.Gray;
                        }
                    }
                    break;
                case 4://右1
                    for (int i = 0; i < arrows.Length; i++)
                    {
                        if (i == 3)
                        {
                            arrows[3].Fill = Brushes.Red;
                        }
                        else
                        {
                            arrows[i].Fill = Brushes.Gray;
                        }
                    }
                    break;
                case 12://右俩
                    for (int i = 0; i < arrows.Length; i++)
                    {
                        if (i >= 3)
                        {
                            arrows[i].Fill = Brushes.Red;
                        }
                        else
                        {
                            arrows[i].Fill = Brushes.Gray;
                        }
                    }
                    break;
                default:
                    for (int i = 0; i < arrows.Length; i++)
                    {
                        arrows[i].Fill = Brushes.Gray;
                    }
                    break;
            }
            return ready == 0 ? true : false;
        }
        private void LockInfo(LockInfoHelper helper)
        {
            //DataWrite.cs  InfoToInt 3 8 11 17 xx 9 12 18
            together.txtPushTogethor.Background = (helper.PushInfo & (int)Math.Pow(2, 1)) == Math.Pow(2, 1) ? Brushes.Red : Brushes.Gray;//联锁信息
            together.txtTjcDoorOpen.Background = (helper.PushInfo & (int)Math.Pow(2, 3)) == Math.Pow(2, 3) ? Brushes.Red : Brushes.Gray;//炉门已摘
            together.rectLinkTjcDoorOpen.Fill = together.txtTjcDoorOpen.Background;//
            together.txtLjcTroughLocked.Background = (helper.PushInfo & (int)Math.Pow(2, 8)) == Math.Pow(2, 8) ? Brushes.Red : Brushes.Gray;//焦槽锁闭
            together.rectLinkLjcTrough.Fill = together.txtLjcTroughLocked.Background;//
            together.txtCanReady.Background = (helper.PushInfo & (int)Math.Pow(2, 11)) == Math.Pow(2, 11) ? Brushes.Red : Brushes.Gray;//焦罐旋转/车门关闭
            together.rectLinkXjcCan.Fill = together.txtCanReady.Background;
            //together.txtMcSleeve.Background = Brushes.Green;//导套到位
            //together.rectMLinkAllowPing.Fill = Brushes.Green;
            together.txtFstAllow.Background = (helper.PushInfo & (int)Math.Pow(2, 17)) == Math.Pow(2, 17) ? Brushes.Red : Brushes.Gray;//一级允推
            together.rectTLinkFstAllow.Fill = together.txtFstAllow.Background;
            together.rectLLinkFstAllow.Fill = together.txtFstAllow.Background;
            together.rectXLinkFstAllow.Fill = together.txtFstAllow.Background;
            together.txtTimeAllow.Background = (int)(Convert.ToDateTime(helper.ActualPushTime) - Convert.ToDateTime(helper.PushTime)).TotalMinutes >= -5 ? Brushes.Red : Brushes.Gray;//时间允许
            together.txtLjcAllowPush.Background = (helper.PushInfo & (int)Math.Pow(2, 9)) == Math.Pow(2, 9) ? Brushes.Red : Brushes.Gray;//拦人工允推
            together.txtXjcAllowPush.Background = (helper.PushInfo & (int)Math.Pow(2, 12)) == Math.Pow(2, 12) ? Brushes.Red : Brushes.Gray;//熄人工允推
            together.txtSecAllow.Background = (helper.PushInfo & (int)Math.Pow(2, 18)) == Math.Pow(2, 18) ? Brushes.Red : Brushes.Gray;//二级允推
            together.rectTimeAllowLink.Fill = together.txtSecAllow.Background;
            together.rectLLinkSecAllow.Fill = together.txtSecAllow.Background;
            together.rectXLinkSecAllow.Fill = together.txtSecAllow.Background;
        }
        #endregion
        private void dgPlotter_Selected(object sender, SelectionChangedEventArgs e)
        {
            CurHelper helper = dgPlotter.SelectedItem as CurHelper;
            if (helper == null) return;
            string mesg = string.Empty;
            if (helper.PushPole == null || helper.PushCur == null)
            {
                mesg = "当前炉号：" + helper.Room + " 的推焦电流信息未做记录！\n";
            }
            mesg += (helper.PingPole == null || helper.PingCur == null ? "当前炉号：" + helper.Room + " 的平煤电流信息未做记录！" : null);
            if (mesg.Length > 5)
            {
                MessageBox.Show(mesg);
            }
            curLotter.Plotter(helper.PushPole, helper.PushCur, helper.PingPole, helper.PingCur);
        }
        /// <summary>
        /// 判断是否按炉号查询，如果按炉号查询且炉号不在[0,100]之间时则报错
        /// 如果CheckBox没有选中，则返回值为0
        /// 如果选中，输入的数据有误时，点击“查询”时，报错
        /// </summary>
        /// <returns></returns>
        private byte GetQueryRoom()
        {
            if (!chkRoom.IsChecked.Value) return 0;
            int room = 0;
            int.TryParse(txtRoom.Text, out room);

            return (byte)room;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgSum_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }
    }
}
