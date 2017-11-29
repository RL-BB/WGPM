using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WGPM.R.RoomInfo;
using WGPM.R.Sys;
using WGPM.R.XML;
using System.IO;
using System.Windows.Media;
using WGPM.R.LinqHelper;
using System.Diagnostics;
using NPOI.XSSF.UserModel;
using System.Windows.Data;
using System.Windows.Threading;

namespace WGPM.R.UI
{
    /// <summary>
    ///     PlanEdit.xaml 的交互逻辑
    /// </summary>
    public partial class PlanEdit
    {
        int repeatCount = 0;
        int tabIndex = 0;
        //private List<C.ModelPlanTM> _gridBingingData = new List<C.ModelPlanTM>();
        private bool isEditing; //是否处于编辑状态
        private List<TPushPlan> editingTPlan = new List<TPushPlan>(); //储存暂时点btnEnsure保存的数据
        private List<TPushPlan> displayPlan = new List<TPushPlan>();
        private int BreakTime
        {
            get
            {
                try
                {
                    return Convert.ToInt32(txtBreak.Text);
                }
                catch (Exception)
                {

                    MessageBox.Show("出焦周转时间设置错误，请检查后重新录入！");
                    return 9;
                }
            }
        }
        public PlanEdit()
        {
            InitializeComponent();
            Loaded += PlanEdit_Loaded;
        }
        private void PlanEdit_Loaded(object sender, RoutedEventArgs e)
        {
            dpDate.SelectedDate = DateTime.Now.Date;
            BindingDataContext(dgPlan);
            string[] content = Setting.AreaFlag ? new string[] { "1-2#炉区", "1#炉区", "2#炉区" } : new string[] { "3-4#炉区", "3#炉区", "4#炉区" };
            ComboBoxItem cbo = new ComboBoxItem() { Content = content[0], IsSelected = true, HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left, VerticalContentAlignment = System.Windows.VerticalAlignment.Center };
            ComboBoxItem print1 = new ComboBoxItem() { Content = content[1], IsSelected = true, HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left, VerticalContentAlignment = System.Windows.VerticalAlignment.Center };
            ComboBoxItem print2 = new ComboBoxItem() { Content = content[2], HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left, VerticalContentAlignment = System.Windows.VerticalAlignment.Center };
            cboArea.Items.Clear();
            cboArea.Items.Add(cbo);
            cboPrintArea.Items.Clear();
            cboPrintArea.Items.Add(print1);
            cboPrintArea.Items.Add(print2);
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {//DataGrid  LoadingRow时，设置ItemsSouce
            int index = e.Row.GetIndex() + 1;
            e.Row.Header = index;
        }
        private void CheckBox_PlanBurnTime_Click(object sender, RoutedEventArgs e)
        {
            //TextBoxPlanBurnTime.IsEnabled = Convert.ToBoolean(CheckBoxPlanBurnTime.IsChecked);
        }
        private void ChkBurnTime_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk != null)
            {
                bool check = chk.IsChecked.Value;
                List<TPushPlan> plan = isEditing ? editingTPlan : CokeRoom.PushPlan;
                if (isEditing && editingTPlan.Count > 0)
                {//重置装煤时间
                    for (int i = 0; i < editingTPlan.Count; i++)
                    {
                        editingTPlan[i].StokingTime = CokeRoom.BurnStatus[editingTPlan[i].RoomNum].StokingTime;
                    }
                }
                if (plan.Count == 0) return;
                Random r = new Random();
                for (int i = 0; i < plan.Count; i++)
                {
                    //if (Math.Abs(Setting.BurnTime - plan[i].BurnTime) > 5)
                    //{
                    plan[i].BurnTime = check ? (Setting.BurnTime + r.Next(-5, 6)) : (int)(plan[i].PushTime - plan[i].StokingTime).TotalMinutes;
                    if (check)
                    {
                        if (Math.Abs(Setting.BurnTime - plan[i].BurnTime) > 5)
                        {
                            plan[i].BurnTime = Setting.BurnTime + r.Next(-5, 6);
                        }
                    }
                    else
                    {
                        plan[i].BurnTime = (int)(plan[i].PushTime - plan[i].StokingTime).TotalMinutes;

                    }
                }
                ItemsControl control = tabEdit.IsSelected ? dgEdit : dgPlan;
                control.ItemsSource = null;
                control.ItemsSource = plan;
            }
        }
        private void Time_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "00:00")
            {
                return;
            }
            if (txt != null && txt.Text.Length == 2)
            {
                txt.Text = txt.Text + ":";
                txt.SelectionStart = txt.Text.Length;
            }
            if (txt == null || txt.Text.Length != 5) return;
            //if (txt.Name == "TextBoxPlanPushTime")
            //{
            //    if (Convert.ToBoolean(CheckBoxPlanBurnTime.IsChecked))
            //    {
            //        if (DatePickerSelectTime.SelectedDate != null)
            //        {
            //            DateTime t = Convert.ToDateTime(DatePickerSelectTime.SelectedDate.Value).AddHours(
            //                Convert.ToDouble(TextBoxPlanPushTime.Text.Substring(0, 2))).AddMinutes(
            //                    Convert.ToDouble(TextBoxPlanPushTime.Text.Substring(3, 2)));
            //            TimeSpan s = t - DataRun.Rd[Convert.ToInt32(TextBoxPlanRoom.Text) - 1].RealCoalTime;
            //            //如果计算出来的计划结焦时间在标准结焦时间内一小时，则认为是对的，进行赋值，如果不是则返回标准结焦时间
            //            TextBoxPlanBurnTime.Text = (Math.Abs(s.TotalMinutes - DataSystem.SysStandardBurnTime[ComboBoxPlanNumber.SelectedIndex]) <= DataSystem.SysStandardBurnPlanMinus[ComboBoxPlanNumber.SelectedIndex]) ? (s.Days * 24 + s.Hours).ToString("00") + ":" + s.Minutes.ToString("00") : C.GetHourTime(DataSystem.SysStandardBurnTime[ComboBoxPlanNumber.SelectedIndex]);
            //        }
            //        TextBoxPlanBurnTime.Focus();
            //        TextBoxPlanBurnTime.SelectAll();
            //    }
            //    else
            //    {
            //        if (Convert.ToBoolean(CheckBoxStandardBurnTime.IsChecked))
            //        {
            //            TextBoxStandardBurnTime.Focus();
            //            TextBoxStandardBurnTime.SelectAll();
            //        }
            //        else
            //        {
            //            ButtonPlanEnter.Focus();
            //        }
            //    }
            //}
            //else
            //{
            //    ButtonPlanEnter.Focus();
            //}
        }
        private void Buttonr_Click(object sender, RoutedEventArgs e)
        {
            //if (DatePickerSelectTime.SelectedDate == null) return;
            //DateTime t = DatePickerSelectTime.SelectedDate.Value +
            //             TimeSpan.FromHours(Convert.ToDouble(TextBoxPlanPushTime.Text.Substring(0, 2))) +
            //             TimeSpan.FromMinutes(Convert.ToDouble(TextBoxPlanPushTime.Text.Substring(3, 2)));
            //if (ComboBoxPeriod.SelectedIndex == ComboBoxPeriod.Items.Count - 1)
            //{
            //    MessageBox.Show(@"请选择【时段】");
            //    return;
            //}
            //if (ComboBoxClass.SelectedIndex == ComboBoxClass.Items.Count - 1)
            //{
            //    MessageBox.Show(@"请选择【班组】");
            //    return;
            //}
            //if (ComboBoxPlanNumber.SelectedIndex == ComboBoxPlanNumber.Items.Count - 1)
            //{
            //    MessageBox.Show("请选择【计划】号");
            //    return;
            //}
            //if (TextBoxPlanRoom.Text.Length != 3)
            //{
            //    MessageBox.Show(@"【计划炭化室号】格式有误，格式为“012”");
            //    return;
            //}
            //if (Convert.ToInt32(TextBoxPlanRoom.Text) < DataSystem.SysRoomPlanStart[ComboBoxPlanNumber.SelectedIndex] ||
            //    Convert.ToInt32(TextBoxPlanRoom.Text) > DataSystem.SysRoomPlanEnd[ComboBoxPlanNumber.SelectedIndex])
            //{
            //    MessageBox.Show(@"【计划炭化室号】超出范围，范围为" +
            //                    DataSystem.SysRoomPlanStart[ComboBoxPlanNumber.SelectedIndex].ToString("000") + "—" +
            //                    DataSystem.SysRoomPlanEnd[ComboBoxPlanNumber.SelectedIndex].ToString("000"));
            //    return;
            //}
            //if (!C.CheckTextTime(TextBoxPlanPushTime.Text))
            //{
            //    MessageBox.Show(@"【计划推焦时间】格式有误，格式为“12:23”");
            //    return;
            //}
            //if (!C.IsInPlanPeriodTime(DatePickerSelectTime.SelectedDate.Value, ComboBoxPeriod.SelectedIndex, t))
            //{
            //    MessageBox.Show(@"【计划推焦时间】不在【时段】范围内");
            //    return;
            //}
            //if (isEnter)
            //{
            //    C.GetPlanList(DatePickerSelectTime.SelectedDate.Value, ComboBoxPeriod.SelectedIndex,
            //        ComboBoxClass.SelectedIndex, ComboBoxPlanNumber.SelectedIndex, 0);
            //    _tempGridBingingData = C.ListData;
            //    isEnter = false;
            //}
            //C.GetPlanDataOne(TextBoxPlanRoom.Text, 0);
            //C.ModelPlanTM newModelPlan = C.PlanDataOne;
            //newModelPlan.Room = TextBoxPlanRoom.Text;
            //newModelPlan.Period = DataSystem.SysPeriod[ComboBoxPeriod.SelectedIndex];
            //newModelPlan.Class = DataSystem.SysClass[ComboBoxClass.SelectedIndex];
            //newModelPlan.PlanNum = ComboBoxPlanNumber.SelectedIndex.ToString(CultureInfo.InvariantCulture);
            ////NewModelPlan.PushTime,不需要记录
            //newModelPlan.PlanTime = t.ToString("yyyy/MM/dd HH:mm:ss");
            //newModelPlan.BurnTime = TextBoxPlanBurnTime.Text;
            //newModelPlan.StandardBurnTime = TextBoxStandardBurnTime.Text;
            //newModelPlan.Invalid = "0"; //将计划置为有效
            //_tempGridBingingData.Add(newModelPlan);
            //DataGrid.ItemsSource = null;
            //DataGrid.ItemsSource = _tempGridBingingData;

            ////更新下一个计划的信息
            //TextBoxPlanRoom.Text =
            //    C.GetNextRoom(DataSystem.SysOrder1, DataSystem.SysOrder2, 1, Convert.ToInt32(TextBoxPlanRoom.Text),
            //        DataSystem.SysRoomPlanStart[ComboBoxPlanNumber.SelectedIndex],
            //        DataSystem.SysRoomPlanEnd[ComboBoxPlanNumber.SelectedIndex]).ToString("000");
            //TextBoxPlanPushTime.Text =
            //    (t + TimeSpan.FromMinutes(DataSystem.SysPerPushingTime[ComboBoxPlanNumber.SelectedIndex])).ToString(
            //        "HH:mm");
        }
        private void Text_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null) txt.SelectAll();
        }
        private void Text_KeyDown(object sender, KeyEventArgs e)
        {
            //如果是enter键
            if (e.Key == Key.Enter)
            {
                System.Windows.Controls.TextBox tempTextBox = sender as System.Windows.Controls.TextBox;
                if (tempTextBox != null && tempTextBox.Name == "TextBoxPlanBurnTime")
                {
                    Buttonr_Click(null, null);
                }
            }
            //只允许输入数字键+delete+backspace+tab+left+right
            if (e.Key != Key.D0 && e.Key != Key.D1 && e.Key != Key.D2 && e.Key != Key.D3 && e.Key != Key.D4
                && e.Key != Key.D5 && e.Key != Key.D6 && e.Key != Key.D7 && e.Key != Key.D8 && e.Key != Key.D9
                && e.Key != Key.NumPad0 && e.Key != Key.NumPad1 && e.Key != Key.NumPad2 && e.Key != Key.NumPad3 &&
                e.Key != Key.NumPad4
                && e.Key != Key.NumPad5 && e.Key != Key.NumPad6 && e.Key != Key.NumPad7 && e.Key != Key.NumPad8 &&
                e.Key != Key.NumPad9
                && e.Key != Key.Delete && e.Key != Key.Back && e.Key != Key.Tab && e.Key != Key.Left &&
                e.Key != Key.Right)
            {
                e.Handled = true;
            }
        }
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox txt = sender as TextBox;
            //txtRoom和 txtInsertRoom ;编辑计划的TextBox和乱笺的TextBox  for RoomNum
            if (txt.Text.Length == 3)
            {

                byte room = Convert.ToByte(txt.Text.Trim());
                if (room >= 1 && room <= 110)
                {
                    if (txt == txtRoom)
                    {
                        txtPushTime.Focus();
                        txtPushTime.SelectAll();
                        return;
                    }
                    else
                    {
                        txtInsertPushTime.Focus();
                        txtInsertPushTime.SelectAll();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("炉号录入错误！");
                    txt.Focus();
                    txt.SelectAll();
                    return;
                }
            }
        }
        public void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//分两种情况：编辑计划状态和非编辑计划状态；
            ComboBox cbo = sender as ComboBox;
            #region 界面初始化
            if (cbo != null && dgPlan != null && cboGroup != null)
            {
                if (isEditing)
                {//编辑状态时的dgPlan和dgEdit的ItemsSource
                    #region 处于编辑状态时 dgPlan的ItemsSouce=PushPlan ，方便删除重复计划 20170921
                    AssignDataGridItemsSource(0, 0);
                    #endregion 
                    #region  20170921 dgEdit的ItemsSouce  editingPlan=GetItemsSource(Period,Group)
                    editingTPlan = GetItemsSource(cboPeriod.SelectedIndex, cboGroup.SelectedIndex);
                    dgEdit.ItemsSource = null;
                    dgEdit.ItemsSource = editingTPlan;
                    #endregion
                }
                else
                {
                    AssignDataGridItemsSource(cboPeriod.SelectedIndex, cboGroup.SelectedIndex);
                }
                if (cbo.Name == "cboPeriod")
                {//打印计划的时段cboPrintPeriod.SelectedIndex随 计划编辑的cboPeriod.SelectedIndex变化
                    GetSelectedIndex(cbo.SelectedIndex);
                }
            }
            #endregion
        }
        private void txtPushTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text.Length == 2)
            {
                txt.Text = txt.Text + ":";
                txt.SelectionStart = txt.Text.Length;
            }
            if (txt == null || txt.Text.Length != 5) return;
            if (txt != null && txt.Text == "00:00")
            {
                return;
            }
        }
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            //btnPrint.IsEnabled = false;
            List<TPushPlan> printPlan = GetItemsSource(cboPrintArea.SelectedIndex, (short)cboPrintPeriod.SelectedIndex, isEditing);
            if (printPlan.Count >= 40)
            {
                MessageBox.Show("计划条目超过40！！");
                return;
            }
            try
            {
                WrtPlanToExcel(printPlan);
                if (!rbtnDirect.IsChecked.Value)
                {//预览打印
                    PrintNotDirect(printPlan[0]);
                }
                else
                {//直接打印
                    PrintDirect();
                }
            }
            catch (Exception err)
            {
                Logger.Log.LogErr.Info("打印错误：PlanEdit.cs类，btnPrint_Click事件；" + err.ToString());
                MessageBox.Show("请检查打印计划的表格是否已关闭！若没有关闭，请尝试关闭之后重新打印。");
                return;
            }
        }
        public void UpdateDgItemsSource()
        {
            if (tabPush.IsSelected)
            {
                displayPlan = isEditing ? GetItemsSource(0, 0) : GetItemsSource(cboPeriod.SelectedIndex, cboGroup.SelectedIndex);
                dgPlan.ItemsSource = null;
                dgPlan.ItemsSource = displayPlan;
            }
            if (tabStoking.IsSelected)
            {
                dgStoking.ItemsSource = null;
                dgStoking.ItemsSource = CokeRoom.StokingPlan;
            }
            if (tabEdit.IsSelected)
            {
                dgEdit.ItemsSource = null;
                dgEdit.ItemsSource = editingTPlan;
            }
        }
        /// <summary>
        /// 计划编辑前后，编辑相关控件的处理
        /// </summary>
        /// <param name="enabled">true为编辑计划时，false为取消编辑计划or保存计划</param>
        private void EnableControlDuringEdit(bool enabled)
        {
            BindingDataContext(enabled ? dgEdit : dgPlan);
            tabPush.IsSelected = !enabled;
            isEditing = enabled;
            btnSave.IsEnabled = enabled;
            btnSave.Foreground = enabled ? Brushes.Red : Brushes.Black;
            btnEdit.Content = enabled ? "取消编辑" : "开始编辑";
            btnEdit.Foreground = enabled ? Brushes.Red : Brushes.Black;
            tabEdit.IsEnabled = enabled;
            tabEdit.IsSelected = enabled;
            #region 开始编辑计划后，不允许更改时段
            cboPeriod.IsEnabled = !enabled;
            //cboGroup.IsEnabled = !isEnabled;
            cboArea.IsEnabled = !enabled;
            dpDate.IsEnabled = !enabled;
            #endregion
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (BanEdit())
            {
                MessageBox.Show("接班的第一分钟内禁止编辑计划！");
                return;
            }
            if (cboPeriod.SelectedIndex == 0)
            {
                MessageBox.Show("请正确设置“时段”！", "提示", MessageBoxButton.OK);
                return;
            }
            string strContent = Convert.ToString(btnEdit.Content);
            bool flag = strContent == "开始编辑" ? true : false;
            if (flag)
            {
                EnableControlDuringEdit(true);
                int index = cboGroup.SelectedIndex;
                // 开始编辑：判断时段和班组是否选择正确 ,如果班组选择错误，则下一条的赋值语句:cboGroup.SelectedIndex的改变会触发ComboBox的SelectionChanged事件
                cboGroup.SelectedIndex = Setting.NowPeriod == cboPeriod.SelectedIndex ? Setting.NowGroup : Setting.NextGroup;
                if (index == cboGroup.SelectedIndex)
                {//编辑计划时，如果班组已经选好，则不会触发 ComboBox的SelectionChanged事件 
                    #region 处于编辑状态时 dgPlan的ItemsSouce=PushPlan ，方便删除重复计划 20170921
                    displayPlan.Clear();
                    displayPlan.AddRange(CokeRoom.PushPlan);
                    dgPlan.ItemsSource = null;
                    dgPlan.ItemsSource = displayPlan;
                    #endregion 
                    #region  20170921 dgEdit的ItemsSouce  editingPlan=GetItemsSource(Period,Group)
                    editingTPlan = GetItemsSource(cboPeriod.SelectedIndex, cboGroup.SelectedIndex);
                    dgEdit.ItemsSource = null;
                    dgEdit.ItemsSource = editingTPlan;
                    #endregion
                }
                tabIndex = 2;//TabControl切换到计划编辑的TabItem
            }
            else
            {//取消编辑：生成的推焦计划和装煤计划重置为空；isEditing=false,btnSave.IsEnable=false;
                EnableControlDuringEdit(false);//恢复时段和班组的IsEnable属性，
                tabPush.IsSelected = true;
                repeatCount = 0;
                tabIndex = 0;
                editingTPlan.Clear();
                displayPlan = GetItemsSource(cboPeriod.SelectedIndex, cboGroup.SelectedIndex);
                dgPlan.ItemsSource = null;
                dgPlan.ItemsSource = displayPlan;
            }
        }
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            //如果不是编辑计划状态则return，不执行
            if (!isEditing)
            {//非编辑状态，返回
                return;
            }
            else
            {
                bool flag = editingTPlan.Count == 0 ? true : false;
                //1.由editingPlan.Count==0的值 来得到生成计划的 单挑Plan：如果==0，则根据输入的计划来Generate，否则根据editingPlan.Last()来Generate
                if (flag)
                {//根据输入的炉号，预定出焦时间得到当前的计划
                    #region 判断是否正确输入了：炉号，推焦时间
                    byte room = (byte)GetInputRoomNum(txtRoom.Text);
                    DateTime planTime = GetInputDateTime(txtPushTime.Text, dpDate.SelectedDate);
                    bool timeRight = IsTimeRight(cboPeriod.SelectedIndex, planTime);
                    if ((room <= 0 || room > 110) || !timeRight)
                    {
                        MessageBox.Show("炉号输入错误或推焦时间不在时段范围内！");
                        return;
                    }
                    #endregion
                    #region 生成并输出fstPlanInfo，确认后生成所有计划,使btnSave按钮可用
                    GeneratePlanBy(new TPushPlan(room, cboPeriod.SelectedIndex, cboGroup.SelectedIndex, planTime));
                    #endregion
                }
                else
                {//根据editingPlan的最后一项来生成计划
                    GeneratePlanBy(editingTPlan.Last());
                }
                dgEdit.ItemsSource = null;
                dgEdit.ItemsSource = editingTPlan;
                for (int i = editingTPlan.Count - 1; i >= 0; i--)
                {//目的：中班分解（16点之前或00点之前的第一计划）
                    int h = editingTPlan[i].PushTime.Hour;
                    if (h == 15 || h == 23)
                    {
                        dgEdit.SelectedIndex = i;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 非编辑计划下的删除：更新PushPlan，更新Config，不更新DB（只有在编辑计划时才会对DB有删除操作）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            List<IPlan> delPlan;
            IList iLst = (tabPush.IsSelected ? dgPlan : (tabStoking.IsSelected ? dgStoking : dgEdit)).SelectedItems;
            delPlan = IListToDelPlan(iLst);
            UpdatePlanByDelPlan(delPlan);
            //if (!tabEdit.IsSelected) InvalidByDelPlan(delPlan);
            if (!tabEdit.IsSelected) Dispatcher.BeginInvoke(new Action<List<IPlan>>(InvalidByDelPlan), delPlan);
            DataGrid dg = tabPush.IsSelected ? dgPlan : (tabStoking.IsSelected ? dgStoking : dgEdit);
            dg.ItemsSource = null;
            if (tabPush.IsSelected) displayPlan = isEditing ? GetItemsSource(0, 0) : GetItemsSource(cboPeriod.SelectedIndex, cboGroup.SelectedIndex);
            dg.ItemsSource = tabPush.IsSelected ? displayPlan : (tabStoking.IsSelected ? (IEnumerable)CokeRoom.StokingPlan : editingTPlan);
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                if (editingTPlan.Count == 0)
                {
                    EnableControlDuringEdit(false);
                    return;
                }
                #region 0701 重做计划保存逻辑;0715补充：计划重复时，不做保存
                //更新到CokeRoom.PushPlan 和.StokeingPlan,先判断是否有重复炉号，没有才可以保存计划
                int repeatRoomNum = 0;
                List<TPushPlan> anotherPeriod = GetItemsSource(editingTPlan[0].Period == 1 ? 2 : 1, 0);//得到另一时段的计划（从PushPlan中）
                #region 重复编辑当前班计划且下一班的计划已经排好（这种情况基本不会发生）
                //if (editingTPlan.Last().PushTime >= anotherPeriod.First().PushTime)
                //{//重复编辑当前班计划且下一班的计划已经排好（这种情况基本不会发生）
                //    MessageBoxResult btn = MessageBox.Show("继续保存当前计划将删除下一个" + anotherPeriod[0].StrGroup + "-" + anotherPeriod[0].StrPeriod + "的计划！是否继续保存", "提示", MessageBoxButton.YesNo);
                //    if (btn == MessageBoxResult.No) return;
                //    if (btn == MessageBoxResult.Yes)
                //    {//
                //        //①保存到config文件
                //        //②保存到DB
                //        //③更新PushPlan
                //    }
                //}
                #endregion
                bool repeat = false;
                if (anotherPeriod.Count > 0)
                {
                    repeat = CokeRoom.IsPlanRepetitive(anotherPeriod, editingTPlan, out repeatRoomNum);
                }
                if (repeat)
                {
                    string mesg = "炉号：" + repeatRoomNum + "  还未出焦，计划结焦时间计算错误，计划禁止保存！\n可以切换到“推焦计划”界面,删除重复的炉号后再执行保存。";
                    MessageBox.Show(mesg, "禁止保存");
                    return;
                }
                //20171124 使用异步委托BeginInvoke，目的是不影响UI的效果
                Dispatcher.BeginInvoke(new Action(Save), null);
                #endregion
            }
        }
        /// <summary>
        /// 修改推焦时间（结焦时间）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdatePushTime_Click(object sender, RoutedEventArgs e)
        {
            if (cboPeriod.SelectedIndex == 0)
            {
                MessageBox.Show("修改出焦时间必须指定时段：白班or夜班！");
                return;
            }
            if (!isEditing)
            {
                MessageBox.Show("计划已保存，不允许修改检修时间！如果需要修改，请先删除当前时段计划！");
                return;
            }
            Button btn = sender as Button;
            if (btn != null && isEditing)
            {
                int count = dgEdit.SelectedItems.Count;
                if (count > 1 || count == 0)
                {
                    MessageBox.Show("修改检修时间时，请选定起始待修改炉号所在行（单行），输入检修时间！");
                    return;
                }
                int min;
                int.TryParse(txtUpdateTime.Text, out min);//得到检修时间的值
                TPushPlan p = dgEdit.SelectedItem as TPushPlan;
                int index = editingTPlan.FindIndex(x => x.RoomNum == p.RoomNum);
                int timespan = index > 0 ? ((int)(editingTPlan[index].PushTime - editingTPlan[index - 1].PushTime).TotalMinutes) : 0;
                int addTime = ((string)btn.Tag) == "0" ? (min - timespan) : ((string)btn.Tag == "1" ? min : -min);
                int planCount = editingTPlan.Count;
                for (int i = index; i < planCount; i++)
                {
                    editingTPlan[i].PushTime = editingTPlan[i].PushTime.AddMinutes(addTime);
                    editingTPlan[i].BurnTime = editingTPlan[i].BurnTime + addTime;
                    if (!IsTimeRight(cboPeriod.SelectedIndex, editingTPlan[i].PushTime))
                    {//RemoveRange方法的Count参数：例  plan.count=9，maxIndex=8，i=5时，包括index=5的count共有9-1-5+1个；
                        editingTPlan.RemoveRange(i, planCount - (i + 1) + 1);
                        break;
                    }
                }
                int dgIndex = dgEdit.SelectedIndex;
                dgEdit.ItemsSource = null;
                dgEdit.ItemsSource = editingTPlan;
                dgEdit.SelectedIndex = dgIndex;//用于修改完检修时间后，使该条仍被选中
            }
        }
        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            #region  dpPlan中连续选择多条乱笺号的处理
            //if (!isEditing)
            //{
            //    MessageBox.Show("当前计划已保存，不允许修改！如有修改必要，请删除当前计划后重新编辑计划！");
            //    return;
            //}
            //if (dgPlan.SelectedItems.Count <= 0)
            //{
            //    MessageBox.Show("请选择乱笺炉号！");
            //    return;
            //}
            //int time;
            //try
            //{
            //    time = Convert.ToInt32(txtInsertPushTime.Text);
            //}
            //catch (Exception)
            //{

            //    MessageBox.Show("请检查输入的时间是否正确");
            //    return;
            //}
            ////准备乱笺的炉号和时间
            ////dgPlan.ItemsSource = null;
            //IList lst = dgPlan.SelectedItems;
            //List<TPushPlan> lstPlan = new List<TPushPlan>();
            //for (int i = 0; i < lst.Count; i++)
            //{
            //    TPushPlan a = lst[i] as TPushPlan;
            //    lstPlan.Add(a);
            //}
            //for (int i = 0; i < lstPlan.Count; i++)
            //{
            //    lstPlan[i].BurnTime = lstPlan[i].BurnTime + time;
            //    lstPlan[i].PushTime = lstPlan[i].PushTime.AddMinutes(time);
            //}
            //lstPlan.Sort(TPushPlan.CompareByTime);
            //int fstIndex = editingTPlan.FindIndex(x => x.RoomNum == lstPlan[0].RoomNum);
            //editingTPlan.RemoveRange(fstIndex, lstPlan.Count);
            ////editingTPlan  lstPlan   
            ////afterCount:乱笺号未后移之前，editingTPlan中 选择乱笺号后  后部分的计划条目数
            //int count = editingTPlan.Count;

            //editingTPlan[fstIndex].PushTime = lstPlan[0].PushTime.AddMinutes(-time);
            //editingTPlan[fstIndex].BurnTime = editingTPlan[fstIndex].GetBurnTime();
            //for (int index = fstIndex + 1; index < count; index++)
            //{
            //    TPushPlan p = editingTPlan[index];
            //    if (p.PushTime < lstPlan.Last().PushTime)
            //    {
            //        p.PushTime = editingTPlan[index - 1].PushTime.AddMinutes(9);
            //        p.BurnTime = p.GetBurnTime();
            //    }
            //    if (p.PushTime >= lstPlan.Last().PushTime)
            //    {
            //        p.PushTime = lstPlan.Last().PushTime.AddMinutes(9);
            //        p.BurnTime = p.GetBurnTime();
            //        if (index + 1 >= count) break;
            //        for (int j = index + 1; j < count; j++)
            //        {
            //            //if (j >= afterCount) break;
            //            TPushPlan p1 = editingTPlan[j - 1];
            //            editingTPlan[j].PushTime = p1.PushTime.AddMinutes(9);
            //            editingTPlan[j].BurnTime = editingTPlan[j].GetBurnTime();
            //            if (!IsTimeRight(cboPeriod.SelectedIndex, editingTPlan[j].PushTime))
            //            {
            //                editingTPlan.RemoveRange(j, count - j);
            //                break;
            //            }
            //        }
            //        break;
            //    }
            //}
            //editingTPlan.AddRange(lstPlan);
            //editingTPlan.Sort(TPushPlan.CompareByTime);
            //dgPlan.ItemsSource = null;
            //dgPlan.ItemsSource = editingTPlan;
            #endregion
            #region 2017-07-24修改乱笺号逻辑
            //if (!isEditing)
            //{
            //    MessageBox.Show("非计划编辑时，禁止修改乱笺号！");
            //    return;
            //}
            //TPushPlan p = new TPushPlan();
            //p.RoomNum = (byte)InsertRoomNum;
            //p.PushTime = (DateTime)InsertPushTime;
            //p.Group = editingTPlan[0].Group;
            //p.Period = editingTPlan[0].Period;
            //p.BurnTime = p.GetBurnTime();
            //int index = editingTPlan.FindIndex(x => x.RoomNum == InsertRoomNum);
            //if (index < 0)
            //{
            //    editingTPlan.Add(p);
            //}
            //else
            //{
            //    //p.PushTime>editingTPlan[index].PushTime
            //    List<TPushPlan> lstP1 = new List<TPushPlan>();
            //    lstP1.Add(editingTPlan[index]);
            //    editingTPlan.RemoveAt(index);
            //    if (p.PushTime > lstP1[0].PushTime)
            //    {
            //        for (int i = index; i < editingTPlan.Count; i++)
            //        {
            //            editingTPlan[i].PushTime = i == index ? lstP1.Last().PushTime : editingTPlan[i - 1].PushTime;
            //            if (editingTPlan[i].PushTime >= p.PushTime)
            //            {
            //                for (int j = i; j < editingTPlan.Count; j++)
            //                {
            //                    editingTPlan[j].PushTime = j == i ? p.PushTime.AddMinutes(9) : editingTPlan[j - 1].PushTime.AddMinutes(9);
            //                    if (!IsTimeRight(cboPeriod.SelectedIndex, editingTPlan[j].PushTime))
            //                    {
            //                        editingTPlan.RemoveRange(j, editingTPlan.Count - j);
            //                        break;
            //                    }
            //                    editingTPlan[j].BurnTime = editingTPlan[j].GetBurnTime();
            //                }
            //                break;
            //            }
            //            editingTPlan[i].BurnTime = editingTPlan[i].GetBurnTime();
            //        }
            //    }
            //    else
            //    {//p.PushTime<lstP1[0].PushTime时，index>0;
            //        for (int i = index - 1; i >= 0; i--)
            //        {
            //            editingTPlan[i].PushTime = i == (index - 1) ? lstP1[0].PushTime : editingTPlan[i + 1].PushTime.AddMinutes(-9);
            //            if (editingTPlan[i].PushTime <= p.PushTime)
            //            {
            //                for (int j = i; j >= 0; j--)
            //                {
            //                    editingTPlan[j].PushTime = j == i ? p.PushTime.AddMinutes(-9) : editingTPlan[j + 1].PushTime.AddMinutes(-9);
            //                    editingTPlan[j].BurnTime = editingTPlan[j].GetBurnTime();
            //                }
            //                break;
            //            }
            //            editingTPlan[i].BurnTime = editingTPlan[i].GetBurnTime();
            //        }
            //    }
            //    editingTPlan.Add(p);
            //    editingTPlan.Sort(TPushPlan.CompareByTime);
            //}
            //txtInsertRoom.Text = (cboArea.SelectedIndex == 0 ? GetNextRoomInfo12(InsertRoomNum) : GetNextRoomInfo34(InsertRoomNum)).ToString("000");
            //txtInsertPushTime.Text = InsertPushTime.Value.AddMinutes(9).ToString("t");
            //dgPlan.ItemsSource = null;
            //dgPlan.ItemsSource = editingTPlan;
            #endregion
            if (!isEditing)
            {
                MessageBox.Show("当前计划已保存，不允许修改！如有修改必要，请删除当前计划后重新编辑计划！");
                return;
            }
            //准备乱笺的炉号和时间
            int room = GetInputRoomNum(txtInsertRoom.Text);
            DateTime time = GetInputDateTime(txtInsertPushTime.Text, dpDate.SelectedDate);
            if (room == -1 || time.Year == 1988)
            {
                MessageBox.Show("乱笺号的出焦炉号或出焦时间输入有误，请检查后继续操作！");
                return;
            }
            TPushPlan p = new TPushPlan(room, cboPeriod.SelectedIndex, cboGroup.SelectedIndex, time);
            int index = editingTPlan.FindIndex(x => x.RoomNum == p.RoomNum);
            if (index >= 0)
            {
                editingTPlan.RemoveAt(index);
            }
            editingTPlan.Add(p);
            editingTPlan.Sort(TPushPlan.CompareByTime);
            int insertIndex = editingTPlan.FindIndex(x => x.RoomNum == p.RoomNum);//insertIndex的值>=0
            for (int i = editingTPlan.Count - 1; i >= 0; i--)
            {//删除超过时段的计划
                if (!IsTimeRight(cboPeriod.SelectedIndex, editingTPlan[i].PushTime))
                {
                    editingTPlan.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }
            txtInsertRoom.Text = GetNextRoomNum(room).ToString("000");
            txtInsertPushTime.Text = time.AddMinutes(BreakTime).ToString("t");
            dgEdit.ItemsSource = null;
            dgEdit.ItemsSource = editingTPlan;
        }
        private void dgPlan_GotMouseCapture(object sender, MouseEventArgs e)
        {
            txtUpdateTime.Focus();
            txtUpdateTime.SelectAll();
        }
        private void KillProcess(string processName)
        {
            //Process myproc = new Process();
            //得到所有打开的进程
            try
            {
                foreach (Process thisproc in Process.GetProcessesByName(processName))
                {
                    if (!thisproc.CloseMainWindow())
                    {
                        thisproc.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        /// <summary>
        /// 是否允许继续保存：当存在重复炉号时，多次点击保存，repeatCount多次自增(==点击保存的次数)
        /// 当重复的炉号在PushPlan中被更新掉之后，repeat=false；重新计算结焦时间，重置repeatCount次数=0；
        /// </summary>
        /// <returns>true时，继续执行对PushPlan，Config，PlanDB保存，false时：用来检查计划结焦时间，下次点击btnSave时，直接保存</returns>
        private bool UpdateBurnTime()
        {
            bool continueSave = true;
            if (repeatCount > 0)
            {
                repeatCount = 0;
                for (int i = 0; i < editingTPlan.Count; i++)
                {
                    editingTPlan[i].BurnTime = (int)(editingTPlan[i].PushTime - editingTPlan[i].StokingTime).TotalMinutes;
                }
                continueSave = MessageBox.Show("部分炉号的计划结焦时间已改变！继续保存请点击“好”，否则点击“取消”", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.OK ? true : false;
            }
            return continueSave;
        }
        private void RecToPlanDb()
        {
            List<TPushPlan> tempEdit = new List<TPushPlan>();
            List<TPushPlan> tempDisplay = new List<TPushPlan>();
            tempEdit.AddRange(editingTPlan);
            tempDisplay.AddRange(displayPlan);
            EditPlanHelper dbHelper = new EditPlanHelper(tempEdit);
            dbHelper.RecToPlanDB();

        }
        /// <summary>
        /// 打印计划的时段选择随编辑计划的时段选择变化
        /// </summary>
        /// <param name="index"></param>
        private void GetSelectedIndex(int index)
        {
            if (index > 0)
            {
                cboPrintPeriod.SelectedIndex = index - 1;
            }
        }
        /// <summary>
        /// 接班的第一分钟 禁止编辑计划:因为自动更改班组的计时器 在第一分钟内不一定能执行
        /// </summary>
        /// <returns></returns>
        private bool BanEdit()
        {
            int h = DateTime.Now.Hour;
            int min = DateTime.Now.Minute;
            return (h == 8 || h == 20) && min == 1 ? true : false;
        }

        private void Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rbtn = sender as RadioButton;
            if (rbtn != null)
            {
                if (rbtn.Name == "rbtnDirect")
                {
                    rbtnPreview.IsChecked = false;
                }
                else
                {
                    rbtnDirect.IsChecked = false;
                }
            }
        }
        /// <summary>
        /// 非编辑计划时的删除：
        /// ①更新config中的计划；
        /// ②更新到DB--20170921 未做
        /// ③更新到PushPlan和StokingPlan
        /// </summary>
        /// <param name="lst"></param>
        private void DelPlan(IList lst, bool tPlan)
        {
            string path = @"Config\RoomPlanInfo.config";
            OperateConfig config = new OperateConfig(path);
            //List<TPushPlan> tPlan = cboArea.SelectedIndex == 0 ? CokeRoom.PushPlan12 : CokeRoom.PushPlan34;
            //List<MStokingPlan> mPlan = cboArea.SelectedIndex == 0 ? CokeRoom.StokingPlan12 : CokeRoom.StokingPlan34;
            List<IPlan> delPlan = new List<IPlan>();
            for (int i = 0; i < lst.Count; i++)
            {
                IPlan p = lst[i] as IPlan;
                if (p == null) break;
                delPlan.Add(p);
                config.SetPlanAttributeValue(p.RoomNum, "Valid", "0");//更新到Config文件

            }
            config.XmlDoc.Save(path);
            //更新CokeRoom.PushPlan
            for (int i = 0; i < delPlan.Count; i++)
            {
                CokeRoom.StokingPlan.RemoveAll(m => m.RoomNum == delPlan[i].RoomNum);
                if (!tPlan) continue;
                CokeRoom.PushPlan.RemoveAll(p => p.RoomNum == delPlan[i].RoomNum);
            }
        }
        private List<IPlan> IListToDelPlan(IList lst)
        {
            List<IPlan> delPlan = new List<IPlan>();
            for (int i = 0; i < lst.Count; i++)
            {
                IPlan p = lst[i] as IPlan;
                if (p == null) break;
                delPlan.Add(p);
            }
            return delPlan;
        }
        private void InvalidByDelPlan(List<IPlan> plan)
        {
            string path = @"Config\RoomPlanInfo.config";
            OperateConfig config = new OperateConfig(path);
            for (int i = 0; i < plan.Count; i++)
            {
                config.SetPlanAttributeValue(plan[i].RoomNum, "Valid", "0");//更新到Config文件
            }
            config.XmlDoc.Save(path);
        }
        private void UpdatePlanByDelPlan(List<IPlan> delPlan)
        {
            for (int i = 0; i < delPlan.Count; i++)
            {
                if (tabStoking.IsSelected)
                {
                    CokeRoom.StokingPlan.RemoveAll(x => x.RoomNum == delPlan[i].RoomNum);
                }
                else if (tabPush.IsSelected)
                {
                    CokeRoom.PushPlan.RemoveAll(x => x.RoomNum == delPlan[i].RoomNum);
                    CokeRoom.StokingPlan.RemoveAll(x => x.RoomNum == delPlan[i].RoomNum);
                }
                else
                {
                    editingTPlan.RemoveAll(x => x.RoomNum == delPlan[i].RoomNum);
                }
            }
        }
        private void tabItemIsSelected(object sender, MouseButtonEventArgs e)
        {
            TabItem item = sender as TabItem;
            if (item == null) return;
            #region 防止 同一界面重复点击
            int tag = 0;
            tag = Convert.ToInt32(item.Tag);
            if (tag == tabIndex)
            {
                return;
            }
            else
            {
                tabIndex = tag;
            }
            #endregion
            if (tabPush != null && tabPush.IsSelected)
            {
                if (!isEditing)
                {
                    AssignDataGridItemsSource(cboPeriod.SelectedIndex, cboGroup.SelectedIndex);
                }
                else
                {
                    AssignDataGridItemsSource(0, 0);
                }
            }
            if (tabStoking != null && tabStoking.IsSelected)
            {
                dgStoking.ItemsSource = null;
                dgStoking.ItemsSource = CokeRoom.StokingPlan;
            }
        }
        /// <summary>
        /// 给GroupBox的DataContext赋值  grpRoomInfo: txtRoomNum,txtPushTime,txtBurnTime
        /// </summary>
        /// <param name="dg"> dgPlan||dgEdit</param>
        private void BindingDataContext(DataGrid dg)
        {
            Binding binding = new Binding();
            binding.Source = dg;
            binding.Path = new PropertyPath("SelectedItem");
            BindingOperations.ClearBinding(grpRoomInfo, DataContextProperty);
            grpRoomInfo.SetBinding(DataContextProperty, binding);
        }
        #region 计划打印
        /// <summary>
        /// 后台打印计划
        /// </summary>
        private void PrintDirect()
        {
            string path = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.Length - 9) + "PushPlan.xlsx";
            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook book = app.Workbooks.Open(path, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            book.PrintOutEx();
            book.Close(false);
            app.Quit();
            KillProcess("PushPlan.xlsx");
        }
        /// <summary>
        /// 打开Excel表格，预览后打印
        /// </summary>
        /// <param name="plan"></param>
        private void PrintNotDirect(TPushPlan plan)
        {
            string filePath = "..\\..\\PushPlan.xlsx";
            string mesg = "即将打印的计划为：" + plan.StrPeriod + "，" + plan.StrGroup + "，" + cboPrintArea.SelectionBoxItem.ToString() + "；\n稍后3s中打开Excel表格，预览后手动打印！";
            if (MessageBoxResult.Yes == MessageBox.Show(mesg, "提示", MessageBoxButton.YesNo))
            {
                System.Diagnostics.Process.Start(filePath);//打开EXCEL表格
            }
        }
        /// <summary>
        /// 由模板PlanModel.xlsx把printPlan写到PushPlan.xlsx
        /// </summary>
        /// <param name="printPlan"></param>
        private void WrtPlanToExcel(List<TPushPlan> printPlan)
        {
            string modelPath = "..\\..\\PlanModel.xlsx";
            string filePath = "..\\..\\PushPlan.xlsx";
            using (FileStream modelFile = new FileStream(modelPath, FileMode.Open, FileAccess.ReadWrite))
            {
                XSSFWorkbook workbook = new XSSFWorkbook(modelFile);
                int area = cboPrintArea.SelectedIndex;
                PlanPrintHelper excel = new PlanPrintHelper(workbook, printPlan, area);
                excel.GetToPrintedExcel();
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                using (FileStream file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    excel.workbook.Write(file);
                }
            }
        }
        #endregion
        private void Save()
        {
            EnableControlDuringEdit(false);
            CokeRoom.SaveToPlan(editingTPlan);
            //更新到Config文件
            string path = @"Config\RoomPlanInfo.config";
            OperateConfig config1 = new OperateConfig(path, CokeRoom.PushPlan);
            config1.RecToConfig();
            //更新到数据库;
            RecToPlanDb();
            //给dgPlan.ItemsSource赋值 重置displayPlan 和 editingTPlan;
            AssignDataGridItemsSource(cboPeriod.SelectedIndex, cboGroup.SelectedIndex);
            editingTPlan.Clear();
        }
        private void tabItemMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            #region 使用样式(Style)来设置 单击事件(鼠标左键按下/松开) 20171128 勿删
            //使用以下策略 可获得 使用当前样式的TabItem
            Border item = sender as Border;
            int tag = Convert.ToInt32(((TabItem)item.TemplatedParent).Tag);
            #endregion

            if (tabPush != null && tag == 0)
            {
                if (!isEditing)
                {
                    Dispatcher.BeginInvoke(new Action<int, int>(AssignDataGridItemsSource), cboPeriod.SelectedIndex, cboGroup.SelectedIndex);
                }
                else
                {
                    AssignDataGridItemsSource(0, 0);
                }
            }
            if (tabStoking != null && tag == 1)
            {
                dgStoking.ItemsSource = null;
                dgStoking.ItemsSource = CokeRoom.StokingPlan;
            }
        }
    }
}