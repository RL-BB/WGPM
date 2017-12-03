using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using WGPM.R.OPCCommunication;
using WGPM.R.UI;
using WGPM.R.RoomInfo;
using WGPM.R.Sys.SysParms;
using WGPM.R.Vehicles;
using WGPM.R.LinqHelper;
using System.Threading;
using WGPM.Properties;

namespace WGPM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer uiTimer = new DispatcherTimer();
        private Addrs Addrs = new Addrs();
        private Communication comm = new Communication();
        private readonly Windows[] ui = new Windows[8];
        private PlanEdit planEdit;
        private int UITag { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            //获取当前进程  
            Process currentProcess = Process.GetCurrentProcess();
            //获取当前运行程序完全限定名   
            string currentFileName = currentProcess.MainModule.FileName;
            //获取进程名为ProcessName的Process数组。 
            Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
            //遍历有相同进程名称正在运行的进程 
            // ReSharper disable once UnusedVariable
            foreach (Process p in processes.Where(process => process.MainModule.FileName == currentFileName).Where(process => process.Id != currentProcess.Id))
            {
                MessageBox.Show("程序已打开！");
                Environment.Exit(0);
            }
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Settings conn = new Settings();
            Setting.ConnectionStr = Setting.AreaFlag ? conn.WGPM_CokeArea12 : conn.WGPM_CokeArea34;
            RecActivateInfo();//登陆时间作记录
            LoadWindows();
            //从数据库读取计划
            //由当前时间得到时段，间接得到下个时段（“白班”或“夜班”）

            //开启窗口切换计时器
            uiTimer.Tick += MainTimer_Tick;
            uiTimer.Interval = TimeSpan.FromMilliseconds(20);
            uiTimer.Start();
        }
        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (UITag == SysParms.UITag)
            {//单击的为当前窗口
                return;
            }
            if (SysParms.UITag != 7 && Setting.ScheduleFlag)
            {
                MessageBox.Show("请确定“时段班组”组中的“时段”和“班组”已正确设置后，点击“保存”按钮！");
                SysParms.UITag = 7;
                UITag = 7;
                return;
            }
            if (SysParms.UITag == 8)
            {//退出时 如推焦车推焦杆和平煤杆正在动，即处于生产状态时，禁止；
                //bool yes = false;
                for (int i = 0; i < 2; i++)
                {
                    if (((TjcDataRead)Communication.CarsLst[i].DataRead).PushPoleLength == 0 && ((TjcDataRead)Communication.CarsLst[i].DataRead).PingPoleLength == 0)
                    {
                        ActivateInfo act = new ActivateInfo(DateTime.Now, false);
                        act.RecToDB();

                        Environment.Exit(0);
                    }
                    else
                    {
                        SysParms.UITag = UITag;
                        string mesg = (i + 1) + "#推焦车正在推焦或者平煤，请等待推焦和平煤结束之后再退出！";
                        MessageBox.Show(mesg, "提示");
                        return;
                    }
                }
            }
            // 0630 新增结焦状态 0912增加系统设置
            if (SysParms.UITag != UITag)
            {
                if (SysParms.UITag == 1)
                {
                    planEdit.UpdateDgItemsSource();
                }
                ui[SysParms.UITag].Show();
                ui[UITag].Hide();
                UITag = SysParms.UITag;
            }
        }
        private void LoadWindows()
        {
            UITag = 7;
            double screenHeight = 768;
            //只设置了主界面
            for (int i = 0; i < 8; i++)
            {
                ui[i] = new Windows();
                switch (i)
                {
                    case 0://主界面
                        ui[i].MainGrid.Children.Add(new MainUI { Margin = new Thickness(0, 30, 0, 0) });
                        ui[i].MainGrid.Children.Add(new MainTogether { Margin = new Thickness(0, screenHeight - 165, 0, 0) });
                        break;
                    case 1://计划编辑
                        ui[i].MainGrid.Children.Add(planEdit = new PlanEdit { Margin = new Thickness(0, 30, 0, 0) });
                        break;
                    case 2://通讯状态
                        ui[i].MainGrid.Children.Add(new Comm { Margin = new Thickness(0, 0, 0, 0) });
                        break;
                    case 3://电流曲线
                        for (int index = 0; index < 2; index++)
                        {
                            ui[i].MainGrid.Children.Add(new CurPlotter
                            {
                                Margin = new Thickness(10, 60 + 320 * index, 10, 10),
                                Height = 300,
                                VerticalAlignment = VerticalAlignment.Top,
                                TCarIndex = index,
                                NextPush = true,
                                NextPing = true
                            });
                        }
                        break;
                    case 4://结焦状态
                        ui[i].MainGrid.Children.Add(new BurnStatus { Margin = new Thickness(0, 50, 0, 0), Area = 1 });
                        ui[i].MainGrid.Children.Add(new BurnStatus { Margin = new Thickness(0, 320, 0, 0), Height = 260, Area = 2 });
                        break;
                    case 5://历史记录
                        ui[i].MainGrid.Children.Add(new QueryRec { Margin = new Thickness(0, 30, 0, 0) });
                        break;
                    case 6://用户信息
                        ui[i].MainGrid.Children.Add(new ContactInfo { Margin = new Thickness(0, 30, 0, 0) });
                        break;
                    case 7://系统设置
                        ui[i].MainGrid.Children.Add(new Setting { Margin = new Thickness(0, 0, 0, 0) });
                        break;
                    default:
                        break;
                }
                ui[i].MainGrid.Children.Add(new Title { Margin = new Thickness(0, 0, 0, 0) });
                ui[i].MainGrid.Children.Add(new R.UI.Menu());
            }
            this.Hide();
            ui[7].Show();
        }
        private void RecActivateInfo()
        {
            ActivateInfo act = new ActivateInfo(DateTime.Now, true);
            act.RecToDB();
        }
    }
}
