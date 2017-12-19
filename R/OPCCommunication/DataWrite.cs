using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using WGPM.R.RoomInfo;

namespace WGPM.R.OPCCommunication
{
    class DataWrite
    {
        /// <summary>
        /// 上位机写计数
        /// </summary>
        public ushort SoftwareCount
        {
            get
            {
                if (softwareCount > 60 * 60)
                {
                    softwareCount = 1;
                }
                return softwareCount++;
            }
        }
        private ushort softwareCount = 1;
        /// <summary>
        /// 系统时间
        /// </summary>
        public ushort SysTime
        {
            get
            {
                ushort sysTime = (ushort)(DateTime.Now.Hour * 100 + DateTime.Now.Minute);
                return sysTime;
            }
        }
        /// <summary>
        /// 系统时间秒
        /// </summary>
        public ushort SysTimeSec
        {
            get
            {
                ushort sec = (ushort)DateTime.Now.Second;
                return sec;
            }
        }
        /// <summary>
        /// 焦杆长度
        /// </summary>
        public ushort PushPoleLengh { get; set; }
        /// <summary>
        /// 计划时间：分推焦和装煤
        /// </summary>
        public ushort PlanTime { get; set; }
        public ushort planTime { get; set; }
        /// <summary>
        /// 计划炉号，如055
        /// </summary>
        public ushort PushPlanRoomNum { get; set; }
        //计划显示炉号：PLC用来和当前车显示炉号作对比
        public ushort DisplayPushPlanRoomNum { get; set; }
        //当前车炉号
        public ushort CurrentCarCokeNum { get; set; }
        //工作推焦车显示炉号
        public ushort TJobCarCokeNum { get; set; }
        //工作拦焦车显示炉号
        public ushort LJobCarCokeNum { get; set; }
        //工作熄焦车显示炉号
        public ushort XJobCarCokeNum { get; set; }
        //工作装煤车显示炉号
        public ushort MJobCarCokeNum { get; set; }
        //螺旋转速1
        public ushort SpirilSpeed1 { get; set; }
        //螺旋转速2
        public ushort SpirilSpeed2 { get; set; }
        //螺旋转速3
        public ushort SpirilSpeed3 { get; set; }
        //螺旋转速4
        public int SpirilSpeed4 { get; set; }
        //工作推焦车物理地址
        public ushort TPhysicalAddr { get; set; }
        //工作拦焦车物理地址
        public int LPhysicalAddr { get; set; }
        //工作熄焦车物理地址
        public int XPhysicalAddr { get; set; }
        //工作煤车物理地址
        public int MPhysicalAddr { get; set; }
        private byte[] GetBytes(short s)
        {
            byte[] count = BitConverter.GetBytes(s);
            return count;
        }
    }
    interface ITogetherInfo
    {

    }
    class DwTogetherInfo : DependencyObject, ITogetherInfo
    {//下列字段（属性的顺序很重要）
        public DwTogetherInfo() { }
        public List<bool> DwIntValue;
        //[1]推焦请求,无用
        public bool PushRequest;
        //[2]推焦联锁
        public bool PushTogether
        {
            get { return (bool)GetValue(PushTogetherProperty); }
            set { SetValue(PushTogetherProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PushTogether.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PushTogetherProperty =
            DependencyProperty.Register("PushTogether", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));
        //[3]推到位
        public bool TJobCarReady
        {
            get { return (bool)GetValue(TJobCarReadyProperty); }
            set { SetValue(TJobCarReadyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TJobCarReady.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TJobCarReadyProperty =
            DependencyProperty.Register("TJobCarReady", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));
        //[4]推炉门已摘
        public bool TRoomDoorOpen
        {
            get { return (bool)GetValue(TRoomDoorOpenProperty); }
            set { SetValue(TRoomDoorOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TRoomDoorOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TRoomDoorOpenProperty =
            DependencyProperty.Register("TRoomDoorOpen", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(true));

        //[5]推焦开始
        public bool PushBegin
        {
            get { return (bool)GetValue(PushBeginProperty); }
            set { SetValue(PushBeginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PushBegin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PushBeginProperty =
            DependencyProperty.Register("PushBegin", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        //[6]推焦结束
        public bool PushEnd
        {
            get { return (bool)GetValue(PushEndProperty); }
            set { SetValue(PushEndProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PushEnd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PushEndProperty =
            DependencyProperty.Register("PushEnd", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        //[7]拦到位
        public bool LJobCarReady
        {
            get { return (bool)GetValue(LJobCarReadyProperty); }
            set { SetValue(LJobCarReadyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LJobCarReady.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LJobCarReadyProperty =
            DependencyProperty.Register("LJobCarReady", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        //[8]拦炉门已摘
        public bool LRoomDoorOpen
        {
            get { return (bool)GetValue(LRoomDoorOpenProperty); }
            set { SetValue(LRoomDoorOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LRoomDoorOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LRoomDoorOpenProperty =
            DependencyProperty.Register("LRoomDoorOpen", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        //[9]导焦槽锁闭
        public bool TroughLocked
        {
            get { return (bool)GetValue(TroughLockedProperty); }
            set { SetValue(TroughLockedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TroughLocked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TroughLockedProperty =
            DependencyProperty.Register("TroughLocked", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(true));

        //[10]拦人工允推
        public bool LAllowPush
        {
            get { return (bool)GetValue(LAllowPushProperty); }
            set { SetValue(LAllowPushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LAllowPush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LAllowPushProperty =
            DependencyProperty.Register("LAllowPush", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        //[11]熄到位
        public bool XJobCarReady
        {
            get { return (bool)GetValue(XJobCarReadyProperty); }
            set { SetValue(XJobCarReadyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for XJobCarReady.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XJobCarReadyProperty =
            DependencyProperty.Register("XJobCarReady", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        //[12]焦罐旋转\车门关闭
        public bool CanReady
        {
            get { return (bool)GetValue(CanReadyProperty); }
            set { SetValue(CanReadyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanReady.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanReadyProperty =
            DependencyProperty.Register("CanReady", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        //[13]熄人工允推
        public bool XAllowPush
        {
            get { return (bool)GetValue(XAllowPushProperty); }
            set { SetValue(XAllowPushProperty, value); }
        }
        // Using a DependencyProperty as the backing store for XAllowPush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XAllowPushProperty =
            DependencyProperty.Register("XAllowPush", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        //[14]干熄/水熄
        public bool Dry
        {
            get { return (bool)GetValue(DryProperty); }
            set { SetValue(DryProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Dry.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DryProperty =
            DependencyProperty.Register("Dry", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        /// <summary>
        /// 用于界面熄焦车的联锁信号点Binding：车门关闭 or 焦罐旋转
        /// false 对应1#或3#熄焦车；true对应2#或4#熄焦车
        /// </summary>
        public bool CarNum
        {
            get { return (bool)GetValue(CarNumProperty); }
            set { SetValue(CarNumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CarNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CarNumProperty =
            DependencyProperty.Register("CarNum", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));


        //[15]焦罐号
        public bool CanNum
        {
            get { return (bool)GetValue(CanNumProperty); }
            set { SetValue(CanNumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanNumProperty =
            DependencyProperty.Register("CanNum", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        //[16]1#焦罐有无
        public bool FstCan
        {
            get { return (bool)GetValue(FstCanProperty); }
            set { SetValue(FstCanProperty, value); }
        }
        // Using a DependencyProperty as the backing store for FstCan.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FstCanProperty =
            DependencyProperty.Register("FstCan", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        //[17]2#焦罐有无
        public bool SecCan
        {
            get { return (bool)GetValue(SecCanProperty); }
            set { SetValue(SecCanProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecCan.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecCanProperty =
            DependencyProperty.Register("SecCan", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        //[18]一级允推
        public bool FstAllow
        {
            get { return (bool)GetValue(FstAllowProperty); }
            //get { return TJobCarReady && TRoomDoorOpen && LJobCarReady && LRoomDoorOpen && TroughLocked && XJobCarReady && CanReady; }
            set { SetValue(FstAllowProperty, value); }
        }
        // Using a DependencyProperty as the backing store for FstAllow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FstAllowProperty =
            DependencyProperty.Register("FstAllow", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));
        //[19]二级允推:一级允推+拦人工允推+熄人工允推+时间允许
        public bool TimeAllow
        {
            get { return (bool)GetValue(TimeAllowProperty); }
            set { SetValue(TimeAllowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeAllow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeAllowProperty =
            DependencyProperty.Register("TimeAllow", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));
        public bool SecAllow
        {
            get { return (bool)GetValue(SecAllowProperty); }
            //get { return FstAllow && LAllowPush && XAllowPush && TimeAllow; }
            set { SetValue(SecAllowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecAllow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecAllowProperty =
            DependencyProperty.Register("SecAllow", typeof(bool), typeof(DwTogetherInfo), new PropertyMetadata(false));

        //[20]平煤请求
        public bool PingRequest;
        //[21]当前车箭头指示
        public bool IsReady { get; set; }
        /// <summary>
        /// 联锁信息转换为一个int数值，方便转换为字节来发送
        /// </summary>
        public int InfoToInt
        {
            get
            {
                int dw = 0;
                DwIntValue = new List<bool>();
                DwIntValue.Add(PushRequest);//[1]推焦请求,无用
                DwIntValue.Add(PushTogether);//[2]推焦联锁
                DwIntValue.Add(TJobCarReady);//[3]推到位
                DwIntValue.Add(TRoomDoorOpen);//[4]推炉门已摘
                DwIntValue.Add(PushBegin);//[5]推焦开始
                DwIntValue.Add(PushEnd);//[6]推焦结束
                DwIntValue.Add(LJobCarReady);//[7]拦到位
                DwIntValue.Add(LRoomDoorOpen);//[8]拦炉门已摘
                DwIntValue.Add(TroughLocked);//[9]导焦槽锁闭
                DwIntValue.Add(LAllowPush);//[10]拦人工允推
                DwIntValue.Add(XJobCarReady);//[11]熄到位
                DwIntValue.Add(CanReady);//[12]焦罐旋转\车门关闭
                DwIntValue.Add(XAllowPush);//[13]熄人工允推
                DwIntValue.Add(Dry);//[14]干熄/水熄
                DwIntValue.Add(CanNum);//[15]焦罐号
                DwIntValue.Add(FstCan);//[16]1#焦罐有无
                DwIntValue.Add(SecCan);//[17]2#焦罐有无
                DwIntValue.Add(FstAllow);//[18]一级允推
                DwIntValue.Add(SecAllow);//[19]二级允推
                DwIntValue.Add(PingRequest);////[20]平煤请求
                DwIntValue.Add(IsReady);////[21]当前车的对中指示
                for (int i = 0; i < 21; i++)
                {//for循环中出现的数字21有明确意义：联锁信息点为上面的21个
                    int b = DwIntValue[i] ? (int)Math.Pow(2, i) : 0;
                    dw += b;
                }

                return dw;
            }
        }
        public ushort[] GetDwUshortArr
        {
            get
            {
                byte[] byteArr = BitConverter.GetBytes(InfoToInt);
                ushort[] ushortArr = new ushort[2];
                ushortArr[0] = BitConverter.ToUInt16(byteArr, 0);
                ushortArr[1] = BitConverter.ToUInt16(byteArr, 2);
                return ushortArr;//
            }
        }
        public bool GetFstAllow()
        {//推到位+推炉门已摘+拦到位+拦炉门已摘+焦槽锁闭+熄到位+熄焦罐ready；20171205 去掉拦焦车炉门已摘
            return TJobCarReady && TRoomDoorOpen &&
                LJobCarReady && TroughLocked &&
                XJobCarReady && CanReady;
        }
        public bool GetSecAllow()
        {
            return FstAllow && LAllowPush && XAllowPush && TimeAllow;
        }
        public bool IsTimeAllow()
        {
            if (CokeRoom.PushPlan.Count == 0) return false;
            return DateTime.Now.Subtract(CokeRoom.PushPlan[0].PushTime).TotalMinutes >= -10 ? true : false;
        }
    }
    class DwMTogetherInfo : DependencyObject, ITogetherInfo
    {
        public DwMTogetherInfo() { }
        public List<bool> DwIntValue;
        /// <summary>
        /// 1正在平煤:推焦车的平煤开始信号
        /// </summary>
        public bool Pinging { get; set; }
        /// <summary>
        /// 推焦车到位
        /// </summary>


        public bool TReady
        {
            get { return (bool)GetValue(TReadyProperty); }
            set { SetValue(TReadyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TReady.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TReadyProperty =
            DependencyProperty.Register("TReady", typeof(bool), typeof(DwMTogetherInfo), new PropertyMetadata(false));

        /// <summary>
        /// 3推小炉门已摘
        /// </summary>
        public bool TMDoorOpen { get; set; }
        /// <summary>
        /// 拦焦车到位
        /// </summary>
        public bool LReady { get; set; }
        /// <summary>
        /// 请求平煤
        /// </summary>
        public bool PingRequest { get; set; }
        /// <summary>
        /// 装煤车到位
        /// </summary>
        public bool MReady { get; set; }
        /// <summary>
        /// 准备平煤：推到位，推小炉门打开
        /// </summary>
        public bool ReadyToPing
        {
            get; set;
        }
        /// <summary>
        /// 允许装煤：煤车到位，炉盖打开，机侧、焦侧炉门关闭
        /// </summary>


        public bool AllowPing
        {
            get { return (bool)GetValue(AllowPingProperty); }
            set { SetValue(AllowPingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllowPing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowPingProperty =
            DependencyProperty.Register("AllowPing", typeof(bool), typeof(DwMTogetherInfo), new PropertyMetadata(false));

        /// <summary>
        /// 允许取煤
        /// </summary>
        public bool AllowGet { get; set; }
        /// <summary>
        /// 装煤联锁
        /// </summary>
        public bool MLock { get; set; }

        /// <summary>
        /// 导套到位
        /// </summary>
        public bool SleeveReady
        {
            get { return (bool)GetValue(SleeveReadyProperty); }
            set { SetValue(SleeveReadyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SleeveReady.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SleeveReadyProperty =
            DependencyProperty.Register("SleeveReady", typeof(bool), typeof(DwMTogetherInfo), new PropertyMetadata(false));


        /// <summary>
        /// 机侧炉门
        /// </summary>
        public bool TDoorClosed
        {
            get { return (bool)GetValue(TDoorClosedProperty); }
            set { SetValue(TDoorClosedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TDoorClosed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TDoorClosedProperty =
            DependencyProperty.Register("TDoorClosed", typeof(bool), typeof(DwMTogetherInfo), new PropertyMetadata(false));

        /// <summary>
        /// 焦侧炉门
        /// </summary>
        public bool LDoorClosed
        {
            get { return (bool)GetValue(LDoorClosedProperty); }
            set { SetValue(LDoorClosedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LDoorClosed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LDoorClosedProperty =
            DependencyProperty.Register("LDoorClosed", typeof(bool), typeof(DwMTogetherInfo), new PropertyMetadata(false));

        /// <summary>
        /// 炉盖打开
        /// </summary>
        public bool LidOpen { get; set; }
        public bool IsPingReady()
        {
            return TReady && TMDoorOpen;
        }
        public bool IsAllowPing()
        {//装煤车到位，炉盖打开，两侧炉门已关
            return MReady && LidOpen && TDoorClosed && LDoorClosed;
        }
        /// <summary>
        /// 联锁信息转换为一个int数值，方便转换为字节来发送
        /// </summary>
        public int InfoToInt
        {
            get
            {
                int dw = 0;
                DwIntValue = new List<bool>();
                DwIntValue.Add(Pinging);//[1]平煤开始
                DwIntValue.Add(TReady);//[2]推焦车到位
                DwIntValue.Add(TMDoorOpen);//[3]焦侧炉门开（小炉门）
                DwIntValue.Add(LReady);//[4]拦焦车到位，无用
                DwIntValue.Add(PingRequest);//[5]请求平煤，无用
                DwIntValue.Add(MReady);//[6]装煤车到位
                DwIntValue.Add(ReadyToPing);//[7]准备平煤
                DwIntValue.Add(AllowPing);//[8]允许装煤
                DwIntValue.Add(AllowGet);//[9]允许取煤
                DwIntValue.Add(MLock);//[10]装煤联锁
                DwIntValue.Add(TDoorClosed);//[11]推炉门关（推焦的炉门）
                DwIntValue.Add(LDoorClosed);//[12]焦侧炉门关（推焦时的拦焦车炉门）
                DwIntValue.Add(LidOpen);//[13]炉盖关闭
                for (int i = 0; i < DwIntValue.Count; i++)
                {//for循环中出现的数字21有明确意义：联锁信息点为上面的21个
                    int b = DwIntValue[i] ? (int)Math.Pow(2, i) : 0;
                    dw += b;
                }
                return dw;
            }
        }
        /// <summary>
        /// 把联锁信息转换为的Int值-->ushort[]数组
        /// </summary>
        public ushort[] GetDwUshortArr
        {
            get
            {
                byte[] byteArr = BitConverter.GetBytes(InfoToInt);
                ushort[] ushortArr = new ushort[2];
                ushortArr[0] = BitConverter.ToUInt16(byteArr, 0);
                ushortArr[1] = BitConverter.ToUInt16(byteArr, 2);
                return ushortArr;//
            }
        }
    }
}
