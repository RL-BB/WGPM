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
        }
        private void BoolToIntBinding(TextBox txt, string path, Vehicle car)
        {
            Binding myBinding = new Binding(path);
            myBinding.Source = car;
            BoolToIntConverter converter = new BoolToIntConverter();
            myBinding.Converter = converter;
            txt.SetBinding(TextBox.TextProperty, myBinding);
        }
        private void IntDataBinding(TextBox txt, string path, Vehicle car)
        {
            Binding myBinding = new Binding(path);
            myBinding.Source = car;
            txt.SetBinding(TextBox.TextProperty, myBinding);
        }
        private void TBinding(TextBox txt, Vehicle car)
        {
            IntDataBinding(txt, "CarNum", car);//车号
            IntDataBinding(txt, "RoomNum", car);//炉号
            IntDataBinding(txt, "PhysicalAddr", car);//物理地址
            IntDataBinding(txt, "RoomNum", car);//灯状态
            IntDataBinding(txt, "RoomNum", car);//解码器计数
            IntDataBinding(txt, "RoomNum", car);//炉号
            IntDataBinding(txt, "RoomNum", car);//炉号
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
            Titles(title.TouchCount, "TogetherInfo");
        }
        private void TjcTitles()
        {
            CommonTitles(tjc);
            Titles(tPushCur, "PushCur");
            Titles(tPingCur, "PingCur");
            Titles(tPushPole, "PushPole");
            Titles(tPingPole, "PingPole");
            Titles(tAskPush, "AskPush");
            Titles(tDoorOpen, "DoorOpen");
            Titles(tPushLock, "PushLock");
            Titles(tPickDoorLock, "PickDoorLock");
            Titles(tPushBegin, "PushBegin");
            Titles(tPushEnd, "PushEnd");
            Titles(tPingBegin, "PingBegin");
            Titles(tPingEnd, "PingEnd");
            Titles(tPushForward, "PoleForward");
            Titles(tPause, "PushPause");
            Titles(tPushEndFlag, "PushEndFlag");
            Titles(tPMDoorOpen, "PMDoor");
        }
        private void LjcTitles()
        {
            CommonTitles(ljc);
            Titles(lAllowPush, "AllowPush");
            Titles(lDoorOpen, "DoorOpen");
            Titles(lTroughLock, "TroughLock");
            Titles(lPickDoor, "PickDoor");
            Titles(lBan, "Ban");
        }
        private void XjcTitles()
        {
            CommonTitles(xjc);
            Titles(xAllowPush, "AllowPush");
            Titles(xCanReady, "CanReady");
            Titles(xCanNum, "CanNum");
            Titles(xDry, "干熄");
            Titles(xBan, "Ban");
            Titles(xFstCan, "FstCan");
            Titles(xSecCan, "SecCan");

        }
        private void McTitles()
        {
            CommonTitles(mc);
            Titles(mSpiral1, "Sprial1");
            Titles(mSpiral2, "Sprial2");
            Titles(mSpiral3, "Sprial3");
            Titles(mSpiral4, "Sprial4");
            Titles(mStokingLock, "StokingLock");
            Titles(mAskPing, "AskPing");
            Titles(mSleeveReady, "导套");
            Titles(mLidOpen, "炉盖");
            Titles(mGateSegmentOpen, "炉盖");
            Titles(mExhaustDuctReady, "闸板");
            Titles(tPushCur, "除尘");
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
