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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WGPM.R.OPCCommunication;
using WGPM.R.RoomInfo;
using WGPM.R.Vehicles;
using WGPM.R.UI.UIVehicals;
using WGPM.R.UI.UIVehicals.Xjc;
using WGPM.R.UI.UIConverter;
//熄焦车焦罐编号从干熄往水熄方向，依次为1,2,3,4
namespace WGPM.R.UI
{
    /// <summary>
    /// MainUI.xaml 的交互逻辑
    /// </summary>
    public partial class MainUI : UserControl
    {
        //动画计时器
        private readonly DispatcherTimer burnStatus = new DispatcherTimer();
        public TextBox[] mt = new TextBox[3];
        public TextBox[] rooms = new TextBox[110];
        public Rectangle[] pathwayBottom = new Rectangle[3];
        public Rectangle[] pathwayTop = new Rectangle[3];
        #region UIXjc
        UserControl xjc1;
        UserControl xjc2;
        #endregion

        public MainUI()
        {
            InitializeComponent();
            Loaded += MainUI_Loaded;
            SetZIndex();
            _Binding();
        }

        private void MainUI_Loaded(object sender, RoutedEventArgs e)
        {
            CreateMainUI();
            //CarMove();
            BurnCoke();
        }
        public void CreateMainUI()
        {
            CreateRooms();
            CreatePathway();
            CreateMT();
        }
        //以下所有的CreateXXX()方法仅仅是创建模型，初始化后也不显示在界面
        /// <summary>
        /// 煤塔设置有瑕疵0321
        /// </summary>
        public void CreateMT()
        {
            //焦炉模型：端台--1#炉区(55孔)--煤塔--2#炉区(55孔)--端台
            //借鉴江西丰城的思路，界面显示完整的1#、2#炉区概貌，宽度为1024，每个room的width为7，煤塔宽度为12*7？
            //即12*7*3+110*7=252+770=1022<1024,2个像素的余量
            for (int i = 0; i < mt.Length; i++)
            {
                mt[i] = new TextBox
                {
                    Name = "mt" + (i + 1).ToString("00"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 7 * 12,
                    Height = 110,
                    Background = Brushes.Green,
                    Foreground = Brushes.White,
                    Cursor = Cursors.Hand,
                    BorderBrush = Brushes.White,
                    IsReadOnly = true,
                    IsTabStop = false
                };
                string mtTooltip = (i == 1) ? "煤塔" : (i == 0 ? "1#端台" : "2#端台");
                ToolTipService.SetToolTip(mt[i], mtTooltip);
                Canvas.SetTop(mt[i], 200);
                double leftValue = 0;
                if (i == 0)
                {
                    leftValue = 1;
                }
                else
                {
                    leftValue = i == 1 ? (Canvas.GetLeft(rooms[54]) + rooms[54].Width) : (Canvas.GetLeft(rooms[109]) + rooms[109].Width);
                }
                Canvas.SetLeft(mt[i], leftValue);
                mainCanvas.Children.Add(mt[i]);
            }
        }
        public void CreateRooms()
        {
            for (int i = 0; i < 110; i++)
            {
                rooms[i] = new TextBox
                {
                    Name = "room" + i.ToString("000"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 7,//7个像素
                    Height = 110,
                    Background = Brushes.Black,
                    Foreground = Brushes.White,
                    Cursor = Cursors.Hand,
                    BorderBrush = Brushes.White,
                    IsReadOnly = true,
                    IsTabStop = false
                };
                string area = i < 55 ? "1# " : "2# ";
                ToolTipService.SetToolTip(rooms[i], area + (i + 1));
                Canvas.SetTop(rooms[i], 200);
                int width = i < 55 ? 0 : 7 * 12;
                //1#DT+55孔+MT+55孔+2#DT（端台）
                Canvas.SetLeft(rooms[i], 7 * 12 + i * 7 + width);
                mainCanvas.Children.Add(rooms[i]);
                //鼠标Enter和Leave的两个事件待补
            }
        }
        public void CreatePathway()
        {
            for (int i = 0; i < pathwayTop.Length; i++)
            {
                pathwayTop[i] = new Rectangle
                {
                    Fill = Brushes.Black,
                    Stroke = Brushes.Black,
                    Width = 2048,
                    Height = 2
                };
                pathwayBottom[i] = new Rectangle
                {
                    Fill = Brushes.Black,
                    Stroke = Brushes.Black,
                    Width = 2048,
                    Height = 2
                };
                Canvas.SetLeft(pathwayTop[i], -500);
                Canvas.SetLeft(pathwayBottom[i], -500);
            }
            Canvas.SetTop(pathwayTop[0], 333);
            Canvas.SetTop(pathwayBottom[0], 400);
            mainCanvas.Children.Add(pathwayTop[0]);
            mainCanvas.Children.Add(pathwayBottom[0]);
            Canvas.SetTop(pathwayTop[1], 173);
            Canvas.SetTop(pathwayBottom[1], 187);
            mainCanvas.Children.Add(pathwayTop[1]);
            mainCanvas.Children.Add(pathwayBottom[1]);
            Canvas.SetTop(pathwayTop[2], 102);
            Canvas.SetTop(pathwayBottom[2], 129);
            mainCanvas.Children.Add(pathwayTop[2]);
            mainCanvas.Children.Add(pathwayBottom[2]);
        }
        #region 动画计时器设置
        public void BurnCoke()
        {
            burnStatus.Start();
            burnStatus.Tick += BurnStatus_Tick;
            burnStatus.Interval = TimeSpan.FromSeconds(60);
        }
        private void BurnStatus_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 110; i++)
            {
                rooms[i].Background = CokeRoom.GetRoomsColor(CokeRoom.BurnStatus[i + 1].BurnStatus);
            }
        }
        #endregion
        #region TjcBinding
        private void TjcBinding()
        {
            //炉号
            RoomNumBinding((Vehicles.Tjc)Communication.CarsLst[0], tjc1);
            RoomNumBinding((Vehicles.Tjc)Communication.CarsLst[1], tjc2);
            //车辆的(左右)移动
            //35 推焦杆向左偏离1#炭化室一个单位距离(7)；938：推焦杆向右偏离110#炭化室一个单位距离(7)
            TjcMoveBinding((Vehicles.Tjc)Communication.CarsLst[0], tjc1, 35, 320);
            TjcMoveBinding((Vehicles.Tjc)Communication.CarsLst[1], tjc2, 896, 320);
            //推杆、平杆上下方向移动
            PoleMoveBinding(Communication.CarsLst[0], true, tjc1);
            PoleMoveBinding(Communication.CarsLst[0], false, tjc1);
            PoleMoveBinding(Communication.CarsLst[1], true, tjc2);
            PoleMoveBinding(Communication.CarsLst[1], false, tjc2);
        }
        private void PoleMoveBinding(Vehicle T, bool isPushPole, UIVehicals.Tjc tjc)
        {
            Binding myBinding = new Binding();
            myBinding.Source = T.DataRead;
            myBinding.Path = new PropertyPath(isPushPole ? "PushPoleLength" : "PingPoleLength");
            PoleMoveConverter move = new PoleMoveConverter();
            move.IsPushPole = isPushPole;
            if (isPushPole)
            {
                move.Pause = ((TjcTogetherInfo)T.DataRead.TogetherInfo).PushPolePause;
            }
            myBinding.Converter = move;
            (isPushPole ? tjc.pushPole : tjc.pingPole).SetBinding(MarginProperty, myBinding);
        }
        #endregion
        #region LjcBinding  
        private void LjcBinding()
        {
            //炉号
            RoomNumBinding((Vehicles.Ljc)Communication.CarsLst[2], ljc1);
            //车辆的移动
            MoveBinding((Vehicles.Ljc)Communication.CarsLst[2], ljc1, 52.5, 130);
            //焦槽位移
            TroughMoveBinding(Communication.CarsLst[2], ljc1);

            RoomNumBinding((Vehicles.Ljc)Communication.CarsLst[3], ljc2);
            MoveBinding((Vehicles.Ljc)Communication.CarsLst[3], ljc2, 913.5, 130);
            TroughMoveBinding(Communication.CarsLst[3], ljc2);
        }
        private void TroughMoveBinding(Vehicle L, UIVehicals.Ljc ljc)
        {
            Binding myBinding = new Binding();
            //myBinding.Source = ((Vehicles.Ljc)L).DataRead.TogetherInfo;
            myBinding.Source = L.DataRead.TogetherInfo;
            myBinding.Path = new PropertyPath("TroughLocked");
            TroughMoveConverter move = new TroughMoveConverter();
            myBinding.Converter = move;
            ljc.trough.SetBinding(MarginProperty, myBinding);
        }
        #endregion
        #region McBinding
        private void McBinding()
        {
            //炉号
            RoomNumBinding((Vehicles.Mc)Communication.CarsLst[6], mc1);
            RoomNumBinding((Vehicles.Mc)Communication.CarsLst[7], mc2);
            //车辆移动
            MoveBinding(Communication.CarsLst[6], mc1, 56, 210);
            MoveBinding(Communication.CarsLst[7], mc2, 917, 210);
        }
        #endregion
        #region XjcBinding
        private void XjcBinding()
        {
            //熄焦车实例化，根据设置界面设置的参数 来生成熄焦车
            GetXjc();
            //显示炉号Binding
            RoomNumBinding(Communication.CarsLst[4], xjc1);
            RoomNumBinding(Communication.CarsLst[5], xjc2);

            //车辆移动
            XjcMoveBinding(Communication.CarsLst[4], xjc1, Setting.AreaFlag ? 7 : 56, 88);
            XjcMoveBinding(Communication.CarsLst[5], xjc2, 868, 88);
            //if (Setting.areaFlag)
            //{//1、2#炉区
            //    MoveBinding(Communication.CarsInfo[4], xjc1, 7, 88, new XjcMoveHelper(true, 49));
            //    MoveBinding(Communication.CarsInfo[5], xjc2, 868, 88, new XjcMoveHelper(true, 98));
            //}
            //else
            //{//3、4#炉区
            //    MoveBinding(Communication.CarsInfo[4], xjc1, 56, 88, new XjcMoveHelper(true, -98));
            //    MoveBinding(Communication.CarsInfo[5], xjc2, 868, 88, new XjcMoveHelper(true, -49));
            //}
        }
        /// <summary>
        /// 在参数设置界面配置bool变量来选择生成1\2#炉区
        /// 或者 3、4#炉区的熄焦车(未配置,20171022,默认为创建1/2#炉区的熄焦车)
        /// </summary>
        private void GetXjc()
        {
            if (Setting.AreaFlag)
            {//1、2#炉区
                xjc1 = new D_Xjc1() { Margin = new Thickness(7, 88, 0, 0) };//干熄 在左侧
                xjc2 = new W_Xjc1() { Margin = new Thickness(868, 88, 0, 0) };//水熄 在右侧
            }
            else
            {//3、4#炉区
                xjc1 = new W_Xjc2() { Margin = new Thickness(56, 88, 0, 0) };//水熄 在左侧
                xjc2 = new D_Xjc2() { Margin = new Thickness(868, 88, 0, 0) };//干熄 在右侧
            }
            mainCanvas.Children.Add(xjc1);
            mainCanvas.Children.Add(xjc2);
            Panel.SetZIndex(xjc1, 3);
            Canvas.SetZIndex(xjc2, 3);
        }
        #endregion
        private void SetZIndex()
        {
            Canvas.SetZIndex(tjc1, 3);
            Canvas.SetZIndex(tjc2, 3);
            Canvas.SetZIndex(ljc1, 4);
            Canvas.SetZIndex(ljc2, 4);

            Canvas.SetZIndex(mc1, 4);
            Canvas.SetZIndex(mc2, 4);
        }
        private void RoomNumBinding(Vehicle V, UserControl C)
        {
            Binding myBinding = new Binding();
            myBinding.Source = V;
            myBinding.Path = new PropertyPath("RoomNum");
            RoomNumConverter move = new RoomNumConverter();
            move.CarNum = V.CarNum;
            myBinding.Converter = move;
            GetTextBoxOfRoomNum(C).SetBinding(TextBox.TextProperty, myBinding);
        }
        private TextBox GetTextBoxOfRoomNum(UserControl control)
        {
            Type t = control.GetType();
            string typeName = t.Name;
            TextBox txt = new TextBox();
            switch (typeName)
            {
                case "Tjc":
                    txt = ((UIVehicals.Tjc)control).mark;
                    break;
                case "Ljc":
                    txt = ((UIVehicals.Ljc)control).mark;
                    break;
                case "D_Xjc1":
                    txt = ((D_Xjc1)control).headStock.mark;
                    break;
                case "D_Xjc2":
                    txt = ((D_Xjc2)control).headStock.mark;
                    break;
                case "W_Xjc1":
                    txt = ((W_Xjc1)control).headStock.mark;
                    break;
                case "W_Xjc2":
                    txt = ((W_Xjc2)control).headStock.mark;
                    break;
                case "Mc":
                    txt = ((UIVehicals.Mc)control).mark;
                    break;
                default:
                    break;
            }
            return txt;
        }
        private void MoveBinding(Vehicle car, UserControl control, double marginLeft, double defaultTop)
        {
            Binding myBinding = new Binding();
            myBinding.Source = car;
            myBinding.Path = new PropertyPath("RoomNum");
            MoveConverter move = new MoveConverter();
            move.CarNum = car.CarNum;
            move.DefalutLeft = marginLeft;
            move.DefaultTop = defaultTop;
            myBinding.Converter = move;
            control.SetBinding(MarginProperty, myBinding);
        }
        private void XjcMoveBinding(Vehicle car, UserControl control, double marginLeft, double defaultTop)
        {
            MultiBinding binding = new MultiBinding();

            Binding roomNum = new Binding("RoomNum");
            roomNum.Source = car;

            Binding carNum = new Binding("CarNum");
            carNum.Source = car;

            Binding canNum = new Binding("CanNum");
            canNum.Source = car.DataRead.TogetherInfo;

            Binding addr = new Binding("PhysicalAddr");
            addr.Source = car.DataRead;

            XjcMutiConverter converter = new XjcMutiConverter();
            converter.DefalutLeft = marginLeft;
            converter.DefaultTop = defaultTop;
            converter.Deviation = car.CarNum == 1 ? (Setting.AreaFlag ? 49 : -98) : (Setting.AreaFlag ? 98 : -49);

            binding.Bindings.Add(roomNum);
            binding.Bindings.Add(carNum);
            binding.Bindings.Add(canNum);
            binding.Bindings.Add(addr);
            binding.Converter = converter;

            control.SetBinding(MarginProperty, binding);
        }
        private void TjcMoveBinding(Vehicle car, UserControl control, double marginLeft, double defaultTop)
        {
            MultiBinding binding = new MultiBinding();

            Binding roomNum = new Binding();
            roomNum.Source = car;
            roomNum.Path = new PropertyPath("RoomNum");

            Binding addr = new Binding("PhysicalAddr");
            addr.Source = car.DataRead;

            TjcMutiConverter move = new TjcMutiConverter();
            move.CarNum = car.CarNum;
            move.DefalutLeft = marginLeft;
            move.DefaultTop = defaultTop;

            binding.Bindings.Add(roomNum);
            binding.Bindings.Add(addr);
            binding.Converter = move;

            control.SetBinding(MarginProperty, binding);
        }
        private void _Binding()
        {
            TjcBinding();
            LjcBinding();
            XjcBinding();
            McBinding();
        }

    }
}
