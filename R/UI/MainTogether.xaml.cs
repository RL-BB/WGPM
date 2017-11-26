using System;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Expression.Shapes;
using WGPM.R.RoomInfo;
using WGPM.R.OPCCommunication;
using WGPM.R.Vehicles;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Shapes;
using WGPM.R.UI.UIConverter;
using System.Collections.Generic;
using System.Windows;

namespace WGPM.R.UI
{
    /// <summary>
    ///     MainTogether.xaml 的交互逻辑
    /// </summary>
    public partial class MainTogether
    {
        public MainTogether()//联锁界面
        {
            InitializeComponent();
            _Binding();

        }
        #region TLockInfoBinding
        /// <summary>
        /// 联锁信息颜色Binding
        /// </summary>
        private void TLockInfoBinding()
        {
            //推焦联锁，T/LX/M对中信号，炉门已摘，焦槽锁闭，车门关闭/焦罐旋转
            //一级允推，允许时间，L人工允推，X人工允推，二级允推
            TLockInfoToColor(txtPushTogethor, "PushTogether", Communication.JobCarTogetherInfo);
            TLockInfoToColor(txtTjcDoorOpen, "TRoomDoorOpen", Communication.JobCarTogetherInfo);
            TLockInfoToColor(txtLjcTroughLocked, "TroughLocked", Communication.JobCarTogetherInfo);
            TLockInfoToColor(txtCanReady, "CanReady", Communication.JobCarTogetherInfo);
            TLockInfoToColor(txtFstAllow, "FstAllow", Communication.JobCarTogetherInfo);
            TLockInfoToColor(txtTimeAllow, "TimeAllow", Communication.JobCarTogetherInfo);
            TLockInfoToColor(txtLjcAllowPush, "LAllowPush", Communication.JobCarTogetherInfo);
            TLockInfoToColor(txtXjcAllowPush, "XAllowPush", Communication.JobCarTogetherInfo);
            TLockInfoToColor(txtSecAllow, "SecAllow", Communication.JobCarTogetherInfo);
        }
        #endregion
        #region DisplayRoomBinding
        /// <summary>
        /// 工作车的炉号显示
        /// </summary>
        private void UIRoomNumBinding()
        {
            JobCarDisplayRoomNum(Communication.TJobCarLst[0], txtTJobCar);
            JobCarDisplayRoomNum(Communication.TJobCarLst[1], txtLJobCar);
            JobCarDisplayRoomNum(Communication.TJobCarLst[2], txtXJobCar);
            JobCarDisplayRoomNum(Communication.MJobCarLst[1], txtMJobCar);
        }
        private void JobCarDisplayRoomNum(Vehicle V, TextBox txt)
        {
            Binding binding = new Binding("UIRoomNum");
            binding.Source = V;
            UIRoomNumConverter converter = new UIRoomNumConverter();
            binding.Converter = converter;
            txt.SetBinding(TextBox.TextProperty, binding);
        }
        #endregion
        #region MLockInfoBinding
        private void MLockInfoBinding()
        {
            MLockInfoToColor(txtMcSleeve, "SleeveReady", Communication.MJobCarTogetherInfo);
            MLockInfoToColor(txtTDoorClosed, "TDoorClosed", Communication.MJobCarTogetherInfo);
            MLockInfoToColor(txtLDoorClosed, "LDoorClosed", Communication.MJobCarTogetherInfo);
            MLockInfoToColor(txtMcSleeve, "SleeveReady", Communication.MJobCarTogetherInfo);
            MLockInfoToColor(txtAllowStoking, "AllowPing", Communication.MJobCarTogetherInfo);
            MLockInfoToColor(txtTPingReady, "TReady", Communication.MJobCarTogetherInfo);

        }
        #endregion
        #region CentringBinding
        /// <summary>
        /// 对中指示  binding
        /// </summary>
        private void CentringBinding()
        {
            EllipseBinding(tReady, Communication.TJobCarLst[0]);
            EllipseBinding(lReady, Communication.TJobCarLst[1]);
            EllipseBinding(xReady, Communication.TJobCarLst[2]);
            EllipseBinding(mReady, Communication.MJobCarLst[1]);
        }
        private void EllipseBinding(Ellipse ep, Vehicle V)
        {
            Binding mb = new Binding("Arrows");
            mb.Source = V;
            IntToColorConverter converter = new IntToColorConverter();
            converter.DefaultColor = ep.Fill;
            mb.Converter = converter;
            ep.SetBinding(Shape.FillProperty, mb);
        }
        #endregion
        #region ArrowsBinding
        /// <summary>
        /// 箭头颜色Binding
        /// </summary>
        private void ArrowsBinding()
        {
            BlockArrow[] tArrows = { tArrow011, tArrow02, tArrow03, tArrow04 };
            BlockArrow[] lArrows = { lArrow011, lArrow02, lArrow03, lArrow04 };
            BlockArrow[] xArrows = { xArrow011, xArrow02, xArrow03, xArrow04 };
            //BlockArrow[] xArrows = { xArrow04, xArrow03, xArrow02, xArrow011 };
            BlockArrow[] mArrows = { mArrow011, mArrow02, mArrow03, mArrow04 };
            ArrowsColor(Communication.TJobCarLst[0], tArrows);
            ArrowsColor(Communication.TJobCarLst[1], lArrows);
            ArrowsColor(Communication.MJobCarLst[1], mArrows);
            ArrowsColor(Communication.TJobCarLst[2], xArrows);

        }
        private void ArrowsColor(Vehicle V, BlockArrow[] arrows)
        {
            for (int i = 0; i < arrows.Length; i++)
            {
                ArrowColorBinding(V, arrows[i], i);
            }

        }
        private void ArrowColorBinding(Vehicle V, BlockArrow arrow, int index)
        {
            Binding myBinding = new Binding();
            myBinding.Source = V;
            myBinding.Path = new System.Windows.PropertyPath("Arrows");
            ArrowsColorConverter converter = new ArrowsColorConverter();
            converter.DefaultColor = arrow.Fill;
            converter.Index = index;
            myBinding.Converter = converter;
            arrow.SetBinding(Shape.FillProperty, myBinding);
        }
        #endregion
        #region PlanInfoBinding
        /// <summary>
        /// 推焦计划、装煤计划的炉号，时间信息的显示
        /// </summary>
        private void PlanInfoBinding()
        {
            PlanInfo(Communication.PushPlan, txtPushPlanRoomNum, "RoomNum", false);
            PlanInfo(Communication.PushPlan, txtPushPlanTime, "PushTime", true);
            PlanInfo(Communication.StokingPlan, txtStokingPlanRoomNum, "RoomNum", false);
            PlanInfo(Communication.StokingPlan, txtStokingPlanTime, "StokingTime", true);

        }
        private void PlanInfo(IPlan plan, TextBox txt, string path, bool time)
        {
            Binding binding = new Binding();
            binding.Source = plan;
            binding.Path = new System.Windows.PropertyPath(path);
            binding.StringFormat = string.Format(time ? "HH:mm" : "000");
            txt.SetBinding(TextBox.TextProperty, binding);
        }
        #endregion
        public void _Binding()
        {
            UITimeBinding();
            PlanInfoBinding();
            UIRoomNumBinding();
            ArrowsBinding();
            CentringBinding();
            TLockInfoBinding();
            MLockInfoBinding();
            CanReadyTextBinding();
            //20171105 非正常需求 正式使用时 记得注释掉
            
            if (!Setting.AreaFlag)
            {
                xjc1Addr.Visibility = Visibility.Visible;
                xjc2Addr.Visibility = Visibility.Visible;
                XjcAddrBinding();
            }
            else
            {
                xjc1Addr.Visibility = Visibility.Collapsed;
                xjc2Addr.Visibility = Visibility.Collapsed;
            }
        }
        private void TLockInfoToColor(TextBox txt, string path, DwTogetherInfo info)
        {
            Binding mybinding = new Binding();
            mybinding.Source = info;
            mybinding.Path = new PropertyPath(path);
            BoolToColorConverter converter = new BoolToColorConverter();
            converter.DefaultColor = txt.Background;
            converter.TrueToColor = Brushes.Red;
            mybinding.Converter = converter;
            txt.SetBinding(BackgroundProperty, mybinding);
        }
        private void MLockInfoToColor(TextBox txt, string path, DwMTogetherInfo info)
        {
            Binding mybinding = new Binding();
            mybinding.Source = info;
            mybinding.Path = new System.Windows.PropertyPath(path);
            BoolToColorConverter converter = new BoolToColorConverter();
            converter.DefaultColor = txt.Background;
            converter.TrueToColor = Brushes.Red;
            mybinding.Converter = converter;
            txt.SetBinding(BackgroundProperty, mybinding);
        }
        /// <summary>
        /// 焦罐旋转/车门关闭
        /// </summary>
        /// <param name="XJob"></param>
        private void CanReadyTextBinding()
        {
            Binding myBinding = new Binding("CarNum");
            myBinding.Source = Communication.JobCarTogetherInfo;
            myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            CanReadyTextConverter converter = new CanReadyTextConverter();
            myBinding.Converter = converter;
            txtCanReady.SetBinding(TextBox.TextProperty, myBinding);
        }
        private void UITimeBinding()
        {
            Binding myBinding = new Binding("DateTime");
            myBinding.Source = Communication.UITime;
            UITimeConverter converter = new UITimeConverter();
            converter.Date = false;
            myBinding.Converter = converter;
            txtSysTime.SetBinding(TextBox.TextProperty, myBinding);
        }
        private void XjcAddrBinding()
        {
            XjcAddr(xjc1Addr, (Xjc)Communication.CarsInfo[4]);
            XjcAddr(xjc2Addr, (Xjc)Communication.CarsInfo[5]);
        }
        private void XjcAddr(TextBox txt,Xjc xjc)
        {
            Binding myBinding = new Binding("PhysicalAddr");
            myBinding.Source = xjc.DataRead;
            AddrToStringConverter converter = new AddrToStringConverter();
            myBinding.Converter = converter;
            txt.SetBinding(TextBox.TextProperty, myBinding);
        }
    }
}