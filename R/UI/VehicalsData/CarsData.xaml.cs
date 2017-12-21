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
using System.Windows.Shapes;
using WGPM.R.OPCCommunication;
using WGPM.R.Vehicles;
using WGPM.R.UI.UIConverter;

namespace WGPM.R.UI.VehicalsData
{
    /// <summary>
    /// CarsData.xaml 的交互逻辑
    /// 20171126 可以把四大车的共有信息：车号，炉号，物理地址，灯状态，三个计数 等整合为一个用户控件
    /// </summary>
    public partial class CarsData : Window
    {
        public CarsData()
        {
            InitializeComponent();
            _Binding();
        }

        private void _Binding()
        {
            CarsTitles();
            TBinding();
            LBinding();
            XBinding();
            MBinding();
        }
        #region 通用Binding方法
        private void TogetherInfoCellBinding(TextBox txt, string path, Vehicle car)
        {
            BoolToIntBinding(txt, path, car);
            BoolToColorBinding(txt, path, car);
        }
        private void BoolToIntBinding(TextBox txt, string path, Vehicle car)
        {
            Binding myBinding = new Binding(path);
            myBinding.Source = car.DataRead.TogetherInfo;
            BoolToIntConverter converter = new BoolToIntConverter();
            myBinding.Mode = BindingMode.OneWay;
            myBinding.Converter = converter;
            txt.SetBinding(TextBox.TextProperty, myBinding);
        }
        private void BoolToColorBinding(TextBox txt, string path, Vehicle car)
        {
            //0时为默认颜色，1时为红色
            Binding myBinding1 = new Binding(path);
            myBinding1.Source = car.DataRead.TogetherInfo;
            BoolToColorConverter converter1 = new BoolToColorConverter();
            converter1.DefaultColor = txt.Background;
            converter1.TrueToColor = Brushes.Red;
            myBinding1.Converter = converter1;
            txt.SetBinding(BackgroundProperty, myBinding1);
        }
        private void CarDataBinding(TextBox txt, string path, Vehicle car)
        {
            Binding myBinding = new Binding(path);
            myBinding.Source = car;
            txt.SetBinding(TextBox.TextProperty, myBinding);
        }
        private void DataReadBinding(TextBox txt, string path, Vehicle car)
        {
            Binding myBinding = new Binding(path);
            myBinding.Source = car.DataRead;
            txt.SetBinding(TextBox.TextProperty, myBinding);
        }
        private void IntTogetherInfoBinding(TextBox txt, string path, Vehicle car)
        {
            Binding myBinding = new Binding(path);
            myBinding.Source = car.DataRead.TogetherInfo;
            txt.SetBinding(TextBox.TextProperty, myBinding);
        }
        private void TogetherInfoCellBinding(Node node, string path, Vehicle car1, Vehicle car2)
        {
            IntTogetherInfoBinding(node.car1, path, car1);
            IntTogetherInfoBinding(node.car2, path, car2);
        }
        /// <summary>
        /// Binding公共信息：车号，炉号，物理地址，灯状态，三个计数，联锁信息value
        /// </summary>
        /// <param name="main"></param>
        /// <param name="car1"></param>
        /// <param name="car2"></param>
        private void UnitBinding(CommonTitle main, Vehicle car1, Vehicle car2)
        {
            CarCellBinding(main.CarNum, "CarNum", car1, car2);
            CarCellBinding(main.RoomNum, "RoomNum", car1, car2);
            DataReadCellBinding(main.Addr, "PhysicalAddr", car1, car2);
            DataReadCellBinding(main.Light, "LightStatus", car1, car2);
            DataReadCellBinding(main.DecodeCount, "DecodeCount", car1, car2);
            DataReadCellBinding(main.PLCCount, "PLCCount", car1, car2);
            DataReadCellBinding(main.TouchCount, "TouchCount", car1, car2);
            TogetherInfoCellBinding(main.TogetherInfo, "TogetherInfoValue", car1, car2);
        }
        private void CarCellBinding(Node node, string path, Vehicle car1, Vehicle car2)
        {
            CarDataBinding(node.car1, path, car1);
            CarDataBinding(node.car2, path, car2);

        }
        private void DataReadCellBinding(Node node, string path, Vehicle car1, Vehicle car2)
        {
            DataReadBinding(node.car1, path, car1);
            DataReadBinding(node.car2, path, car2);
        }
        private void BoolCellBinding(Node node, string path, Vehicle car1, Vehicle car2)
        {
            TogetherInfoCellBinding(node.car1, path, car1);
            TogetherInfoCellBinding(node.car2, path, car2);
        }
        #endregion
        private void TBinding()
        {
            Tjc t1 = (Tjc)Communication.CarsLst[0];
            Tjc t2 = (Tjc)Communication.CarsLst[1];
            UnitBinding(tjc, t1, t2);
            DataReadCellBinding(tPushCur, "PushCur", t1, t2);
            DataReadCellBinding(tPingCur, "PingCur", t1, t2);
            DataReadCellBinding(tPushPole, "PushPoleLength", t1, t2);
            DataReadCellBinding(tPingPole, "PingPoleLength", t1, t2);
            BoolCellBinding(tAskPush, "PushRequest", t1, t2);
            BoolCellBinding(tDoorOpen, "DoorOpen", t1, t2);
            BoolCellBinding(tPushLock, "PushTogether", t1, t2);
            BoolCellBinding(tPickDoorLock, "PickDoorTogether", t1, t2);
            BoolCellBinding(tPushBegin, "PushBegin", t1, t2);
            BoolCellBinding(tPushEnd, "PushEnd", t1, t2);
            BoolCellBinding(tPingBegin, "PingBegin", t1, t2);
            BoolCellBinding(tPingEnd, "PingEnd", t1, t2);
            BoolCellBinding(tPushForward, "PushPoleForward", t1, t2);
            BoolCellBinding(tPause, "PushPolePause", t1, t2);
            BoolCellBinding(tPushEndFlag, "PushEndFlag", t1, t2);
            BoolCellBinding(tPMDoorOpen, "PMDoorOpen", t1, t2);
        }
        private void LBinding()
        {
            Ljc l1 = (Ljc)Communication.CarsLst[2];
            Ljc l2 = (Ljc)Communication.CarsLst[3];
            UnitBinding(ljc, l1, l2);
            BoolCellBinding(lAllowPush, "AllowPush", l1, l2);
            BoolCellBinding(lDoorOpen, "DoorOpen", l1, l2);
            BoolCellBinding(lTroughLock, "TroughLocked", l1, l2);
            BoolCellBinding(lPickDoor, "PickDoorTogether", l1, l2);
            BoolCellBinding(lBan, "Ban", l1, l2);
        }
        private void XBinding()
        {
            Xjc x1 = (Xjc)Communication.CarsLst[4];
            Xjc x2 = (Xjc)Communication.CarsLst[5];
            UnitBinding(xjc, x1, x2);
            BoolCellBinding(xAllowPush, "AllowPush", x1, x2);
            BoolCellBinding(xCanReady, "CanReady", x1, x2);
            BoolCellBinding(xCanNum, "CanNum", x1, x2);
            BoolCellBinding(xDry, "Dry", x1, x2);
            BoolCellBinding(xBan, "Ban", x1, x2);
            BoolCellBinding(xFstCan, "FstCan", x1, x2);
            BoolCellBinding(xSecCan, "SecCan", x1, x2);

        }
        private void MBinding()
        {
            Vehicle m1 = Communication.CarsLst[6];
            Vehicle m2 = Communication.CarsLst[7];
            UnitBinding(mc, m1, m2);
            DataReadCellBinding(mSpiral1, "SprialSpeed1", m1, m2);
            DataReadCellBinding(mSpiral2, "SprialSpeed2", m1, m2);
            DataReadCellBinding(mSpiral3, "SprialSpeed3", m1, m2);
            DataReadCellBinding(mSpiral4, "SprialSpeed4", m1, m2);
            BoolCellBinding(mStokingLock, "StokingTogether", m1, m2);
            BoolCellBinding(mAskPing, "PingRequest", m1, m2);
            BoolCellBinding(mSleeveReady, "SleeveReady", m1, m2);
            BoolCellBinding(mLidOpen, "LidOpen", m1, m2);
            BoolCellBinding(mGateSegmentOpen, "GateSegentOpen", m1, m2);
            BoolCellBinding(mExhaustDuctReady, "ExhaustDuctReady", m1, m2);
            BoolCellBinding(mStokingBegin, "StokingBegin", m1, m2);
            BoolCellBinding(mStokingEnd, "StokingEnd", m1, m2);
            BoolCellBinding(mGetLidTogether, "GetLidTogether", m1, m2);
            BoolCellBinding(mVFDRuning, "VFDRuning", m1, m2);
        }
        #region Titles ItemName
        private void Titles(Node node, string title)
        {
            node.node.Text = title;
        }
        private void CommonTitles(CommonTitle title)
        {
            Titles(title.CarNum, "车号");
            Titles(title.RoomNum, "炉号");
            Titles(title.Addr, "物理地址");
            Titles(title.Light, "灯状态");
            Titles(title.DecodeCount, "Decode");
            Titles(title.PLCCount, "PLC");
            Titles(title.TouchCount, "Touch");
            Titles(title.TogetherInfo, "TogetherInfo");
        }
        private void TjcTitles()
        {
            CommonTitles(tjc);
            Titles(tPushCur, "PushCur");
            Titles(tPingCur, "PingCur");
            Titles(tPushPole, "PushPole");
            Titles(tPingPole, "PingPole");
            Titles(tAskPush, "请求");
            Titles(tDoorOpen, "炉门已摘");
            Titles(tPushLock, "联锁");
            Titles(tPickDoorLock, "摘门联锁");
            Titles(tPushBegin, "推焦开始");
            Titles(tPushEnd, "推焦结束");
            Titles(tPingBegin, "平煤开始");
            Titles(tPingEnd, "平煤结束");
            Titles(tPushForward, "PoleForward");
            Titles(tPause, "PushPause");
            Titles(tPushEndFlag, "PushEndFlag");
            Titles(tPMDoorOpen, "小炉门");
        }
        private void LjcTitles()
        {
            CommonTitles(ljc);
            Titles(lAllowPush, "AllowPush");
            Titles(lDoorOpen, "DoorOpen");
            Titles(lTroughLock, "焦槽锁闭");
            Titles(lPickDoor, "PickDoor");
            Titles(lBan, "Ban");
        }
        private void XjcTitles()
        {
            CommonTitles(xjc);
            Titles(xAllowPush, "AllowPush");
            Titles(xCanReady, @"旋转/关闭");
            Titles(xCanNum, "罐号");
            Titles(xDry, "干熄");
            Titles(xBan, "Ban");
            Titles(xFstCan, "FstCan");
            Titles(xSecCan, "SecCan");

        }
        private void McTitles()
        {
            CommonTitles(mc);
            Titles(mSpiral1, "螺旋1");
            Titles(mSpiral2, "螺旋1");
            Titles(mSpiral3, "螺旋1");
            Titles(mSpiral4, "螺旋1");
            Titles(mStokingLock, "联锁");
            Titles(mAskPing, "请求");
            Titles(mSleeveReady, "导套");
            Titles(mLidOpen, "炉盖");
            Titles(mGateSegmentOpen, "炉盖");
            Titles(mExhaustDuctReady, "除尘");
            Titles(mStokingBegin, "Begin");
            Titles(mStokingEnd, "End");
            Titles(mGetLidTogether, "取盖");
            Titles(mVFDRuning, "VFD");
        }
        private void CarsTitles()
        {
            TjcTitles();
            LjcTitles();
            XjcTitles();
            McTitles();
        }
        #endregion

    }

}
