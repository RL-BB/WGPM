using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WGPM.R.OPCCommunication;
using WGPM.R.Parms;
using WGPM.R.RoomInfo;

namespace WGPM.R.Vehicles
{
    class Mc : Vehicle
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
            ssArr.Add(((McDataRead)car.DataRead).SpirilSpeed1);
            ssArr.Add(((McDataRead)car.DataRead).SpirilSpeed1);
            ssArr.Add(((McDataRead)car.DataRead).SpirilSpeed1);
            ssArr.Add(((McDataRead)car.DataRead).SpirilSpeed1);
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
    }
    class McDataRead : DataRead
    {
        public McDataRead()
        {
            DecodeDataReadValue = DecodeDataRead;
        }
        private ushort spirilSpeed1;

        public ushort SpirilSpeed1 { get { return spirilSpeed1; } }
        private ushort spirilSpeed2;

        public ushort SpirilSpeed2 { get { return spirilSpeed2; } }
        private ushort spirilSpeed3;

        public ushort SpirilSpeed3 { get { return spirilSpeed3; } }
        private ushort spirilSpeed4;

        public ushort SpirilSpeed4 { get { return spirilSpeed4; } }
        public void DecodeUncommonDataReadValue(int index)
        {
            spirilSpeed1 = ToDecodeProtocolData[7];
            spirilSpeed2 = ToDecodeProtocolData[8];
            spirilSpeed3 = ToDecodeProtocolData[9];
            spirilSpeed4 = ToDecodeProtocolData[10];
        }

    }
    class McTogetherInfo : TogetherInfo
    {
        public McTogetherInfo(int mcTogetherInfoCount) : base(mcTogetherInfoCount) { }
        private bool stokingTogether;
        /// <summary>
        /// 装煤联锁
        /// </summary>
        public bool StokingTogether { get { return stokingTogether; } }
        private bool pingRequest;
        public bool PingRequest { get { return pingRequest; } }
        private bool sleeveReady;
        /// <summary>
        /// 导套到位
        /// </summary>
        public bool SleeveReady { get { return sleeveReady; } }
        private bool lidOpen;
        /// <summary>
        /// 炉盖打开
        /// </summary>
        public bool LidOpen { get { return lidOpen; } }
        private bool gateSegmentOpen;
        /// <summary>
        /// 闸板开
        /// </summary>
        public bool GateSegentOpen { get { return gateSegmentOpen; } }
        private bool exhaustDuctReady;
        /// <summary>
        /// 除尘就绪
        /// </summary>
        public bool ExhaustDuctReady { get { return exhaustDuctReady; } }
        private bool stokingBegin;
        public bool StokingBegin { get { return stokingBegin; } }
        private bool stokingEnd;
        public bool StokingEnd { get { return stokingEnd; } }
        private bool getLidTogether;
        /// <summary>
        /// 取盖联锁
        /// </summary>
        public bool GetLidTogether { get { return getLidTogether; } }
        private bool vfdRunning;
        /// <summary>
        /// 给煤变频器运行
        /// </summary>
        private bool VFDRuning { get { return vfdRunning; } }
        public void DecodeTogetherInfoValue()
        {
            byte index = 0;
            if (ToDecodeTogetherInfo.Count > 0)
            {
                stokingTogether = ToDecodeTogetherInfo[index++];//装煤联锁
                pingRequest = ToDecodeTogetherInfo[index++];//请求平煤
                sleeveReady = ToDecodeTogetherInfo[index++];//内导套到位
                lidOpen = ToDecodeTogetherInfo[index++];//炉盖打开
                gateSegmentOpen = ToDecodeTogetherInfo[index++];//闸板开
                exhaustDuctReady = ToDecodeTogetherInfo[index++];//除尘就绪
                stokingBegin = ToDecodeTogetherInfo[index++];//装煤开始
                stokingEnd = ToDecodeTogetherInfo[index++];//装煤结束
                getLidTogether = ToDecodeTogetherInfo[index++];//取盖联锁
                vfdRunning = ToDecodeTogetherInfo[index++];//给煤变频器运行
            }
        }
    }
}
