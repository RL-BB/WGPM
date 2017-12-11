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
    class Mc : Vehicle, IDisplayRoomNum,IVehicalDataCopy
    {

        public Mc(ushort carNum)
        {
            base.CarNum = carNum;
            GetArrows = GetMArrows;
            togetherInfoCount = 10;
            DataRead = new McDataRead();
            DataRead.DecodeOtherDataRead = ((McDataRead)DataRead).DecodeUncommonDataReadValue;
            DataRead.TogetherInfo = new McTogetherInfo(TogetherInfoCount);
            DataRead.TogetherInfo.DecodeTogetherInfo = ((McTogetherInfo)DataRead.TogetherInfo).DecodeTogetherInfoValue;
            addrDic = Addrs.MAddrDic;
        }
        public ushort DisplayRoomNum
        {
            get
            {
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
        public Vehicle GetJobCar(Mc car1, Mc car2)
        {
            if (CokeRoom.StokingPlan.Count == 0) return car1;
            int s1 = Math.Abs(((Mc)car1).DataRead.PhysicalAddr - Addrs.PRoomNumDic[CokeRoom.StokingPlan[0].RoomNum]);
            int s2 = Math.Abs(((Mc)car2).DataRead.PhysicalAddr - Addrs.PRoomNumDic[CokeRoom.StokingPlan[0].RoomNum]);
            car1.JobCar = (s1 <= s2) ? true : false;
            car2.JobCar = !car1.JobCar;
            Vehicle car = car1.JobCar ? car1 : car2;
            return car;
        }
        public List<ushort> GetSpirilSpeed(Mc m1, Mc m2)
        {
            List<ushort> ssArr = new List<ushort>();
            //①找到工作煤车
            Vehicle car = m1.JobCar ? m1 : m2;
            ssArr.Add((ushort)((McDataRead)car.DataRead).SpiralSpeed1);
            ssArr.Add((ushort)((McDataRead)car.DataRead).SpiralSpeed1);
            ssArr.Add((ushort)((McDataRead)car.DataRead).SpiralSpeed1);
            ssArr.Add((ushort)((McDataRead)car.DataRead).SpiralSpeed1);
            return ssArr;
        }
        /// <summary>
        /// 得到工作煤车的螺旋转速
        /// </summary>
        /// <returns></returns>
        public List<byte> GetSpirilSpeed(Vehicle car1, Vehicle car2)
        {
            List<byte> ssArr = new List<byte>();
            ////①找到工作煤车
            //CarInfo car = car1.JobCar ? car1 : car2;
            ////②把螺旋转速To为ByteArr
            //byte[] s1 = BitConverter.GetBytes(((McDataRead)car.DataRead).SpirilSpeed1);
            //byte[] s2 = BitConverter.GetBytes(((McDataRead)car.DataRead).SpirilSpeed2);
            //byte[] s3 = BitConverter.GetBytes(((McDataRead)car.DataRead).SpirilSpeed3);
            //byte[] s4 = BitConverter.GetBytes(((McDataRead)car.DataRead).SpirilSpeed4);
            //ssArr.AddRange(s1);
            //ssArr.AddRange(s2);
            //ssArr.AddRange(s3);
            //ssArr.AddRange(s4);
            return ssArr;
        }
        public ushort GetMArrows()
        {
            if (CokeRoom.StokingPlan.Count == 0) return 0;
            //计划炉号的中心地址
            int middle = Addrs.MRoomNumDic[CokeRoom.StokingPlan[0].RoomNum];
            int actual = DataRead.PhysicalAddr;
            //计划炉号的中心地址，二进制从右向左 为低位到高位
            if (middle - actual > StaticParms.ZFstArrow)
            {//左俩箭头:0011
                Arrows = 3;
            }
            else if ((middle - actual <= StaticParms.ZFstArrow) && (middle - actual > StaticParms.ZArrow))
            {//左单箭头:0010
                Arrows = 2;
            }
            else if (Math.Abs(middle - actual) <= StaticParms.ZArrow)
            {//对中:0001
                Arrows = 0;
            }
            else if ((middle - actual < -StaticParms.ZArrow) && (middle - actual >= -StaticParms.ZFstArrow))
            {//右单箭头:0100
                Arrows = 4;
            }
            else if (middle - actual < -StaticParms.ZFstArrow)
            {//右俩箭头:1100
                Arrows = 12;
            }
            return Arrows;
        }

        public void GetCopy(Vehicle car)
        {
            RoomNum = car.RoomNum;
            CarNum = car.CarNum;
            DataRead = car.DataRead;
            JobCar = car.JobCar;
            Arrows = car.Arrows;
            UIRoomNum = (CarNum + (Setting.AreaFlag ? 0 : 2)) + "#" + DisplayRoomNum;
        }
    }
    class McDataRead : DataRead
    {
        public McDataRead()
        {
            DecodeDataReadValue = DecodeDataRead;
        }


        public int SpiralSpeed1
        {
            get { return (int)GetValue(SpiralSpeed1Property); }
            set { SetValue(SpiralSpeed1Property, value); }
        }

        // Using a DependencyProperty as the backing store for SpiralSpeed1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpiralSpeed1Property =
            DependencyProperty.Register("SpiralSpeed1", typeof(int), typeof(McDataRead), new PropertyMetadata(0));
        public int SpiralSpeed2
        {
            get { return (int)GetValue(SpiralSpeed2Property); }
            set { SetValue(SpiralSpeed2Property, value); }
        }

        // Using a DependencyProperty as the backing store for SpiralSpeed2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpiralSpeed2Property =
            DependencyProperty.Register("SpiralSpeed2", typeof(int), typeof(McDataRead), new PropertyMetadata(0));
        public int SpiralSpeed3
        {
            get { return (int)GetValue(SpiralSpeed3Property); }
            set { SetValue(SpiralSpeed3Property, value); }
        }

        // Using a DependencyProperty as the backing store for SpiralSpeed3.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpiralSpeed3Property =
            DependencyProperty.Register("SpiralSpeed3", typeof(int), typeof(McDataRead), new PropertyMetadata(0));


        public int SpiralSpeed4
        {
            get { return (int)GetValue(SpiralSpeed4Property); }
            set { SetValue(SpiralSpeed4Property, value); }
        }

        // Using a DependencyProperty as the backing store for SpiralSpeed4.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpiralSpeed4Property =
            DependencyProperty.Register("SpiralSpeed4", typeof(int), typeof(McDataRead), new PropertyMetadata(0));

        public void DecodeUncommonDataReadValue(int index)
        {
            SpiralSpeed1 = ToDecodeProtocolData[7];
            SpiralSpeed2 = ToDecodeProtocolData[8];
            SpiralSpeed3 = ToDecodeProtocolData[9];
            SpiralSpeed4 = ToDecodeProtocolData[10];
        }

    }
    class McTogetherInfo : TogetherInfo
    {
        public McTogetherInfo(int mcTogetherInfoCount) : base(mcTogetherInfoCount) { }
        /// <summary>
        /// 装煤联锁
        /// </summary>
        public bool StokingTogether
        {
            get { return (bool)GetValue(StokingTogetherProperty); }
            set { SetValue(StokingTogetherProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StokingTogether.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StokingTogetherProperty =
            DependencyProperty.Register("StokingTogether", typeof(bool), typeof(McTogetherInfo), new PropertyMetadata(false));
        public bool PingRequest
        {
            get { return (bool)GetValue(PingRequestProperty); }
            set { SetValue(PingRequestProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PingRequest.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PingRequestProperty =
            DependencyProperty.Register("PingRequest", typeof(bool), typeof(McTogetherInfo), new PropertyMetadata(false));

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
            DependencyProperty.Register("SleeveReady", typeof(bool), typeof(McTogetherInfo), new PropertyMetadata(false));

        /// <summary>
        /// 炉盖打开
        /// </summary>
        public bool LidOpen
        {
            get { return (bool)GetValue(LidOpenProperty); }
            set { SetValue(LidOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LidOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LidOpenProperty =
            DependencyProperty.Register("LidOpen", typeof(bool), typeof(McTogetherInfo), new PropertyMetadata(false));

        /// <summary>
        /// 闸板开
        /// </summary>
        public bool GateSegmentOpen
        {
            get { return (bool)GetValue(GateSegmentOpenProperty); }
            set { SetValue(GateSegmentOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GateSegentOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GateSegmentOpenProperty =
            DependencyProperty.Register("GateSegentOpen", typeof(bool), typeof(McTogetherInfo), new PropertyMetadata(false));
        /// <summary>
        /// 除尘就绪
        /// </summary>
        public bool ExhaustDuctReady
        {
            get { return (bool)GetValue(ExhaustDuctReadyProperty); }
            set { SetValue(ExhaustDuctReadyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExhaustDuctReady.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExhaustDuctReadyProperty =
            DependencyProperty.Register("ExhaustDuctReady", typeof(bool), typeof(McTogetherInfo), new PropertyMetadata(false));


        public bool StokingBegin
        {
            get { return (bool)GetValue(StokingBeginProperty); }
            set { SetValue(StokingBeginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StokingBegin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StokingBeginProperty =
            DependencyProperty.Register("StokingBegin", typeof(bool), typeof(McTogetherInfo), new PropertyMetadata(false));


        public bool StokingEnd
        {
            get { return (bool)GetValue(StokingEndProperty); }
            set { SetValue(StokingEndProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StokingEnd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StokingEndProperty =
            DependencyProperty.Register("StokingEnd", typeof(bool), typeof(McTogetherInfo), new PropertyMetadata(false));


        /// <summary>
        /// 取盖联锁
        /// </summary>
        public bool GetLidTogether
        {
            get { return (bool)GetValue(GetLidTogetherProperty); }
            set { SetValue(GetLidTogetherProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GetLidTogether.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GetLidTogetherProperty =
            DependencyProperty.Register("GetLidTogether", typeof(bool), typeof(McTogetherInfo), new PropertyMetadata(false));

        /// <summary>
        /// 给煤变频器运行
        /// </summary>
        public bool VFDRunning
        {
            get { return (bool)GetValue(VFDRunningProperty); }
            set { SetValue(VFDRunningProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VFDRuning.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VFDRunningProperty =
            DependencyProperty.Register("VFDRuning", typeof(bool), typeof(McTogetherInfo), new PropertyMetadata(false));

        public void DecodeTogetherInfoValue()
        {
            byte index = 0;
            if (ToDecodeTogetherInfo.Count > 0)
            {
                StokingTogether = ToDecodeTogetherInfo[index++];//装煤联锁
                PingRequest = ToDecodeTogetherInfo[index++];//请求平煤
                SleeveReady = ToDecodeTogetherInfo[index++];//内导套到位
                LidOpen = ToDecodeTogetherInfo[index++];//炉盖打开
                GateSegmentOpen = ToDecodeTogetherInfo[index++];//闸板开
                ExhaustDuctReady = ToDecodeTogetherInfo[index++];//除尘就绪
                StokingBegin = ToDecodeTogetherInfo[index++];//装煤开始
                StokingEnd = ToDecodeTogetherInfo[index++];//装煤结束
                GetLidTogether = ToDecodeTogetherInfo[index++];//取盖联锁
                VFDRunning = ToDecodeTogetherInfo[index++];//给煤变频器运行
            }
        }
    }
}
