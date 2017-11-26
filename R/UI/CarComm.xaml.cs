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
using WGPM.R.CommHelper;
using System.Windows.Threading;
using System.Threading;
using WGPM.R.UI.UIConverter;

namespace WGPM.R.UI
{
    /// <summary>
    /// CarComm.xaml 的交互逻辑
    /// </summary>
    public partial class CarComm : UserControl
    {
        public CarComm()
        {
            InitializeComponent();
            Loaded += CarComm_Loaded;
        }
        CarCommHelper helper = new CarCommHelper();
        private void CarComm_Loaded(object sender, RoutedEventArgs e)
        {
            CommHelper = new CommExamine(DeviceIP);
            trd = new Thread(CommHelper.GetCommStatus);
            trd.Start();
            pingTimer.Start();
            pingTimer.Tick += PingTimer_Tick;
            pingTimer.Interval = TimeSpan.FromSeconds(5);
            _ColorBinding();
        }

        //设备的顺序：PLC，触摸屏，无线模块，解码器
        private void PingTimer_Tick(object sender, EventArgs e)
        {
            ColorIron();
            if (trd.IsAlive) return;
            trd = new Thread(CommHelper.GetCommStatus)
            {
                Priority = ThreadPriority.Lowest,
                IsBackground = true
            };
            trd.Start();
        }
        DispatcherTimer pingTimer = new DispatcherTimer();
        public string[] DeviceIP { get; set; }//每个string数组包含车上四个的IP：①PLC，②触摸屏Touch，③无线模块Wireless，④解码器Decode
        public CommExamine CommHelper;
        public bool[] CommStatus = new bool[4];
        Thread trd;

        private void IMG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            if (img != null)
            {
                CommHelper.ExcutePing((string)img.Tag);
            }
        }
        /// <summary>
        /// CommExamine类中的SucceedCount计数=3时，各种颜色图标已经稳定，不需要在重新赋值
        /// 只有当SucceedCount计数小于3时（意味着断线重连？15s后计数恢复至3后）
        /// </summary>
        private void ColorIron()
        {
            if (CommHelper.SucceedCount[0] < 3)
            {//PLC
                helper.PLC = CommHelper.CommStatus[0];
            }
            if (CommHelper.SucceedCount[1] < 3)
            {//Touch
                helper.Touch = CommHelper.CommStatus[1];
            }
            if (CommHelper.SucceedCount[2] < 3)
            {//Wireless
                CommStatus = CommHelper.CommStatus;
                helper.Wireless = CommHelper.CommStatus[2];
            }
            if (CommHelper.SucceedCount[3] < 3)
            {//Decode
                helper.Decode = CommHelper.CommStatus[3];
            }
        }
        private void _ColorBinding()
        {
            ColorPerIron(touch, "Touch");
            ColorPerIron(plc, "PLC");
            ColorPerIron(decode, "Decode");
            ColorPerIron(wireless, "Wireless");
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
    /// 
    /// </summary>
     class CarCommHelper : DependencyObject
    {
        public bool Wireless
        {
            get { return (bool)GetValue(WirelessProperty); }
            set { SetValue(WirelessProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Wireless.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WirelessProperty =
            DependencyProperty.Register("Wireless", typeof(bool), typeof(CarCommHelper), new PropertyMetadata(false));
        public bool Decode
        {
            get { return (bool)GetValue(DecodeProperty); }
            set { SetValue(DecodeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Decode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DecodeProperty =
            DependencyProperty.Register("Decode", typeof(bool), typeof(CarCommHelper), new PropertyMetadata(false));
        public bool Touch
        {
            get { return (bool)GetValue(TouchProperty); }
            set { SetValue(TouchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Touch.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TouchProperty =
            DependencyProperty.Register("Touch", typeof(bool), typeof(CarCommHelper), new PropertyMetadata(false));
        public bool PLC
        {
            get { return (bool)GetValue(PLCProperty); }
            set { SetValue(PLCProperty, value); }
        }
        // Using a DependencyProperty as the backing store for PLC.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PLCProperty =
            DependencyProperty.Register("PLC", typeof(bool), typeof(CarCommHelper), new PropertyMetadata(false));
    }
}
