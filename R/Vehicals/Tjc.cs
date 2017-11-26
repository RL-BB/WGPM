using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using WGPM.R.OPCCommunication;
using WGPM.R.RoomInfo;
using WGPM.R.XML;
using WGPM.R.SqlHelper;
using WGPM.R.LinqHelper;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using WGPM.R.Parms;
using System.Windows.Data;
using System.Globalization;
using WGPM.R.UI;

namespace WGPM.R.Vehicles
{
    public delegate void DealPushPlanDelegate();
    public delegate void DealStokingPlanDelegate();
    /// <summary>
    /// 在本类中实现对接收到的数据的解析，使之能为下传的数据和界面使用的数据所使用
    /// 传入接收到的当前车Info，直接解析为所需要的信息
    /// 如PhysicalAddr，转换为当前车炉号和软件上的地址（界面上当前车的位置）
    /// StokingTime:开始平煤的时间为平煤时间
    /// PushTime：推焦杆接触焦炳并且前进的时候即为推焦时间
    /// </summary>
    class Tjc : Vehicle
    {
        public Tjc(ushort carNum)
        {
            base.CarNum = carNum;
            addrDic = Addrs.TAddrDic;
            PushCurLst = new List<byte>();
            PushPoleLst = new List<byte>();
            PingCurLst = new List<byte>();
            PingPoleLst = new List<byte>();
            GetPRoomDic();
            GetArrows = GetTArrows;
            togetherInfoCount = 12;//联锁信息的个数有11个，对应协议
            DataRead = new TjcDataRead();
            DataRead.TogetherInfo = new TjcTogetherInfo(TogetherInfoCount);
            //以下两行代码的意义或在于：初始化车的实例时的第一次赋值
            DataRead.DecodeOtherDataRead = ((TjcDataRead)DataRead).DecodeUncommonDataReadValue;
            DataRead.TogetherInfo.DecodeTogetherInfo = ((TjcTogetherInfo)DataRead.TogetherInfo).DecodeTogetherInfoValue;
            IniCurTimer();
            StopTime = DateTime.Now;
        }
        public static int RepeatCount;
        public static int RepeatIndex;
        DealPushPlanDelegate DealPushPlan;
        /// <summary>
        /// 平煤数据的记录Helper ToPingInfoDB
        /// </summary>
        public PgInfo pgInfo;
        /// <summary>
        /// 推焦数据的记录Helper ToPushInfoDB
        /// </summary>
        public PsInfo psInfo;
        /// <summary>
        /// 得到推焦车的箭头指示
        /// </summary>
        /// <param name="cr">提供计划（推焦or装煤）</param>
        /// <returns>箭头指示</returns>
        public ushort GetTArrows()
        {
            //工作车的物理地址和计划炉号的中心地址之差的绝对值为5cm
            //1#：1-55,2#：56-110
            //以0060108为例：
            //离开005后进入006之前为55000，进入之后为0060004,物理地址为[0058,0158]
            //MiddleAddr-PhysicalAddr>5108时 为左二箭头：>>○<<[1]
            //（M-P<=5108）&&（M-P>50）时 为左单箭头：>>○<<[2]
            //（M-P>=-50）&&（M-P<=50）时 为对中：>>○<<[3]
            //（M-P<-50）&&（M-P>-108）时 为右单箭头：>>○<<[4]
            //M-P<-4892时 为右双箭头：>>○<<[5]
            if (CokeRoom.PushPlan.Count != 0)
            {
                //计划炉号的中心地址
                int middle = Addrs.TRoomNumDic[CokeRoom.PushPlan[0].RoomNum];
                int actual = DataRead.PhysicalAddr;
                if (middle - actual > StaticParms.TFstArrow)
                {//左俩箭头:0011
                    Arrows = 3;
                }
                else if ((middle - actual <= StaticParms.TFstArrow) && (middle - actual > StaticParms.TArrow))
                {//左单箭头:0010
                    Arrows = 2;
                }
                else if (Math.Abs(middle - actual) <= StaticParms.TArrow)
                {//对中:0001
                    Arrows = 0;
                }
                else if ((middle - actual < -StaticParms.TArrow) && (middle - actual >= -StaticParms.TFstArrow))
                {//右单箭头:0100
                    Arrows = 4;
                }
                else if (middle - actual < -StaticParms.TFstArrow)
                {//右俩箭头:1100
                    Arrows = 12;
                }
            }
            return Arrows;
        }
        private bool pushBeginFlag;
        private bool pushingFlag;
        private bool pushEndFlag;
        private bool pingBegin;
        /// <summary>
        /// 平煤（装煤）工作车
        /// </summary>
        public bool PMJob { get; set; }
        /// <summary>
        /// 推焦车平煤就位：平煤杆已对准计划平煤炉号
        /// </summary>
        public bool MReady { get { return MArrows == 0 ? true : false; } }
        public int MArrows { get; set; }
        private DispatcherTimer curTimer = new DispatcherTimer();
        #region 推焦电流相关

        public byte PushCurMax { get; set; }
        public int PushCurSum { get; set; }
        public byte PushCurAvg { get; set; }
        public List<byte> PushCurLst { get; set; }
        public List<byte> PushPoleLst { get; set; }
        public bool Pushing
        {
            get
            {
                bool pushing = false;
                //推焦开始
                bool begin = ((TjcTogetherInfo)DataRead.TogetherInfo).PushBegin;
                bool forward = ((TjcTogetherInfo)DataRead.TogetherInfo).PushPoleForward;
                bool poleLen = ((TjcDataRead)DataRead).PushPoleLength > 2000 ? true : false;
                //pushing1 = begin && forward ? true : false;
                //pushing2 = poleLen;
                pushing = begin && forward ? true : poleLen;
                return pushing;
            }
        }
        //private bool pushing1;
        //private bool pushing2;
        /// <summary>
        /// 后续设置为
        /// 在开始推焦后几秒内开始计算：最大和平均推焦电流 
        /// </summary>
        public double PushStartTime { get { return 0.5; } }
        public Point PushLenPoint { get; set; }
        public Point PushCurPoint { get; set; }
        public int PushCurCount { get; set; }
        public int ValidPushCurCount { get; set; }
        private void PreparePlotterPushCur()
        {
            PushCurMax = 0;
            PushCurSum = 0;
            PushCurAvg = 0;
            PushCurCount = 0;
            ValidPushCurCount = 0;
            PushCurLst.Clear();
            PushPoleLst.Clear();
            PushCurPoint = new Point();
            PushLenPoint = new Point();
        }
        private void DoPushing()
        {
            if (Pushing)
            {
                PushCurCount++;
                //推焦电流 ((TjcDataRead)DataRead).PushCur;
                //推焦杆长度 ((TjcDataRead)DataRead).PushPoleLength;
                //需要知道电流计数次数，以便计算平均电流？
                byte cur = ((TjcDataRead)DataRead).PushCur >= 255 ? (byte)255 : (byte)((TjcDataRead)DataRead).PushCur;
                //6000为推焦杆长的最大值；（把推焦杆长映射为 （1-255））
                byte len = ((TjcDataRead)DataRead).PushPoleLength * 255 / 6500 > 255 ? (byte)255 : Convert.ToByte(((TjcDataRead)DataRead).PushPoleLength * 255 / 6500);
                PushCurLst.Add(cur);
                PushPoleLst.Add(len);
                PushLenPoint = new Point(PushCurCount, len);
                PushCurPoint = new Point(PushCurCount, cur);
                //开始记录推焦电流和杆长的条件
                //数字1000单位为ms，即1s，数字200为计时器的Interval，单位为ms
                if (PushCurCount > PushStartTime * 1500 / 200 && cur > 0 && ((TjcTogetherInfo)DataRead.TogetherInfo).PushPoleForward)
                {
                    ValidPushCurCount++;
                    PushCurMax = PushCurMax < cur ? cur : PushCurMax;
                    PushCurSum += cur;
                    //计算平均电流值，是否考虑电流值为0的情况？
                    PushCurAvg = (byte)(PushCurSum / ValidPushCurCount);
                }
            }
        }
        #endregion
        #region 平煤电流相关
        public byte PingCurMax { get; set; }
        public int PingCurSum { get; set; }
        public byte PingCurAvg { get; set; }
        public List<byte> PingCurLst { get; set; }
        public List<byte> PingPoleLst { get; set; }
        public bool Pinging
        {
            get
            {
                bool pingBegin = ((TjcTogetherInfo)DataRead.TogetherInfo).PingBegin && !PingStop;
                return pingBegin;
            }
        }
        /// <summary>
        /// 后续设置为
        /// 在开始推焦后几秒内开始计算：最大和平均推焦电流 
        /// </summary>
        public double PingStartTime { get { return 0.5; } }
        public Point PingLenPoint { get; set; }
        public Point PingCurPoint { get; set; }
        public int PingCurCount { get; set; }
        public int ValidPingCurCount { get; set; }
        private DateTime StopTime { get; set; }
        private bool PingStop { get; set; }
        private void DoPinging()
        {
            if (Pinging)
            {
                PingCurCount++;
                byte cur = ((TjcDataRead)DataRead).PingCur >= 255 ? (byte)255 : (byte)((TjcDataRead)DataRead).PingCur;
                byte len = ((TjcDataRead)DataRead).PingPoleLength * 100 / 2000 > 100 ? (byte)100 : Convert.ToByte(((TjcDataRead)DataRead).PingPoleLength * 100 / 2000);
                PingCurLst.Add(cur);
                PingPoleLst.Add(len);
                PingLenPoint = new Point(PingCurCount, len);
                PingCurPoint = new Point(PingCurCount, cur);
                //开始记录推焦电流和杆长的条件
                //数字1000单位为ms，即1s，数字200为计时器的Interval，单位为ms
                if (PingCurCount > PingStartTime * 1000 / 200 && ((TjcDataRead)DataRead).PingPoleLength > 330 || ((TjcDataRead)DataRead).PingPoleLength < 1100)
                {//0907平煤杆值330和1100均为参考值，得来的思路：平煤杆刚进炭化室或者达到最大深度时的电流过大 无意义
                    ValidPingCurCount++;
                    PingCurMax = PingCurMax < cur ? cur : PingCurMax;
                    PingCurSum += cur;
                    //计算平均电流值，是否考虑电流值为0的情况？
                    PingCurAvg = (byte)(PingCurSum / ValidPingCurCount);
                }
            }
        }
        private void PreparePlotterPingCur()
        {
            StopTime = DateTime.Now;
            PingCurMax = 0;
            PingCurSum = 0;
            PingCurSum = 0;
            PingCurCount = 0;
            ValidPingCurCount = 0;
            PingCurLst.Clear();
            PingPoleLst.Clear();
            PingCurPoint = new Point();
            PingLenPoint = new Point();
        }
        #endregion
        private int Group
        {
            get
            {
                return CokeRoom.PushPlan[0].Group;
            }
        }
        public int PingRoomNum
        {
            get
            {
                int num = 0;
                if (DataRead.MainPhysicalAddr >= 11 && DataRead.MainPhysicalAddr <= 65 || (DataRead.MainPhysicalAddr >= 76 && DataRead.MainPhysicalAddr <= 130))
                {
                    num = PRoomDic[DataRead.MainPhysicalAddr];
                }
                return num;
            }
        }
        public int PushPlanIndex { get; set; }
        public int StokingPlanIndex { get; set; }
        public string PushTime { get; set; }
        public string StokingTime { get; set; }
        private Dictionary<int, int> PRoomDic;//平煤时，物理地址对应的炉号
        /// <summary>
        /// 推焦动作时的数据处理
        /// 1删除当前推掉的炉号（如果在计划中）
        /// 2记录联锁数据到DB
        /// </summary>
        public void ProcessingPushInfo()
        {
            //if (!((TjcTogetherInfo)DataRead.TogetherInfo).TRoomDoorOpen && ((TjcDataRead)DataRead).PushPoleLength > 600)
            if (((TjcDataRead)DataRead).PushPoleLength < 600)
            {//炉门已摘或推焦杆长度大于一个值时
                return;
            }
            if (!pushBeginFlag && ((TjcTogetherInfo)DataRead.TogetherInfo).PushPoleForward)
            {//推焦车推焦杆前进为true且第一次进，则pushCondition1赋值为true; "&&!pushCondition1"的意义是：条件一位true时，则不再执行以下语句；
                //即每次的推焦过程中，以下语句只会执行一次；
                pushBeginFlag = true;
                pushingFlag = false;
                pushEndFlag = false;
                PreparePlotterPushCur();//开始记录推焦电流的准备工作
                return;
            }
            if (!pushingFlag && ((TjcTogetherInfo)DataRead.TogetherInfo).PushBegin)
            {//false才进,第一次得到PushBegin为true时，后续不进
             //推焦开始后，开始记录联锁信息
                Dispatcher.BeginInvoke(new Action(PushDataPart1), null);
                int i = PushPlanIndex >= 0 ? PushPlanIndex : 0;
                int r = CokeRoom.PushPlan.Count > 0 ? CokeRoom.PushPlan[i].RoomNum : 1;
                //计划炉号的推焦时间更新：
                CokeRoom.BurnStatus[r].ActualPushTime = Convert.ToDateTime(PushTime);
                //RecToPushInfoDB(PushPlanIndex, PushTime, JobCar);
                pushingFlag = true;
                return;
            }
            bool pushEnd = ((TjcTogetherInfo)DataRead.TogetherInfo).PushEnd;//推焦结束信号到达；
            bool pushPole = ((TjcDataRead)DataRead).PushPoleLength <= 1500;//且推焦杆长度低于1000时才更新计划
            if (!pushEndFlag && pushEnd && pushPole)
            {//条件一、二和PushEnd为true时，才考虑条件三
                Dispatcher.BeginInvoke(new Action(PushDataPart2), null);
                DealPushPlan = JobCar ? new DealPushPlanDelegate(JobCarDealPushPlan) : new DealPushPlanDelegate(NonJobCarDealPushPlan);
                Dispatcher.BeginInvoke(new Action(DealPushPlan), null);
                pushBeginFlag = false;
                pushingFlag = false;
                pushEndFlag = true;
            }
        }
        /// <summary>
        /// 推焦开始信号到达后记录的数据ToPushInfoDB
        /// </summary>
        private void PushDataPart1()
        {
            PushTime = DateTime.Now.ToString("G");
            PushPlanIndex = GetIndexOfPushPlan();
            psInfo = new PsInfo(PushPlanIndex, PushTime, JobCar);
            //记录到数据库：需注意工作车或非工作推焦车的Info
            PushInfoHelper push = new PushInfoHelper(new DbAppDataContext(Setting.ConnectionStr), psInfo, true);
            push.RecToDB();
        }
        /// <summary>
        /// 推焦结束信号到达后记录的数据ToPushInfoDB
        /// </summary>
        private void PushDataPart2()
        {
            if (psInfo == null) PushDataPart1();
            psInfo.MaxCur = PushCurMax;
            psInfo.AvgCur = PushCurAvg;
            psInfo.CurArr = Convert.ToBase64String(PushCurLst.ToArray());
            psInfo.PoleArr = Convert.ToBase64String(PushPoleLst.ToArray());
            psInfo.EndTime = DateTime.Now;
            //记录到数据库：需注意工作车或非工作推焦车的Info
            PushInfoHelper push = new PushInfoHelper(new DbAppDataContext(Setting.ConnectionStr), psInfo, false);
            push.RecToDB();
            psInfo = null;
        }
        public void ProcessingStokingInfo()
        {
            //当PingBegin信号为true时，准备记录平煤的炉号、时间（平煤时间即为装煤时间）
            //根据当前大车的位置得到平煤的炉号？还是根据装煤车的信号（装煤开始、结束）来更新装煤时间？
            //先根据大车的位置-5孔炭化室得到装煤的炉号，20170402：装煤车的支架还未安装；
            //先记录装煤时间到RoomPlanInfo.Config文件中，再更新装煤计划
            if (!((TjcTogetherInfo)DataRead.TogetherInfo).PingBegin && !pingBegin)
            {//平煤未开始，“条件一”不成立则不执行后续的if语句（已经return）；若条件一成立，则执行后续的if语句（因为没有执行return）
                return;
            }
            if (!pingBegin && ((TjcTogetherInfo)DataRead.TogetherInfo).PingBegin)
            {//平煤开始，“条件一”不成立，使“条件一”成立；
                pingBegin = true;
                StokingPlanIndex = GetIndexofStokingPlan();
                StokingTime = DateTime.Now.ToString("G");
                Dispatcher.BeginInvoke(new Action(PingDataPart1), null);
                Dispatcher.BeginInvoke(new Action(UpdateStokingInfoConfig), null);
                return;
            }
            if (((TjcTogetherInfo)DataRead.TogetherInfo).PingEnd && pingBegin)
            {//当平煤结束到达后，才会进入一次，因为进入过一次后不会再进第二次（在一次装煤过程中）
                pingBegin = false;
                int mNum = Math.Abs(Communication.CarsInfo[6].RoomNum - PingRoomNum) < Math.Abs(Communication.CarsInfo[7].RoomNum - PingRoomNum) ? 1 : 2;
                int carLst = CarNum * 10 + mNum;
                Dispatcher.BeginInvoke(new Action(PingDataPart2), null);
            }
        }
        /// <summary>
        /// 推焦开始时PushInfo数据
        /// 已考虑当StokingPlanIndex小于0时对信息的记录
        /// </summary>
        private void PingDataPart1()
        {
            DateTime invalidTime = Convert.ToDateTime("2012-04-03 13:14");
            bool valid = StokingPlanIndex >= 0;
            MStokingPlan m = valid ? CokeRoom.StokingPlan[StokingPlanIndex] : (CokeRoom.StokingPlan.Count > 0 ? new MStokingPlan(CokeRoom.StokingPlan[0]) : new MStokingPlan(1, Convert.ToDateTime("2012-04-03 13:14"), 1, 1));
            pgInfo = new PgInfo();
            pgInfo.Room = m.RoomNum;//20171122应为计划装煤炉号;
            pgInfo.PlanPushTime = m.StokingTime.AddMinutes(-5);
            pgInfo.PlanPingTime = m.StokingTime;
            pgInfo.TRoom = (byte)RoomNum;
            pgInfo.TPRoom = (byte)PingRoomNum;
            pgInfo.MRoom = (byte)Communication.MJob.RoomNum;
            pgInfo.TAddr = DataRead.PhysicalAddr;
            //pInfo.mAddr = Communication.JobCar[3].DataRead.PhysicalAddr;//20170904 和装煤车的通讯还未调试完毕
            pgInfo.MAddr = Communication.MJob.DataRead.PhysicalAddr;
            pgInfo.BeginTime = Convert.ToDateTime(StokingTime);
            pgInfo.LockInfo = 0;//0906
            pgInfo.Period = valid ? m.Period : (byte)0;
            pgInfo.Group = valid ? m.Group : (byte)0;
            pgInfo.PlanIndex = StokingPlanIndex;
            pgInfo.PlanCount = CokeRoom.StokingPlan.Count;
            PingInfoHelper p = new PingInfoHelper(new DbAppDataContext(Setting.ConnectionStr), pgInfo, true);
            p.RecToDB();
        }
        /// <summary>
        /// 推焦完成后记录StokingInfo的数据
        /// 更新装煤计划
        /// </summary>
        private void PingDataPart2()
        {
            if (pgInfo == null) PingDataPart1();
            if (StokingPlanIndex >= 0)
            {
                if (PMJob)
                {
                    CokeRoom.StokingPlan.RemoveRange(0, StokingPlanIndex + 1);
                }
                else
                {
                    CokeRoom.StokingPlan.RemoveAt(StokingPlanIndex);
                }
            }
            pgInfo.EndTime = DateTime.Now;
            pgInfo.MaxCur = PingCurMax;
            pgInfo.AvgCur = PingCurAvg;
            pgInfo.CurArr = Convert.ToBase64String(PingCurLst.ToArray());
            pgInfo.PoleArr = Convert.ToBase64String(PingPoleLst.ToArray());
            PingInfoHelper p = new PingInfoHelper(new DbAppDataContext(Setting.ConnectionStr), pgInfo, false);
            p.RecToDB();
            pgInfo = null;
        }
        /// <summary>
        /// 得到平煤位置对应的炉号，该方法的赋值过程：由观察TAddr.config文件获得
        /// mainAddr 映射到 平煤炉号
        /// </summary>
        private void GetPRoomDic()
        {
            PRoomDic = new Dictionary<int, int>();
            for (int i = 0; i < 55; i++)
            {
                PRoomDic.Add(11 + i, 1 + i);
                PRoomDic.Add(76 + i, 56 + i);
            }
        }
        /// <summary>
        /// 得到推焦计划炉号的索引值
        /// </summary>
        /// <returns></returns>
        private int GetIndexOfPushPlan()
        {
            int index = -1;
            if (CokeRoom.PushPlan.Count == 0)
            {
                index = -1;
                return index;
            }
            index = JobCar && Math.Abs(CokeRoom.PushPlan[0].RoomNum - RoomNum) <= 1 ? 0 : -1;
            if (index < 0)
            {
                int index1 = CokeRoom.PushPlan.FindIndex(x => x.RoomNum == RoomNum);
                index = index1 >= 0 ? index1 : -1;
            }
            return index;
        }
        /// <summary>
        /// 得到装煤炉号在装煤计划的索引值
        /// 若结果小于0，则不作任何记录
        /// 若结果>=0，则更新到Config中相应炉号的装煤时间，更新装煤计划，记录到LockInfo的[装煤时间]
        /// </summary>
        /// <returns></returns>
        private int GetIndexofStokingPlan()
        {
            int index = -1;
            if (CokeRoom.StokingPlan.Count <= 0) return index;
            index = PMJob && Math.Abs(CokeRoom.StokingPlan[0].RoomNum - PingRoomNum) <= 1 ? 0 : -1;
            //在计划表中 查找装煤炉号
            if (index < 0)
            {//计划表中无当前炉号的计划
                index = CokeRoom.StokingPlan.FindIndex(x => x.RoomNum == PingRoomNum);
            }
            return index;
        }
        /// <summary>
        /// 根据推掉的炉号来更新PushPlan、StokingPlan和Config
        /// </summary>
        private void JobCarDealPushPlan()
        {
            OperateConfig config = new OperateConfig(@"Config\RoomPlanInfo.config");
            if (PushPlanIndex >= 0)
            {
                for (int i = 0; i < PushPlanIndex + 1; i++)
                {
                    TPushPlan p = CokeRoom.PushPlan[i];
                    int room = p.RoomNum;
                    config.XmlNodeList[room - 1].Attributes["PushTime"].Value = PushTime;
                    config.SetPlanAttributeValue(RoomNum, "Valid", "0");
                }
                CokeRoom.PushPlan.RemoveRange(0, PushPlanIndex + 1);
            }
            else
            {
                int room = RoomNum;
                config.XmlNodeList[room - 1].Attributes["PushTime"].Value = PushTime;
                config.SetPlanAttributeValue(RoomNum, "Valid", "0");
            }
            config.Save();
        }
        private void NonJobCarDealPushPlan()
        {
            OperateConfig config = new OperateConfig(@"Config\RoomPlanInfo.config");
            if (PushPlanIndex >= 0)
            {
                TPushPlan p = CokeRoom.PushPlan[PushPlanIndex];
                int room = p.RoomNum;
                config.XmlNodeList[room - 1].Attributes["PushTime"].Value = PushTime;
                config.SetPlanAttributeValue(RoomNum, "Valid", "0");
                CokeRoom.PushPlan.RemoveAt(PushPlanIndex);
            }
            else
            {
                int room = RoomNum;
                config.XmlNodeList[room - 1].Attributes["PushTime"].Value = PushTime;
                config.SetPlanAttributeValue(RoomNum, "Valid", "0");
            }
            config.Save();
        }
        /// <summary>
        /// 根据对装煤计划索引index值 是否能定位到 来决定是否更新 ：装煤计划，Config文件，DB
        /// </summary>
        private void UpdateStokingInfoConfig()
        {//如果对炉号的判断不准确，则不更新平煤时间和电流信息到：Config，StokingPlan，DB
            int pRoom = StokingPlanIndex >= 0 ? CokeRoom.StokingPlan[StokingPlanIndex].RoomNum : PingRoomNum;
            try
            {
                //更新装煤时间到Config
                OperateConfig config = new OperateConfig(@"Config\RoomPlanInfo.config");
                config.XmlNodeList[pRoom - 1].Attributes["StokingTime"].Value = StokingTime;
                config.Save();
                CokeRoom.BurnStatus[pRoom].StokingTime = Convert.ToDateTime(StokingTime);
            }
            catch (Exception e)
            {
                Logger.Log.LogErr.Info("类：Tjc 方法：UpdateStokingInfoConfig；" + e.ToString());
            }

        }
        private void IniCurTimer()
        {
            curTimer.Tick += CurTimer_Tick;
            curTimer.Interval = TimeSpan.FromMilliseconds(200);
            curTimer.Start();
        }
        private void CurTimer_Tick(object sender, EventArgs e)
        {
            DoPushing();
            IsPingStop();
            DoPinging();
        }
        private void IsPingStop()
        {
            if (((TjcTogetherInfo)DataRead.TogetherInfo).PingEnd)
            {
                PingStop = true;
                PreparePlotterPingCur();
            }
            else
            {
                DateTime now = DateTime.Now;
                if (((TjcDataRead)DataRead).PingPoleLength != 0)
                {
                    StopTime = now;
                    PingStop = false;
                }
                else
                {//平煤杆长度为0时间持续5s，即判断为平煤结束
                    PingStop = (now - StopTime).TotalSeconds >= 5 ? true : false;
                }
            }
        }
        /// <summary>
        /// 工作车且PlanIndex！=0且重复次数repeatCount>2时才执行计划的自动更新
        /// 
        /// </summary>
        private void GetRepeatIndexCount()
        {
            if (!JobCar || CokeRoom.PushPlan.Count == 0) return;
            if (PushPlanIndex <= 0)
            {
                RepeatCount = 0;
                RepeatIndex = 0;
            }
            else
            {
                RepeatCount++;
                RepeatIndex = PushPlanIndex;
            }
        }
        public void GetMArrows()
        {
            if (CokeRoom.StokingPlan.Count == 0)
            {
                MArrows = 3;
                return;
            }
            //计划炉号的中心地址
            int middle = Addrs.PRoomNumDic[CokeRoom.StokingPlan[0].RoomNum];
            int actual = DataRead.PhysicalAddr;
            if (middle - actual > StaticParms.TFstArrow)
            {//左俩箭头:0011
                MArrows = 3;
            }
            else if ((middle - actual <= StaticParms.TFstArrow) && (middle - actual > StaticParms.TArrow))
            {//左单箭头:0010
                MArrows = 2;
            }
            else if (Math.Abs(middle - actual) <= StaticParms.TArrow)
            {//对中:0001
                MArrows = 0;
            }
            else if ((middle - actual < -StaticParms.TArrow) && (middle - actual >= -StaticParms.TFstArrow))
            {//右单箭头:0100
                MArrows = 4;
            }
            else if (middle - actual < -StaticParms.TFstArrow)
            {//右俩箭头:1100
                MArrows = 12;
            }
        }
        /// <summary>
        /// 得到平煤（装煤）的工作推焦车车
        /// </summary>
        /// <param name="car1">1#\3#推焦车</param>
        /// <param name="car2">2#\4#推焦车</param>
        public void GetPMJobCar(Tjc car1, Tjc car2)
        {
            if (CokeRoom.StokingPlan.Count == 0)
            {
                car1.PMJob = true;
                car2.PMJob = false;
                return;
            }
            int s1 = Math.Abs(car1.DataRead.PhysicalAddr - Addrs.PRoomNumDic[CokeRoom.StokingPlan[0].RoomNum]);
            int s2 = Math.Abs(car2.DataRead.PhysicalAddr - Addrs.PRoomNumDic[CokeRoom.StokingPlan[0].RoomNum]);
            car1.PMJob = (s1 <= s2) ? true : false;
            car2.PMJob = !car1.PMJob;
        }
    }
}
class TjcDataRead : DataRead
{
    public TjcDataRead()
    {
        DecodeDataReadValue = DecodeDataRead;
    }
    /// <summary>
    /// 推焦电流：起始位置23，length=2，6
    /// </summary>
    public ushort PushCur { get { return pushCur; } }
    private ushort pushCur;
    /// <summary>
    /// 平煤电流，Ping==平煤：起始位置25，length=2，7
    /// </summary>
    public ushort PingCur { get { return pingCur; } }
    private ushort pingCur;
    /// <summary>
    /// 推焦杆（Pole）长度：起始位置27，length=2，8
    /// </summary>
    public int PushPoleLength
    {
        get { return (int)GetValue(PushPoleLengthProperty); }
        set { SetValue(PushPoleLengthProperty, value); }
    }

    // Using a DependencyProperty as the backing store for PushPoleLength.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PushPoleLengthProperty =
        DependencyProperty.Register("PushPoleLength", typeof(int), typeof(TjcDataRead), new PropertyMetadata(0));

    /// <summary>
    /// 平煤(Ping)杆(Pole)长度：起始位置29，length=2，9
    /// </summary>
    public int PingPoleLength
    {
        get { return (int)GetValue(PingPoleLengthProperty); }
        set { SetValue(PingPoleLengthProperty, value); }
    }

    // Using a DependencyProperty as the backing store for PingPoleLength.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PingPoleLengthProperty =
        DependencyProperty.Register("PingPoleLength", typeof(int), typeof(TjcDataRead), new PropertyMetadata(0));

    /// <summary>
    ///解析除8辆车共有的DataRead信息之外的其他信息：推焦电流，平煤电流，推杆长，平杆长
    /// </summary>
    public void DecodeUncommonDataReadValue(int index)
    {
        pushCur = ToDecodeProtocolData[7];
        pingCur = ToDecodeProtocolData[8];
        PushPoleLength = ToDecodeProtocolData[9];
        PingPoleLength = ToDecodeProtocolData[10];
    }
}
class TjcTogetherInfo : TogetherInfo
{
    public TjcTogetherInfo(int tjcTogetherInfoCount) : base(tjcTogetherInfoCount) { }
    /// <summary>
    /// 推焦请求
    /// </summary>
    public bool PushRequest { get { return pushRequest; } }
    private bool pushRequest;
    /// <summary>
    /// 炉门已摘
    /// </summary>
    public bool TRoomDoorOpen { get { return roomDoorOpen; } }
    private bool roomDoorOpen;
    /// <summary>
    /// 推焦联锁
    /// </summary>
    public bool PushTogether { get { return pushTogether; } }
    private bool pushTogether;
    /// <summary>
    /// 摘门联锁
    /// </summary>
    public bool PickDoorTogether { get { return pickDoorTogether; } }
    private bool pickDoorTogether;
    /// <summary>
    /// 推焦开始
    /// </summary>
    public bool PushBegin { get { return pushBegin; } }
    private bool pushBegin;
    /// <summary>
    /// 推焦结束
    /// </summary>
    public bool PushEnd { get { return pushEnd; } }
    private bool pushEnd;
    /// <summary>
    /// 平煤开始
    /// </summary>
    public bool PingBegin { get { return pingBegin; } }
    private bool pingBegin;
    /// <summary>
    /// 平煤结束
    /// </summary>
    public bool PingEnd { get { return pingEnd; } }
    private bool pingEnd;
    /// <summary>
    /// 推焦杆前进(无用)
    /// </summary>
    public bool PushPoleForward { get { return pushPoleForward; } }
    private bool pushPoleForward;
    /// <summary>
    /// 推焦杆 炉前暂停
    /// </summary>
    public bool PushPolePause
    {
        get { return (bool)GetValue(PushPolePauseProperty); }
        set { SetValue(PushPolePauseProperty, value); }
    }

    // Using a DependencyProperty as the backing store for PushPolePause.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PushPolePauseProperty =
        DependencyProperty.Register("PushPolePause", typeof(bool), typeof(TjcTogetherInfo), new PropertyMetadata(false));
    /// <summary>
    /// 推焦完成标志(无用)
    /// </summary>
    public bool PushEndFlag { get { return pushEndFlag; } }
    private bool pushEndFlag;
    public bool TMDoorOpen { get { return tMDoorOpen; } }
    private bool tMDoorOpen;
    public void DecodeTogetherInfoValue()
    {
        byte index = 0;
        if (ToDecodeTogetherInfo.Count > 0)
        {
            pushRequest = ToDecodeTogetherInfo[index++];
            roomDoorOpen = ToDecodeTogetherInfo[index++];
            pushTogether = ToDecodeTogetherInfo[index++];
            pickDoorTogether = ToDecodeTogetherInfo[index++];
            pushBegin = ToDecodeTogetherInfo[index++];
            pushEnd = ToDecodeTogetherInfo[index++];
            pingBegin = ToDecodeTogetherInfo[index++];
            pingEnd = ToDecodeTogetherInfo[index++];
            pushPoleForward = ToDecodeTogetherInfo[index++];
            PushPolePause = ToDecodeTogetherInfo[index++];
            pushEndFlag = ToDecodeTogetherInfo[index++];
            tMDoorOpen = ToDecodeTogetherInfo[index];
        }
    }
}

