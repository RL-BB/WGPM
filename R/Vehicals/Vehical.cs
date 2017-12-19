using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WGPM.R.OPCCommunication;
using WGPM.R.RoomInfo;
using System.Threading;
using System.Windows;
using WGPM.R.UI;

namespace WGPM.R.Vehicles
{
    /// <summary>
    /// T,L,X,M
    /// {9,5,5,9} toReverseDataCount;
    /// {11,5,5,10} togetherInfoCount;
    /// 所有车的ProtocolValidDataCount都为13;
    /// </summary>
    class Vehicle : DependencyObject
    {
        public delegate ushort GetArrowsDelegete();
        public GetArrowsDelegete GetArrows { get; set; }
        public DataRead DataRead { get; set; }
        public ushort TogetherInfoCount { get { return togetherInfoCount; } }
        public ushort togetherInfoCount;
        public Dictionary<int, ProtocolAddr> AddrDic { get { return addrDic; } }
        public Dictionary<int, ProtocolAddr> addrDic;
        /// <summary>
        /// 炉区，分1#，2#，3#，4#炉区
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 车号：1#，2#
        /// </summary>
        public ushort CarNum
        {
            get { return (ushort)GetValue(CarNumProperty); }
            set { SetValue(CarNumProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CarNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CarNumProperty =
            DependencyProperty.Register("CarNum", typeof(ushort), typeof(Vehicle), new PropertyMetadata((ushort)0));


        public string UIRoomNum
        {
            get { return (string)GetValue(UIRoomNumProperty); }
            set { SetValue(UIRoomNumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UIRoomNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UIRoomNumProperty =
            DependencyProperty.Register("UIRoomNum", typeof(string), typeof(Vehicle), new PropertyMetadata(null));


        /// <summary>
        /// 炉号
        /// </summary>
        public int RoomNum
        {
            get { return (int)GetValue(RoomNumProperty); }
            set { SetValue(RoomNumProperty, value); }
        }
        // Using a DependencyProperty as the backing store for RoomNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RoomNumProperty =
            DependencyProperty.Register("RoomNum", typeof(int), typeof(Vehicle), new PropertyMetadata(0));
        /// <summary>
        /// 当前显示车炉号
        /// </summary>
        //public ushort DisplayRoomNum
        //{
        //    get
        //    {
        //        if (RoomNum <= 55)
        //        {
        //            return (ushort)(RoomNum + (Setting.AreaFlag ? 1000 : 3000));
        //        }
        //        else
        //        {
        //            return (ushort)((Setting.AreaFlag ? 2000 : 4000) + RoomNum);
        //        }
        //    }
        //}
        /// <summary>
        /// 软件上联锁信息的显示炉号，如1#TJC在2#炉区的56孔
        /// 显示为：1#2056
        /// </summary>
        //public string SoftwareDisplayRoomNum
        //{
        //    get
        //    {
        //        return CarNum + "#" + DisplayRoomNum.ToString("0000");
        //    }
        //}
        /// <summary>
        /// 当前车的箭头显示
        /// </summary>
        public ushort Arrows
        {
            get { return (ushort)GetValue(ArrowsProperty); }
            set { SetValue(ArrowsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Arrows.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ArrowsProperty =
            DependencyProperty.Register("Arrows", typeof(ushort), typeof(Vehicle), new PropertyMetadata((ushort)0));

        public bool IsReady
        {
            get
            {//arrows=Math.Power(2,4)时，为对中信号，见各车GetArrows()方法
                return Arrows == 0 ? true : false;
            }
        }
        /// <summary>
        /// 通过对比当前车和另一辆车所在位置的物理地址和计划炉号中心地址的差值得到：近车为工作车
        /// </summary>
        public bool JobCar { get; set; }
        /// <summary>
        /// 车辆通讯状态，在CarComm.xaml中赋值
        /// </summary>
        public bool[] CommStatus { get; set; }
        /// <summary>
        /// 根据炉号（物理地址最佳）得到T,L,X的工作车
        /// 煤车需要重写
        /// </summary>
        /// <param name="car1">1#车</param>
        /// <param name="car2">2#车</param>
        /// <param name="cr">获得推焦计划</param>
        public Vehicle GetJobCar(Vehicle car1, Vehicle car2, Dictionary<int, int> roomNumDic)
        {
            if (CokeRoom.PushPlan.Count != 0)
            {
                //1#车和推焦计划炉号的间隔
                int s1 = Math.Abs(car1.DataRead.PhysicalAddr - roomNumDic[CokeRoom.PushPlan[0].RoomNum]);
                int s2 = Math.Abs(car2.DataRead.PhysicalAddr - roomNumDic[CokeRoom.PushPlan[0].RoomNum]);
                car1.JobCar = s1 <= s2 ? true : false;
                car2.JobCar = car1.JobCar ? false : true;
            }
            Vehicle car = car1.JobCar ? car1 : car2;
            return car;
        }
        /// <summary>
        /// 获得工作车的物理地址,注意为4个字节
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        public byte[] GetJobCarPhysicalAddrArr()
        {
            //CarInfo car = car1.JobCar ? car1.DataRead.PhysicalAddr : car2.DataRead.PhysicalAddr;
            int addr = DataRead.PhysicalAddr;
            byte[] PA = BitConverter.GetBytes(addr);
            return PA;
        }
        public void GetRoomNum()
        {
            if (AddrDic.ContainsKey(DataRead.MainPhysicalAddr))
            {
                RoomNum = AddrDic[DataRead.MainPhysicalAddr].RoomNum;
            }
        }
        //public void GetCopy(Vehicle car)
        //{
        //    RoomNum = car.RoomNum;
        //    CarNum = car.CarNum;
        //    DataRead = car.DataRead;
        //    JobCar = car.JobCar;
        //    Arrows = car.Arrows;
        //    UIRoomNum = GetUIRoomNum();
        //}
        //public string GetUIRoomNum()
        //{
        //    string strCarNum = (CarNum + (Setting.AreaFlag ? 0 : 2)) + "# ";
        //    int room = RoomNum;
        //    //room = (room <= 55 ? 1000 : 2000) + (Setting.AreaFlag ? 0 : 2000) + room;//20171211 界面显示为"1# 001"不加
        //    return strCarNum + room;
        //}
        #region 得到当前车的箭头指示；不通用，因为每种车的对中要求不一致（已被注释掉，留备用）
        /// <summary>
        /// 得到当前车的箭头指示。对中指示在相应车中作判断，因为对中要求不一致
        /// </summary>
        /// <param name="index">1-8，分别表示8大车（T,L,X,M）</param>
        /// <param name="car">实例车</param>
        /// <param name="cr">提供计划（推焦or装煤）</param>
        /// <returns>当前车的箭头</returns>
        //public short GetArrows(int index, CarInfo car, CokeRoom cr)
        //{
        //    short arrows = 0;
        //    int planRoomNum = GetPlan(index, cr)[0].RoomNum;
        //    int PA = car.DataRead.PhysicalAddr;
        //    int MA = GetRoomNumDic(index, car)[planRoomNum];
        //    if (MA - PA > 5108)
        //    {//左俩箭头
        //        arrows = (short)Math.Pow(2, 0);
        //    }
        //    else if ((MA - PA <= 108) && (MA - PA > 58))
        //    {//左单箭头
        //        arrows = (short)Math.Pow(2, 1);
        //    }
        //    //else if ((MA - PA <= 50) && (MA - PA >= -50))
        //    //{//对中
        //    //    arrows = (short)Math.Pow(2, 2);
        //    //}
        //    else if ((MA - PA < -50) && (MA - PA >= -108))
        //    {//右单箭头
        //        arrows = (short)Math.Pow(2, 2);
        //    }
        //    else if (MA - PA < -108)
        //    {//右俩箭头
        //        arrows = (short)Math.Pow(2, 3);
        //    }
        //    return arrows;
        //}
        ///// <summary>
        ///// 得到推拦煤三车的RoomNumDic
        ///// </summary>
        ///// <param name="index">List<CarInfo> carsInfo的索引</param>
        ///// <param name="car">车的实例</param>
        ///// <returns>炉号字典</returns>
        //private Dictionary<int, int> GetRoomNumDic(int index, CarInfo car)
        //{
        //    Dictionary<int, int> roomNumDic = new Dictionary<int, int>();
        //    if (index < 2)
        //    {
        //        roomNumDic = Addrs.TRoomNumDic;
        //    }
        //    else if (index >= 2 && index < 4)
        //    {
        //        roomNumDic = Addrs.LRoomNumDic;
        //    }
        //    else if (index >= 4 && index < 6)
        //    {
        //        roomNumDic = GetXRoomNumDic(car);
        //    }
        //    else if (index >= 6)
        //    {
        //        roomNumDic = Addrs.MRoomNumDic;
        //    }
        //    return roomNumDic;
        //}
        ///// <summary>
        ///// 得到熄焦车焦罐对应的RoomNumDic
        ///// </summary>
        ///// <param name="car">车号，间接罐号</param>
        ///// <returns>罐号对应的RoomNumDic</returns>
        //private Dictionary<int, int> GetXRoomNumDic(CarInfo car)
        //{
        //    Dictionary<int, int> roomNumDic = new Dictionary<int, int>();
        //    if (car.CarNum == 1)
        //    {//1#熄焦车根据焦罐号选择地址字典;1#罐 false，2#罐 true
        //        roomNumDic = !((XjcTogetherInfo)car.TogetherInfo).CanNum ? Addrs.X1FstCanRoomNumDic : Addrs.X1SecCanRoomNumDic;
        //    }
        //    else
        //    {//2#熄焦车根据焦罐号选择地址字典;1#罐 false，2#罐 true
        //        roomNumDic = !((XjcTogetherInfo)car.TogetherInfo).CanNum ? Addrs.X2FstCanRoomNumDic : Addrs.X2SecCanRoomNumDic;
        //    }
        //    return roomNumDic;
        //}
        ///// <summary>
        ///// 得到当前的计划，0-5即推拦熄为PushPlan,6-7煤车为StokingPlan
        ///// </summary>
        ///// <param name="index"></param>
        ///// <param name="cr"></param>
        ///// <returns></returns>
        //private List<Plan> GetPlan(int index, CokeRoom cr)
        //{
        //    List<Plan> plan = new List<Plan>();

        //    if (index < 6)
        //    {
        //        return plan = cr.PushPlan;
        //    }
        //    else
        //    {
        //        return plan = cr.StokingPlan;
        //    }
        //}
        #endregion

    }
    interface IDisplayRoomNum
    {
        ushort DisplayRoomNum { get; set; }
    }
    interface IVehicalDataCopy
    {
        void GetCopy(Vehicle car);
    }
}