using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using WGPM.R.OPCCommunication;
using WGPM.R.Parms;
using WGPM.R.RoomInfo;
using WGPM.R.UI;

namespace WGPM.R.Vehicles
{
    class Xjc : Vehicle
    {
        public Xjc(ushort carNum)
        {
            base.CarNum = carNum;
            GetArrows = GetXArrows;//给委托赋值
            //熄焦车物理地址对应炉号的字典有四个
            togetherInfoCount = 5;
            //DataRead.DecodeOtherDataRead = ((XjcDataRead)DataRead).DecodeDataReadValue;
            DataRead = new XjcDataRead();
            DataRead.TogetherInfo = new XjcTogetherInfo(TogetherInfoCount);
            DataRead.TogetherInfo.DecodeTogetherInfo = ((XjcTogetherInfo)DataRead.TogetherInfo).DecodeTogetherInfoValue;
            MoveHelper = new XjcMoveHelper(carNum);
        }
        public Dictionary<int, int> RoomNumDic
        {
            get
            {//未考虑水熄罐；（此处默认水熄罐为2#罐，实际上是有问题的）
                Dictionary<int, int> roomNumDic = new Dictionary<int, int>();
                XjcTogetherInfo info = (XjcTogetherInfo)DataRead.TogetherInfo;
                roomNumDic = Setting.AreaFlag ? (CarNum == 1 ? (info.CanNum ? Addrs.X1SecCanRoomNumDic : Addrs.XFstCanRoomNumDic) : Addrs.X2SecCanRoomNumDic)
                    : (CarNum == 1 ? Addrs.X1SecCanRoomNumDic : (info.CanNum ? Addrs.XFstCanRoomNumDic : Addrs.X2SecCanRoomNumDic));
                return roomNumDic;
            }
        }
        /// <summary>
        /// 得到熄焦车每个车的炉号
        /// </summary>
        public new void GetRoomNum()
        {
            //20171103 不能使用单纯使用车号而应根据是否为水熄焦、干熄焦、焦罐号等联锁点来选择物理地址对应炉号的字典
            //20171104 xjc的联锁信息中 可靠的是罐号  在用罐号；水熄干熄这个点不可靠
            XjcTogetherInfo info = (XjcTogetherInfo)DataRead.TogetherInfo;
            //addrDic = Setting.AreaFlag ? (CarNum==1?(info.CanNum ? Addrs.X1SecCanAddrDic : Addrs.XFstCanAddrDic) :(info.Dry ? Addrs.XFstCanAddrDic : Addrs.X2SecCanAddrDic))
            //    : (CarNum==1?(info.Dry ? Addrs.XFstCanAddrDic : Addrs.X2SecCanAddrDic) :(info.CanNum ? Addrs.X1SecCanAddrDic : Addrs.XFstCanAddrDic));
            //20171004 靠近熄焦车的第一个干熄焦焦罐为1#焦罐（水熄焦焦罐为2#焦罐）：这种定义的用途->用来选择物理地址对应炉号的地址字典；
            //20171104  1、2#炉区的电机车  CanNum=0（false），对应1#焦罐；3、4#炉区的CanNum=1（true）对应1#焦罐；3、4#炉区的司机师傅认为靠近车头的为2#焦罐，这一点和软件上对罐号的认定恰好相反
            bool area = Setting.AreaFlag;
            addrDic = CarNum == 1 ? (area ? (info.CanNum ? Addrs.X1SecCanAddrDic : Addrs.XFstCanAddrDic) : (info.Dry ? Addrs.XFstCanAddrDic : Addrs.X1SecCanAddrDic))
                : (area ? (info.Dry ? Addrs.XFstCanAddrDic : Addrs.X2SecCanAddrDic) : (info.CanNum ? Addrs.XFstCanAddrDic : Addrs.X2SecCanAddrDic));

            if (addrDic.ContainsKey(DataRead.MainPhysicalAddr))
            {
                RoomNum = addrDic[DataRead.MainPhysicalAddr].RoomNum;
            }
        }
        public XjcMoveHelper MoveHelper { get; set; }
        public ushort GetXArrows()
        {
            if (CokeRoom.PushPlan.Count != 0)
            {
                int planRoomNum = CokeRoom.PushPlan[0].RoomNum;
                int actual = DataRead.PhysicalAddr;
                int middle = RoomNumDic[planRoomNum];
                //计划炉号的中心地址，二进制从右向左 为低位到高位
                if (middle - actual > StaticParms.XFstArrow)
                {//左俩箭头:0011
                    Arrows = 3;
                }
                else if ((middle - actual <= StaticParms.TFstArrow) && (middle - actual > StaticParms.XArrow))
                {//左单箭头:0010
                    Arrows = 2;
                }
                else if (Math.Abs(middle - actual) <= StaticParms.XArrow)
                {//对中:0001
                    Arrows = 0;
                }
                else if ((middle - actual < -StaticParms.XArrow) && (middle - actual >= -StaticParms.XFstArrow))
                {//右单箭头:0100；20171201 对中范围扩大
                    //Arrows = 4;
                    Arrows = 0;
                }
                else if (middle - actual < -StaticParms.XFstArrow)
                {//右俩箭头:1100
                    Arrows = 12;
                }
            }
            return Arrows;
        }
        public Vehicle GetJobCar(Xjc car1, Xjc car2)
        {
            if (CokeRoom.PushPlan.Count != 0)
            {
                //1#车和推焦计划炉号的间隔
                int s1 = Math.Abs(car1.DataRead.PhysicalAddr - car1.RoomNumDic[CokeRoom.PushPlan[0].RoomNum]);
                int s2 = Math.Abs(car2.DataRead.PhysicalAddr - car2.RoomNumDic[CokeRoom.PushPlan[0].RoomNum]);
                car1.JobCar = (s1 <= s2) ? true : false;
                car2.JobCar = car1.JobCar ? false : true;
            }
            Vehicle car = car1.JobCar ? car1 : car2;
            return car;
        }
        public void GetMoveHelper()
        {
            MoveHelper.RoomNum = RoomNum;
            MoveHelper.Dry = ((XjcTogetherInfo)DataRead.TogetherInfo).Dry;
            MoveHelper.CanNum = ((XjcTogetherInfo)DataRead.TogetherInfo).CanNum;
        }
    }
    class XjcDataRead : DataRead
    {
        public XjcDataRead()
        {
            //DecodeDataReadValue = areaFlag ? (DecodeDataReadDelegate)DecodeDataRead : DecodeXjcDataRead;
            DecodeDataReadValue = Setting.AreaFlag ? new DecodeDataReadDelegate(DecodeDataRead) : new DecodeDataReadDelegate(DecodeXjcDataRead);
            //DecodeDataReadDelegate d = new DecodeDataReadDelegate(DecodeDataRead);
        }
    }
    class XjcTogetherInfo : TogetherInfo
    {
        public XjcTogetherInfo(int xjcTogetherInfoCount) : base(xjcTogetherInfoCount) { }
        /// <summary>
        /// 人工允推
        /// </summary>
        public bool AllowPush
        {
            get { return (bool)GetValue(AllowPushProperty); }
            set { SetValue(AllowPushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllowPush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowPushProperty =
            DependencyProperty.Register("AllowPush", typeof(bool), typeof(XjcTogetherInfo), new PropertyMetadata(false));

        /// <summary>
        /// 焦罐准备完毕：车门关闭或焦罐旋转
        /// </summary>


        public bool CanReady
        {
            get { return (bool)GetValue(CanReadyProperty); }
            set { SetValue(CanReadyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanReady.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanReadyProperty =
            DependencyProperty.Register("CanReady", typeof(bool), typeof(XjcTogetherInfo), new PropertyMetadata(false));

        /// <summary>
        /// 罐号：靠近车头的干熄焦焦罐为1#罐（false),另一罐为2#罐（true）
        /// </summary>
        public bool CanNum
        {
            get { return (bool)GetValue(CanNumProperty); }
            set { SetValue(CanNumProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CanNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanNumProperty =
            DependencyProperty.Register("CanNum", typeof(bool), typeof(XjcTogetherInfo), new PropertyMetadata(false));
        /// <summary>
        /// 干熄：0，水熄：1
        /// </summary>
        public bool Dry
        {
            get { return (bool)GetValue(DryProperty); }
            set { SetValue(DryProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Dry.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DryProperty =
            DependencyProperty.Register("Dry", typeof(bool), typeof(XjcTogetherInfo), new PropertyMetadata(false));
        /// <summary>
        /// 禁止推焦
        /// </summary>
        public bool Ban
        {
            get { return (bool)GetValue(BanProperty); }
            set { SetValue(BanProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Ban.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BanProperty =
            DependencyProperty.Register("Ban", typeof(bool), typeof(XjcTogetherInfo), new PropertyMetadata(false));


        /// <summary>
        /// 1#罐有无
        /// </summary>
        public bool FstCan
        {
            get { return (bool)GetValue(FstCanProperty); }
            set { SetValue(FstCanProperty, value); }
        }
        // Using a DependencyProperty as the backing store for FstCan.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FstCanProperty =
            DependencyProperty.Register("FstCan", typeof(bool), typeof(XjcTogetherInfo), new PropertyMetadata(true));
        /// <summary>
        /// 2#罐有无
        /// </summary>
        public bool SecCan
        {
            get { return (bool)GetValue(SecCanProperty); }
            set { SetValue(SecCanProperty, value); }
        }
        // Using a DependencyProperty as the backing store for SecCan.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecCanProperty =
            DependencyProperty.Register("SecCan", typeof(bool), typeof(XjcTogetherInfo), new PropertyMetadata(false));
        /// <summary>
        /// 给List<bool> propertyValue赋值后调用此方法；
        /// </summary>
        public void DecodeTogetherInfoValue()
        {
            byte lstIndex = 0;
            if (ToDecodeTogetherInfo.Count > 0)
            {
                AllowPush = ToDecodeTogetherInfo[lstIndex++];
                CanReady = ToDecodeTogetherInfo[lstIndex++];
                CanNum = ToDecodeTogetherInfo[lstIndex++];
                Dry = ToDecodeTogetherInfo[lstIndex++];
                Ban = ToDecodeTogetherInfo[lstIndex++];
                FstCan = ToDecodeTogetherInfo[lstIndex];
                SecCan = ToDecodeTogetherInfo[lstIndex];
            }
        }
    }
    /// <summary>
    /// 熄焦车在界面的移动：需要关注是否为干熄及焦罐号、炉号；
    /// </summary>
    class XjcMoveHelper : DependencyObject
    {
        public XjcMoveHelper() { }
        public XjcMoveHelper(int carNum)
        {
            Deviation = carNum == 1 ? (Setting.AreaFlag ? 49 : -98) : (Setting.AreaFlag ? 98 : -49);
        }
        public int RoomNum
        {
            get { return (int)GetValue(RoomNumProperty); }
            set { SetValue(RoomNumProperty, value); }
        }
        // Using a DependencyProperty as the backing store for RoomNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RoomNumProperty =
            DependencyProperty.Register("RoomNum", typeof(int), typeof(XjcMoveHelper), new PropertyMetadata(0));

        public bool Dry
        {
            get { return (bool)GetValue(DryProperty); }
            set { SetValue(DryProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Dry.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DryProperty =
            DependencyProperty.Register("Dry", typeof(bool), typeof(XjcMoveHelper), new PropertyMetadata(false));
        public bool CanNum
        {
            get { return (bool)GetValue(CanNumProperty); }
            set { SetValue(CanNumProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CanNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanNumProperty =
            DependencyProperty.Register("CanNum", typeof(bool), typeof(XjcMoveHelper), new PropertyMetadata(false));
        /// <summary>
        /// 电机车两个焦罐的偏差
        /// 其中靠近车头的干焦罐 为1#焦罐 fstCan
        /// </summary>
        public double Deviation { get; set; }
    }
}
