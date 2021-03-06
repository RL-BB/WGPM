﻿using System;
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
    class Xjc : Vehicle, IDisplayRoomNum, IVehicalDataCopy
    {
        public Xjc(ushort carNum)
        {
            CarNum = carNum;
            GetArrows = GetXArrows;//给委托赋值
            //熄焦车物理地址对应炉号的字典有四个
            togetherInfoCount = 7;
            //DataRead.DecodeOtherDataRead = ((XjcDataRead)DataRead).DecodeDataReadValue;
            DataRead = new XjcDataRead();
            DataRead.TogetherInfo = new XjcTogetherInfo(TogetherInfoCount);
            DataRead.TogetherInfo.DecodeTogetherInfo = ((XjcTogetherInfo)DataRead.TogetherInfo).DecodeTogetherInfoValue;
        }
        public Dictionary<int, ProtocolAddr> XAddrDic
        {
            get
            {
                //20171103 不能使用单纯使用车号而应根据是否为水熄焦、干熄焦、焦罐号等联锁点来选择物理地址对应炉号的字典
                //20171104 xjc的联锁信息中 可靠的是罐号  在用罐号；水熄干熄这个点不可靠
                XjcTogetherInfo info = (XjcTogetherInfo)DataRead.TogetherInfo;
                //addrDic = Setting.AreaFlag ? (CarNum==1?(info.CanNum ? Addrs.X1SecCanAddrDic : Addrs.XFstCanAddrDic) :(info.Dry ? Addrs.XFstCanAddrDic : Addrs.X2SecCanAddrDic))
                //    : (CarNum==1?(info.Dry ? Addrs.XFstCanAddrDic : Addrs.X2SecCanAddrDic) :(info.CanNum ? Addrs.X1SecCanAddrDic : Addrs.XFstCanAddrDic));
                //20171004 靠近熄焦车的第一个干熄焦焦罐为1#焦罐（水熄焦焦罐为2#焦罐）：这种定义的用途->用来选择物理地址对应炉号的地址字典；
                //20171104  1、2#炉区的电机车  CanNum=0（false），对应1#焦罐；3、4#炉区的CanNum=1（true）对应1#焦罐；3、4#炉区的司机师傅认为靠近车头的为2#焦罐，这一点和软件上对罐号的认定恰好相反
                bool area = Setting.AreaFlag;
                return CarNum == 1 ? (area ? (info.CanNum ? Addrs.X1SecCanAddrDic : Addrs.XFstCanAddrDic) : (info.Dry ? Addrs.XFstCanAddrDic : Addrs.X1SecCanAddrDic))
                : (area ? (info.Dry ? Addrs.XFstCanAddrDic : Addrs.X2SecCanAddrDic) : (info.CanNum ? Addrs.XFstCanAddrDic : Addrs.X2SecCanAddrDic));
            }
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
            if (XAddrDic.ContainsKey(DataRead.MainPhysicalAddr))
            {
                RoomNum = XAddrDic[DataRead.MainPhysicalAddr].RoomNum;
            }
        }
        public XjcMoveHelper MoveHelper { get; set; }

        public ushort DisplayRoomNum
        {
            get
            {
                //20171211 定义1#焦罐显示为801；2#焦罐显示为802
                //此处200的意义，考虑到之后可能会增加虚拟炉号，虚拟炉号应该不会超过200；
                if (RoomNum > 200) return (ushort)RoomNum;
                if (RoomNum <= 55)
                {
                    return (ushort)(RoomNum + (Setting.AreaFlag ? 1000 : 3000));
                }
                else
                {
                    return (ushort)((Setting.AreaFlag ? 2000 : 4000) + RoomNum);
                }
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ushort GetXArrows()
        {
            int actual = DataRead.PhysicalAddr;
            #region 电机车焦罐对中
            if (Setting.AreaFlag && actual > 1845000 && CarNum == 1)
            {
                Arrows = (actual >= 1880000 && actual <= 1880216) || (actual >= 1890000 && actual <= 1890216) ? (ushort)0 : (ushort)3;
                return Arrows;
            }
            if (!Setting.AreaFlag && actual > 1845000 && CarNum == 2)
            {
                Arrows = (actual >= 1860000 && actual <= 1860216) || (actual >= 1870000 && actual <= 1870216) ? (ushort)0 : (ushort)3;
                return Arrows;
            }
            #endregion
            #region 码牌炉号对中
            if (CokeRoom.PushPlan.Count == 0) return Arrows = 0;
            int planRoomNum = CokeRoom.PushPlan[0].RoomNum;
            int middle = RoomNumDic[planRoomNum];
            //计划炉号的中心地址，二进制从右向左 为低位到高位 70108-65000
            if (middle - actual >= StaticParms.XLeftDoubleArrow)
            {//左俩箭头:0011  左二：70108-60216=9892；左一：70108 -65000=5108；对中70108-70000=108 70108-70216=-108；右一：75000-70108=4892 右二 80000-70108=9892
                Arrows = 3;
            }
            else if ((middle - actual < StaticParms.XLeftDoubleArrow) && (middle - actual > StaticParms.XZeroArrow))
            {//左单箭头:0010  
                Arrows = 2;
            }
            else if (Math.Abs(middle - actual) <= StaticParms.XZeroArrow)
            {//对中:0000
                Arrows = 0;
            }
            else if ((actual - middle > StaticParms.XZeroArrow) && (actual - middle < StaticParms.XRightDoubleArrow))
            {//右单箭头:0100；20171201 对中范围扩大；20171105 改回;20171106 扩大,又恢复；
             //Arrows = 4;
                Arrows = 4;
            }
            else if (actual - middle >= StaticParms.XRightDoubleArrow)
            {//右俩箭头:1100
                Arrows = 12;
            }
            return Arrows;
            #endregion
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
            MoveHelper.CanNum = ((XjcTogetherInfo)DataRead.TogetherInfo).CanNum;
        }

        public void GetCopy(Vehicle car)
        {
            RoomNum = car.RoomNum;
            CarNum = car.CarNum;
            DataRead = car.DataRead;
            JobCar = car.JobCar;
            Arrows = car.Arrows;
            UIRoomNum = (CarNum + (Setting.AreaFlag ? 0 : 2)) + "#" + DisplayRoomNum.ToString("0000");
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
            CarNum = carNum;
            Deviation = carNum == 1 ? (Setting.AreaFlag ? 49 : -98) : (Setting.AreaFlag ? 98 : -49);
        }
        public int CarNum { get; set; }

        public int RoomNum
        {
            get { return (int)GetValue(RoomNumProperty); }
            set { SetValue(RoomNumProperty, value); }
        }
        // Using a DependencyProperty as the backing store for RoomNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RoomNumProperty =
            DependencyProperty.Register("RoomNum", typeof(int), typeof(XjcMoveHelper), new PropertyMetadata(0));

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
        /// <summary>
        /// 得到熄焦车为电机车，且焦罐为2#焦罐时的偏移
        /// </summary>
        /// <returns></returns>
        public bool CanNum2()
        {
            return ((Setting.AreaFlag && CarNum == 1) || (CarNum == 2 && !Setting.AreaFlag)) && CanNum;
        }
    }
}
