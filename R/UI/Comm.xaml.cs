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
using WGPM.R.Parms;
using System.Windows.Threading;
using WGPM.R.CommHelper;
using System.Threading;
using WGPM.R.OPCCommunication;
using System.Globalization;
using WGPM.R.UI.UIConverter;
using WGPM.R.UI.VehicalsData;

namespace WGPM.R.UI
{
    /// <summary>
    /// Comm.xaml 的交互逻辑
    /// </summary>
    public partial class Comm : UserControl
    {
        public Comm()
        {
            InitializeComponent();
            Loaded += Comm_Loaded;
        }
        DispatcherTimer pingTimer = new DispatcherTimer();
        CommHelper helper = new CommHelper();
        public CommExamine commHelper;
        string[] wirelessIp =
        {
            "192.168.0."+(Setting.AreaFlag?101:201),
            "192.168.0."+(Setting.AreaFlag?102:202),
            "192.168.0."+(Setting.AreaFlag?103:203),
            "192.168.0."+(Setting.AreaFlag?104:204)
        };
        Thread trd;
        private void Comm_Loaded(object sender, RoutedEventArgs e)
        {
            _ColorBinding();
            //暂时在界面中给IpArr赋值(之后需要在参数配置界面给各车网络设备的IP赋值)
            AssignCarDeviceIpArr();
            for (int i = 0; i < 8; i++)
            {
                AddCarComm(i);
            }
            pingTimer.Start();
            pingTimer.Tick += PingTimer_Tick;
            pingTimer.Interval = TimeSpan.FromSeconds(5);
            commHelper = new CommExamine(wirelessIp);
            Communication.CommLst.Add(commHelper);
            trd = new Thread(commHelper.GetCommStatus);
            trd.Start();
        }

        private void PingTimer_Tick(object sender, EventArgs e)
        {
            GetConnectionStatus();
            if (trd.IsAlive) return;
            trd = new Thread(commHelper.GetCommStatus)
            {
                Priority = ThreadPriority.Lowest,
                IsBackground = true
            };
            trd.Start();
            AssignCommStatus();
        }

        List<string[]> IpLst = new List<string[]>();
        private void AddCarComm(int carIndex)
        {
            CarComm carComm = new CarComm { DeviceIP = IpLst[carIndex] };
            Communication.CommLst.Add(carComm.CommHelper);//统计各车的通讯数据
            int carType = 0;
            carComm.txtCarInfo.Text = carIndex % 2 + (Setting.AreaFlag ? 1 : 3) + " # " + GetCarNameAndType(carIndex, out carType);
            Canvas.SetTop(carComm, 24 + carType * 180);
            Canvas.SetLeft(carComm, 255 + carIndex % 2 * 180);
            mainCanvas.Children.Add(carComm);//Children在未添加carComm时的Count为14，maxIndex=13；20170923
        }
        /// <summary>
        /// 根据所有车的索引号来得到车的类型和名称
        /// </summary>
        /// <param name="carIndex">存放所有车的List的索引值</param>
        /// <param name="carType">T0,L1,X2,M3</param>
        /// <returns>车的名称：推焦车，拦焦车，熄焦车，装煤车</returns>
        private string GetCarNameAndType(int carIndex, out int carType)
        {
            string carName = null;
            if (carIndex <= 1)
            {
                carName = "推焦车";
                carType = 0;
            }
            else if (carIndex > 1 && carIndex <= 3)
            {
                carName = "拦焦车";
                carType = 1;
            }
            else if (carIndex > 3 && carIndex <= 5)
            {
                carName = "熄焦车";
                carType = 2;
            }
            else
            {
                carName = "装煤车";
                carType = 3;
            }
            return carName;
        }
        /// <summary>
        /// 给四个地面无线模块的图例(iron)添加颜色
        /// </summary>
        private void GetConnectionStatus()
        {
            if (commHelper.SucceedCount[0] < 3)
            {
                helper.TWireless = commHelper.CommStatus[0];
            }
            if (commHelper.SucceedCount[1] < 3)
            {
                helper.XWireless = commHelper.CommStatus[1];
            }
            if (commHelper.SucceedCount[2] < 3)
            {
                helper.LWireless = commHelper.CommStatus[2];
            }
            if (commHelper.SucceedCount[3] < 3)
            {
                helper.MWireless = commHelper.CommStatus[3];
            }
        }

        private void WirelessImg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            if (img != null)
            {
                commHelper.ExcutePing((string)img.Tag);
            }
        }

        private void ImageComputer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CarsData c = new CarsData();
            c.Show();
        }
        private void AssignCarDeviceIpArr()
        {
            //string ip="192.168.0."++"";
            for (int carIndex = 0; carIndex < 8; carIndex++)
            {
                string[] ipArr = new string[4];
                for (int deviceID = 0; deviceID < 4; deviceID++)
                {
                    ipArr[deviceID] = "192.168.0." + ((Setting.AreaFlag ? 0 : 100) + (carIndex + 1) * 10 + (deviceID + 1));
                }
                IpLst.Add(ipArr);
            }
        }
        /// <summary>
        /// 得到四个地面站无线网桥和8辆车的网络设备的通讯状态
        /// 需要注意的是：mainCanvas.Children在未添加CarComm时的Count=14，最大索引maxIndex=13；方法中出现的数字13即为此含义
        /// 数字8代表8辆车
        /// </summary>
        private void AssignCommStatus()
        {
            if (Communication.CommStatus.Count == 0)
            {
                for (int i = 0; i < 9; i++)
                {
                    Communication.CommStatus.Add(new CommStatus(new bool[] { false, false, false, false }));
                }
            }

            if (commHelper.SucceedCount[2] < 3)
            {//索引为2时对应的为无线网桥的状态计数
                Communication.CommStatus[0] = new CommStatus(commHelper.CommStatus);
            }
            for (int i = 0; i < 8; i++)
            {//添加8辆车的通讯状态
                CarComm carComm = (CarComm)mainCanvas.Children[14 + i];
                CommStatus comm = new CommStatus(carComm.CommStatus);
                Communication.CommStatus[i + 1] = comm;
            }
        }
        private void _ColorBinding()
        {
            ColorPerIron(tWireless, "TWireless");
            ColorPerIron(lWireless, "LWireless");
            ColorPerIron(xWireless, "XWireless");
            ColorPerIron(mWireless, "MWireless");
        }
        private void ColorPerIron(Ellipse ep, string path)
        {
            Binding myBinding = new Binding(path);
            myBinding.Source = helper;
            BoolToColorConverter converter = new BoolToColorConverter();
            converter.DefaultColor = ep.Stroke;
            converter.TrueToColor = Brushes.Lime;
            myBinding.Converter = converter;
            ep.SetBinding(Shape.StrokeProperty, myBinding);
        }
    }
    /// <summary>
    /// 地面站无线模块的Connection状态
    /// </summary>
    class CommHelper : DependencyObject
    {
        public bool TWireless
        {
            get { return (bool)GetValue(TWirelessProperty); }
            set { SetValue(TWirelessProperty, value); }
        }
        // Using a DependencyProperty as the backing store for TWireless.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TWirelessProperty =
            DependencyProperty.Register("TWireless", typeof(bool), typeof(CommHelper), new PropertyMetadata(false));
        public bool LWireless
        {
            get { return (bool)GetValue(LWirelessProperty); }
            set { SetValue(LWirelessProperty, value); }
        }
        // Using a DependencyProperty as the backing store for LWireLess.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LWirelessProperty =
            DependencyProperty.Register("LWireless", typeof(bool), typeof(CommHelper), new PropertyMetadata(false));
        public bool XWireless
        {
            get { return (bool)GetValue(XWirelessProperty); }
            set { SetValue(XWirelessProperty, value); }
        }
        // Using a DependencyProperty as the backing store for XWireless.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XWirelessProperty =
            DependencyProperty.Register("XWireless", typeof(bool), typeof(CommHelper), new PropertyMetadata(false));
        public bool MWireless
        {
            get { return (bool)GetValue(MWirelessProperty); }
            set { SetValue(MWirelessProperty, value); }
        }
        // Using a DependencyProperty as the backing store for MWireless.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MWirelessProperty =
            DependencyProperty.Register("MWireless", typeof(bool), typeof(CommHelper), new PropertyMetadata(false));


    }
}
