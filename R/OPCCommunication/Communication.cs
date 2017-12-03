using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using WGPM.R.RoomInfo;
using WGPM.R.Vehicles;
using WGPM.R.KepServer;
using WGPM.R.XML;
using WGPM.R.OPCCommunication;
using WGPM.R.Logger;
using System.Windows.Data;
using System.Windows;
using WGPM.R.TcpComm;
using WGPM.R.UI;
using WGPM.R.CommHelper;

namespace WGPM.R.OPCCommunication
{
    //public delegate void DecodeDataReadValueDelegate(int index);
    class Communication : DependencyObject, IDisposable
    {
        public Communication()
        {
            //加载错误日志记录帮助,20170924
            Log.LogErr.Info("启动时间：" + DateTime.Now.ToString("g"));
            CommLst = new List<CommExamine>();
            UITime = new SysTime();
            //初始地址字典，对应炉号的各生产时间
            Room = new CokeRoom();
            PushPlan = new TPushPlan();
            StokingPlan = new MStokingPlan();
            //开启TCP/IP通讯
            CommHelper = new SocketHelper();
            CommHelper.StartListening();
            OPCInfo = new OPC();
            //初始化8辆车
            InitCars();
            IniTogetherInfo();
            //得到工作车的联锁信息
            GetCarTogetherInfo(JobCarTogetherInfo, true);
            GetMTogetherInfo(MJobCarTogetherInfo, true);
            //得到非工作车的联锁信息********
            GetCarTogetherInfo(NonJobCarTogetherInfo, false);
            GetMTogetherInfo(MNonJobCarTogetherInfo, false);
            DataWrite = new DataWrite();
            CommStatus = new List<OPCCommunication.CommStatus>();
            commTimer.Tick += CommTimer_Tick;
            commTimer.Interval = TimeSpan.FromMilliseconds(200);
            commTimer.Start();
            //测试用方法 正式编译前应注释掉 20171021
            //_BindingTest();

        }
        /// <summary>
        /// 通讯类.Tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommTimer_Tick(object sender, EventArgs e)
        {
            //系统时间
            UITime.DateTime = DateTime.Now;
            GetCurrentPlan();
            GetVehicalInfo();
            GetRoomDoorStatus();
            WrtOpc();
            UpdatePlan();
            //测试用方法 正式编译前应注释掉 20171021
            //_BindingTest();
        }
        /// <summary>
        /// 用于系统时间的显示
        /// </summary>
        public static SysTime UITime { get; set; }
        private SocketHelper CommHelper { get; set; }
        private DispatcherTimer commTimer = new DispatcherTimer();
        /// <summary>
        /// 各车设备的通讯状态
        /// </summary>
        public static List<CommExamine> CommLst { get; set; }
        private ushort[][] recvDataRead = new ushort[8][];
        /// <summary>
        /// 8辆车和四个地面站的通讯状态
        /// 其中index=0时，为四个地面站无线网桥的通讯状态
        /// index=1-8时，分别为T1，T2，L1，L2，X1，X2，M1，M2
        /// </summary>
        public static List<CommStatus> CommStatus { get; set; }
        public static List<Vehicle> CarsLst { get; set; }
        /// <summary>
        /// 四大工作车
        /// </summary>
        public static List<Vehicle> TJobCarLst { get; set; }
        public static List<Vehicle> NonJobCarLst { get; set; }
        /// <summary>
        /// Count=2,索引0d对应工作推焦车，索引1对应工作装煤车
        /// </summary>
        public static List<Vehicle> MJobCarLst { get; set; }
        public static List<Vehicle> MNonJobCarLst { get; set; }
        /// <summary>
        /// 用于绑定到MainTogether界面的推焦信息：炉号，时间
        /// </summary>
        public static TPushPlan PushPlan { get; set; }
        /// <summary>
        /// 用于绑定到MainTogether界面的装煤信息：炉号，时间
        /// </summary>
        public static MStokingPlan StokingPlan { get; set; }
        public Tjc T1 { get; set; }
        public Tjc T2 { get; set; }
        public static Tjc TJob { get; set; }
        public static Tjc TNonJob { get; set; }
        public Ljc L1 { get; set; }
        public Ljc L2 { get; set; }
        public static Ljc LJob { get; set; }
        public static Ljc LNonJob { get; set; }
        public Xjc X1 { get; set; }
        public Xjc X2 { get; set; }
        public static Xjc XJob { get; set; }
        public static Xjc XNonJob { get; set; }
        public Mc M1 { get; set; }
        public Mc M2 { get; set; }
        public static Mc MJob { get; set; }
        public static Mc MNonJob { get; set; }
        public static Tjc TMJob { get; set; }
        public static Tjc TMNonJob { get; set; }
        public int TogetherInfoIndex { get { return 11; } }
        public OPC OPCInfo { get; set; }
        public DataWrite DataWrite { get; set; }
        public DwTogetherInfo DwTogehterInfo { get; set; }
        /// <summary>
        /// MainTogether联锁信息 界面颜色显示
        /// </summary>
        public static DwTogetherInfo JobCarTogetherInfo { get; set; }
        /// <summary>
        /// 用于存储非工作推焦车推焦时的信息
        /// </summary>
        public static DwTogetherInfo NonJobCarTogetherInfo { get; set; }
        //public static DwMTogetherInfo DwMTogetherInfo { get; set; }
        public static DwMTogetherInfo MJobCarTogetherInfo { get; set; }
        public static DwMTogetherInfo MNonJobCarTogetherInfo { get; set; }
        private void IniTogetherInfo()
        {
            JobCarTogetherInfo = new DwTogetherInfo();
            NonJobCarTogetherInfo = new DwTogetherInfo();
            MJobCarTogetherInfo = new DwMTogetherInfo();
            MNonJobCarTogetherInfo = new DwMTogetherInfo();
        }
        public CokeRoom Room { get; set; }
        /// <summary>
        /// 初始8辆车，为接收数据做准备
        /// </summary>
        private void InitCars()
        {
            T1 = new Tjc(1);
            T2 = new Tjc(2);
            L1 = new Ljc(1);
            L2 = new Ljc(2);
            X1 = new Xjc(1);
            X2 = new Xjc(2);
            M1 = new Mc(1);
            M2 = new Mc(2);
            InitCarsInfoList();//得到存放CarInfo的List
            IniJobAndNonJobCar();
            //**得到装煤工作车和非工作车
            IniMJobAndNonJobCar();
        }
        private void InitCarsInfoList()
        {
            CarsLst = new List<Vehicle>();
            CarsLst.Add(T1);
            CarsLst.Add(T2);
            CarsLst.Add(L1);
            CarsLst.Add(L2);
            CarsLst.Add(X1);
            CarsLst.Add(X2);
            CarsLst.Add(M1);
            CarsLst.Add(M2);
        }
        private bool GetDataRead(ushort[][] dataRead)
        {
            bool hasData = false;
            //每辆车的DataRead是一个byte数组，8辆车的是个二维数组
            for (int i = 0; i < dataRead.Length; i++)
            {
                if (dataRead[i] != null)
                {
                    recvDataRead[i] = dataRead[i];
                    //FstRecvData = i == (dataRead.Length - 1) ? true : false;
                    hasData = true;
                }
            }
            return hasData;
        }
        /// <summary>
        /// 解析所有车的DataRead(包括联锁信息)
        /// </summary>
        /// <param name="car"></param>
        /// <param name="dataRead"></param>
        /// <param name="index"></param>
        private void DecodeCarsDataRead()
        {
            for (int index = 0; index < 8; index++)
            {//R20170326至今有5辆车有通讯,R20170629 改至6辆车通讯，问题：index<5时，水熄焦车有地址？
                //R20170906 改为8辆车的通讯
                if (recvDataRead[index] != null)
                {
                    //接收到的DataRead数据赋值给每辆车的DataRead的接收数组
                    CarsLst[index].DataRead.ToDecodeProtocolData = recvDataRead[index];
                    //解析出包括TogetherInfo和其他公共DataRead(①物理地址;②读码器灯状态;③解码器计数;④PLC计数;⑤触摸屏计数),赋值给字段，之后就可以使用属性了
                    CarsLst[index].DataRead.DecodeDataReadValue(index);
                    //解析TogetherInfo：由ushort类型的一个数据得到List<bool> ，然后调用DecodeTogetherInfo();
                    CarsLst[index].DataRead.TogetherInfo.ConvertToBoolList();
                    CarsLst[index].DataRead.TogetherInfo.DecodeTogetherInfo();//可以访问联锁数据了                
                    CarsLst[index].CarNum = Convert.ToUInt16(index % 2 + 1);// 车号还是
                    if (index == 0 || index == 1 || index == 6 || index == 7)
                    {//解析推焦车和装煤车特有的数据
                        CarsLst[index].DataRead.DecodeOtherDataRead(index);
                    }
                }
            }
        }
        /// <summary>
        /// 获得各车炉号(和显示炉号)
        /// </summary>
        public void GetAllCarRoomNum()
        {
            for (int i = 0; i < 8; i++)
            {
                if (CarsLst[i].DataRead.ToDecodeProtocolData == null || CarsLst[i].DataRead.MainPhysicalAddr == 0 || CarsLst[i].DataRead.MainPhysicalAddr == 666)
                {//该车的接收数据为空时，或物理地址不来时，不处理炉号
                    continue;
                }
                if (i == 4 || i == 5)
                {
                    ((Xjc)CarsLst[i]).GetRoomNum();
                }
                else
                {
                    //if (i >= 6) continue;
                    CarsLst[i].GetRoomNum();
                }
            }
        }
        private void IniJobAndNonJobCar()
        {
            TJob = new Tjc(1);
            LJob = new Ljc(1);
            XJob = new Xjc(1);
            TJobCarLst = new List<Vehicle>();
            TJobCarLst.AddRange(new Vehicle[] { TJob, LJob, XJob });
            TNonJob = new Vehicles.Tjc(2);
            LNonJob = new Ljc(2);
            XNonJob = new Xjc(2);
            NonJobCarLst = new List<Vehicle>();
            NonJobCarLst.AddRange(new Vehicle[] { TNonJob, LNonJob, XNonJob });
        }
        private void IniMJobAndNonJobCar()
        {
            TMJob = new Tjc(1);
            MJob = new Mc(1);
            MJobCarLst = new List<Vehicle>();
            MJobCarLst.AddRange(new Vehicle[] { TMJob, MJob });

            TMNonJob = new Tjc(2);
            MNonJob = new Mc(2);
            MNonJobCarLst = new List<Vehicle>();
            MNonJobCarLst.AddRange(new Vehicle[] { TMNonJob, MNonJob });
        }
        /// <summary>
        /// 得到四大车的工作车；(需要在一个计时器中)
        /// </summary>
        public void GetAllJobAndNonJobCar()
        {
            TJob.GetCopy((Tjc)T1.GetJobCar(T1, T2, Addrs.TRoomNumDic));
            LJob.GetCopy((Ljc)L1.GetJobCar(L1, L2, Addrs.LRoomNumDic));
            XJob.GetCopy((Xjc)X1.GetJobCar(X1, X2));
            //20171124 得到平煤的工作推焦车
            TNonJob.GetCopy(TJob.CarNum == 1 ? T2 : T1);
            LNonJob.GetCopy(LJob.CarNum == 1 ? L2 : L1);
            XNonJob.GetCopy(XJob.CarNum == 1 ? X2 : X1);
        }
        public void GetMJobAndNonJobCar()
        {
            if (CokeRoom.StokingPlan.Count > 0)
            {
                T1.GetPMJobCar(T1, T2);
                TMJob.GetCopy(T1.PMJob ? T1 : T2);
                TMNonJob.GetCopy(TMJob.CarNum == 1 ? T1 : T2);
                MJob.GetCopy(M1.GetJobCar(M1, M2));
                MNonJob.GetCopy(MJob.CarNum == 1 ? M1 : M2);
            }
            else
            {
                TMJob = T1;
                TMNonJob = T2;
                MJob = M1;
                MNonJob = M2;
            }
        }
        /// <summary>
        /// 得到四大车工作车炉号的ByteArr
        /// 然后使用List<byte>进行拼接
        /// </summary>
        /// <returns></returns>
        private List<ushort> GetAllJobCarRoomNumArr()
        {
            List<ushort> list = new List<ushort>();
            for (int i = 0; i < TJobCarLst.Count + 1; i++)
            {
                list.Add(i < TJobCarLst.Count ? TJobCarLst[i].DisplayRoomNum : MJobCarLst[1].DisplayRoomNum);
            }
            return list;
        }
        /// <summary>
        /// 得到工作车的物理地址
        /// </summary>
        /// <returns>四大工作车的物理地址</returns>
        private List<ushort> GetAllJobCarPhysicalAddr(int index)
        {
            List<ushort> jobCarPhysicalAddr = new List<ushort>();
            //数字4：工作车有四辆
            Vehicle T = index < 6 ? TJobCarLst[0] : MJobCarLst[0];
            for (int i = 0; i < TJobCarLst.Count + 1; i++)
            {
                byte[] jobCarAddr = (i == 0 ? T : (i < TJobCarLst.Count ? TJobCarLst[i] : MJobCarLst[1])).GetJobCarPhysicalAddrArr();
                //byte[] jobCarAddr = i < JobCarLst.Count ? JobCarLst[i].GetJobCarPhysicalAddrArr() : MJobCarLst[1].GetJobCarPhysicalAddrArr();
                ushort addrPart1 = BitConverter.ToUInt16(jobCarAddr, 0);
                ushort addrPart2 = BitConverter.ToUInt16(jobCarAddr, 2);
                jobCarPhysicalAddr.Add(addrPart1);
                jobCarPhysicalAddr.Add(addrPart2);
            }
            return jobCarPhysicalAddr;
        }
        private List<ushort> GetDataWrite(int index)
        {
            List<ushort> dw = new List<ushort>();
            dw.Add(DataWrite.SoftwareCount);//上位机计数[1]
            ushort pingCur = (ushort)((TjcDataRead)(CarsLst[index].JobCar ? MJobCarLst[0] : MNonJobCarLst[0]).DataRead).PingCur;
            pingCur = pingCur > 255 ? (byte)255 : pingCur;
            dw.Add(pingCur);//（工作推焦车）焦杆长度[2],20170920 把焦杆长度替换为平煤电流
            dw.Add(DataWrite.SysTime);//系统时间（时，分）[3]
            dw.Add(DataWrite.SysTimeSec);//系统时间（秒）[4]
            ushort planTime = 0;
            ushort planRoomNum = 0;
            if (index < 6 && CokeRoom.PushPlan.Count > 0)
            {
                planTime = (ushort)(CokeRoom.PushPlan[0].PushTime.Hour * 100 + CokeRoom.PushPlan[0].PushTime.Minute);
                planRoomNum = CokeRoom.PushPlan[0].RoomNum;
            }
            if (index >= 6 && CokeRoom.StokingPlan.Count > 0)
            {
                planTime = (ushort)(CokeRoom.StokingPlan[0].StokingTime.Hour * 100 + CokeRoom.StokingPlan[0].StokingTime.Minute);
                planRoomNum = (ushort)CokeRoom.StokingPlan[0].RoomNum;
            }
            dw.Add(planTime);//计划时间[5]
            dw.Add(planRoomNum);//计划炉号（分装煤和推焦，应有个判断）[6] 已作判断20171027 之前就处理好了
            //displayRoom
            ushort displayRoom = (ushort)(planRoomNum + (Setting.AreaFlag ? 0 : 2000) + (planRoomNum <= 55 ? 1000 : 2000));
            dw.Add(displayRoom);//计划显示炉号[7]
            dw.Add(CarsLst[index].DisplayRoomNum);//当前车炉号[8]
            //四大车工作车显示炉号。
            dw.AddRange(index < 6 ? GetAllJobCarRoomNumArr() : new List<ushort> { MJobCarLst[0].DisplayRoomNum, 0, 0, MJobCarLst[1].DisplayRoomNum });
            dw.AddRange(index <= 1 ? XjcPhysicalAddr() : (index >= 6 ? PlanTime() : new List<ushort> { 0, 0, 0, 0 }));//4个螺旋转速[13,14,15,16];20171113 推焦车-->熄焦车的物理地址；煤车--> 计划时间（推和装煤）
            dw.AddRange(GetAllJobCarPhysicalAddr(index));//四大工作车的物理地址[17,18,19,20];20171027装煤时，工作推焦车和工作装煤车的物理地址未处理
            dw.AddRange(index < 6 ? GetDwTogetherInfo(index) : GetDwMTogetherInfo(index));
            //推拦熄煤箭头[22]；20170924发送到煤车的箭头为 平煤杆到位的推焦车相对于装煤炉号的箭头
            dw.Add(GetJobCarDwArrows(index));
            dw.Add(CarsLst[index].Arrows);//当前车箭头[23]           
            return dw;
        }
        /// <summary>
        /// 1上位机计数；2系统时间；3计划时间；4推焦计划炉号；5当前车显示炉号；6推焦联锁状态
        /// 7焦杆长度/平煤电流（置零即可，无实用意义）；8工作推焦车地址；9工作拦焦车地址；10工作熄焦车地址；11工作装没车地址
        /// 12工作推焦车显示炉号；13工作拦焦车显示炉号；14工作熄焦车显示炉号；15工作装煤车显示炉号；
        /// 16系统时间秒（置零即可，无实用意义）；17、18、19、20：螺旋计数；
        /// 21推拦熄煤箭头指示
        /// 22当前车箭头指示
        /// </summary>
        /// <param name="index">4为3#熄焦车，5为4#熄焦车</param>
        /// <returns></returns>
        private List<int> GetABDataWrite(int index)
        {

            List<int> dw = new List<int>();
            dw.Add(DataWrite.SoftwareCount);//上位机计数[1]
            dw.Add(DataWrite.SysTime);//系统时间（时，分）[2]
            ushort planTime = 0;
            ushort planRoomNum = 0;
            if (CokeRoom.PushPlan.Count > 0)
            {
                planTime = (ushort)(CokeRoom.PushPlan[0].PushTime.Hour * 100 + CokeRoom.PushPlan[0].PushTime.Minute);
                planRoomNum = CokeRoom.PushPlan[0].RoomNum;
            }
            dw.Add(planTime);//计划时间[3]
            dw.Add(planRoomNum);//推焦计划炉号[4]
            dw.Add(CarsLst[index].DisplayRoomNum);//当前车炉号[5]
            dw.Add((CarsLst[index].JobCar ? JobCarTogetherInfo : NonJobCarTogetherInfo).InfoToInt);//推焦联锁状态[6]
            dw.Add(0);//焦杆长度/平煤电流[7]
            dw.Add(TJobCarLst[0].DataRead.PhysicalAddr);//四大工作车的物理地址[8,9,10,11]
            dw.Add(TJobCarLst[1].DataRead.PhysicalAddr);//四大工作车的物理地址[8,9,10,11]
            dw.Add(TJobCarLst[2].DataRead.PhysicalAddr);//四大工作车的物理地址[8,9,10,11]
            dw.Add(MJobCarLst[1].DataRead.PhysicalAddr);//四大工作车的物理地址[8,9,10,11]
            dw.Add(TJobCarLst[0].DisplayRoomNum);//四大工作车的显示炉号[12,13,14,15]
            dw.Add(TJobCarLst[1].DisplayRoomNum);//四大工作车的显示炉号[12,13,14,15]
            dw.Add(TJobCarLst[2].DisplayRoomNum);//四大工作车的显示炉号[12,13,14,15]
            dw.Add(MJobCarLst[1].DisplayRoomNum);//四大工作车的显示炉号[12,13,14,15]
            //[16]->[20]的数据为20171112 新增 ，为服务器使用
            dw.Add(0);//系统时间秒，置零即可 无实际意义[16], 
            dw.Add(0);//螺旋计数，置零即可 无实际意义[17]， 
            dw.Add(0);//螺旋计数，置零即可 无实际意义[18]， 
            dw.Add(0);//螺旋计数，置零即可 无实际意义[19]， 
            dw.Add(0);//螺旋计数，置零即可 无实际意义[20]， 
            dw.Add(GetJobCarDwArrows(index));//推拦熄煤箭头指示[21]
            dw.Add(CarsLst[index].Arrows);//当前车箭头[22]           
            dw.Add(0);//备用，置零即可 无实际意义[23]
            dw.Add(0);//备用，置零即可 无实际意义[24]
            dw.Add(0);//备用，置零即可 无实际意义[25]
            return dw;
        }
        #region DataForServer
        /// <summary>
        /// 服务器用数据：
        /// 推时间，推炉号，装煤时间，装煤炉号，X1物理地址，X2物理地址；
        /// </summary>
        /// <returns></returns>
        private List<ushort> XjcPhysicalAddr()
        {
            List<ushort> s = new List<ushort>();
            s.AddRange(IntToUShortArr(CarsLst[4].DataRead.PhysicalAddr));
            s.AddRange(IntToUShortArr(CarsLst[5].DataRead.PhysicalAddr));
            return s;
        }
        private List<ushort> PlanTime()
        {
            //时间格式为 dd-HH-mm，2017-11-12 13-14--> 121314；
            List<ushort> s = new List<ushort>();
            TPushPlan t = PushPlan;
            string tTime = t.PushTime.Day.ToString("00") + t.PushTime.Hour.ToString("00") + t.PushTime.Minute.ToString("00");
            MStokingPlan m = StokingPlan;
            string mTime = m.StokingTime.Day.ToString("00") + m.StokingTime.Hour.ToString("00") + m.StokingTime.Minute.ToString("00");
            s.AddRange(IntToUShortArr(Convert.ToInt32(tTime)));//推计划时间
            s.AddRange(IntToUShortArr(Convert.ToInt32(mTime)));
            return s;
        }
        #endregion

        /// <summary>
        /// 得到8辆车的箭头指示
        /// </summary>
        public void GetSoftwareArrows()
        {
            for (int i = 0; i < 8; i++)
            {
                CarsLst[i].GetArrows();
                if (i < 2) ((Tjc)CarsLst[i]).GetMArrows();
            }
        }
        public ushort GetJobCarDwArrows(int index)
        {
            ushort arrows = 0;
            Vehicle T = index < 6 ? TJobCarLst[0] : MJobCarLst[0];
            for (int i = 0; i < TJobCarLst.Count + 1; i++)//四辆车
            {
                Vehicle car = i == 0 ? T : (i <= 2 ? TJobCarLst[i] : MJobCarLst[1]);
                arrows += (ushort)(car.Arrows * (ushort)Math.Pow(2, 4 * i));
            }
            return arrows;
        }
        public ushort[] GetDwTogetherInfo(int index)
        {//***未考虑到装煤车的联锁信息
            #region 20170919之前上位机下发联锁信息的处理方式
            ////推焦请求
            //DwTogehterInfo.PushRequest = false;
            ////推焦联锁
            //DwTogehterInfo.PushTogether = CarsInfo[index].JobCar ? JobCarTogetherInfo.PushTogether : false;
            ////推到位
            //DwTogehterInfo.TJobCarReady = CarsInfo[index].JobCar ? JobCarTogetherInfo.TJobCarReady : false;
            ////推炉门已摘
            //DwTogehterInfo.TRoomDoorOpen = CarsInfo[index].JobCar ? JobCarTogetherInfo.TRoomDoorOpen : false;
            ////推焦开始
            //DwTogehterInfo.PushBegin = CarsInfo[index].JobCar ? JobCarTogetherInfo.PushBegin : false;
            ////推焦结束
            //DwTogehterInfo.PushEnd = CarsInfo[index].JobCar ? JobCarTogetherInfo.PushEnd : false;
            ////拦到位
            //DwTogehterInfo.LJobCarReady = CarsInfo[index].JobCar ? JobCarTogetherInfo.LJobCarReady : false;
            ////拦炉门已摘
            //DwTogehterInfo.LRoomDoorOpen = CarsInfo[index].JobCar ? JobCarTogetherInfo.LRoomDoorOpen : false;
            ////焦槽锁闭
            //DwTogehterInfo.TroughLocked = CarsInfo[index].JobCar ? JobCarTogetherInfo.TroughLocked : false;
            ////拦人工允推
            //DwTogehterInfo.LAllowPush = CarsInfo[index].JobCar ? JobCarTogetherInfo.LAllowPush : false;
            ////熄到位
            //DwTogehterInfo.XJobCarReady = CarsInfo[index].JobCar ? JobCarTogetherInfo.XJobCarReady : false;
            ////焦罐旋转/车门关闭
            //DwTogehterInfo.CanReady = CarsInfo[index].JobCar ? JobCarTogetherInfo.CanReady : false;
            ////熄人工允推
            //DwTogehterInfo.XAllowPush = CarsInfo[index].JobCar ? JobCarTogetherInfo.XAllowPush : false;
            ////水熄/干熄
            //DwTogehterInfo.Dry = CarsInfo[index].JobCar ? JobCarTogetherInfo.Dry : false;
            ////焦罐号（0/1）
            //DwTogehterInfo.CanNum = CarsInfo[index].JobCar ? JobCarTogetherInfo.CanNum : false;
            ////1#罐有无
            //DwTogehterInfo.FstCan = true;
            ////1#罐有无
            //DwTogehterInfo.SecCan = true;
            ////一级允推
            //DwTogehterInfo.FstAllow = carsInfo[index].JobCar ? JobCarTogetherInfo.FstAllow : false;
            ////二级允推
            //DwTogehterInfo.SecAllow = carsInfo[index].JobCar ? JobCarTogetherInfo.SecAllow : false;
            ////平煤请求
            //DwTogehterInfo.PingRequest = ((McTogetherInfo)(JobCar[3].DataRead.TogetherInfo)).PingRequest;
            ////当前车对中指示
            //DwTogehterInfo.IsReady = CarsInfo[index].IsReady;
            #endregion

            #region 20170919 考虑到装煤车联锁信息的下发 修改的处理方式
            DwTogehterInfo = CarsLst[index].JobCar ? JobCarTogetherInfo : NonJobCarTogetherInfo;
            DwTogehterInfo.IsReady = CarsLst[index].IsReady;
            #endregion
            return DwTogehterInfo.GetDwUshortArr;
        }
        public ushort[] GetDwMTogetherInfo(int index)
        {
            DwMTogetherInfo info = CarsLst[index].JobCar ? MJobCarTogetherInfo : MNonJobCarTogetherInfo;
            if (info == null) return new ushort[] { 0, 0 };
            info.MReady = CarsLst[index].IsReady;
            return info.GetDwUshortArr;
        }
        /// <summary>
        /// 
        /// </summary>
        public void GetCarTogetherInfo(DwTogetherInfo tInfo, bool job)
        {
            //推焦车
            tInfo.PushRequest = false;//推焦请求
            Vehicle T = job ? TJobCarLst[0] : NonJobCarLst[0];
            tInfo.PushTogether = ((TjcTogetherInfo)(T.DataRead.TogetherInfo)).PushTogether;//推焦联锁
            tInfo.TJobCarReady = T.IsReady;//推到位
            tInfo.TRoomDoorOpen = ((TjcTogetherInfo)(T.DataRead.TogetherInfo)).DoorOpen;//炉门已摘
            tInfo.PushBegin = ((TjcTogetherInfo)(T.DataRead.TogetherInfo)).PushBegin;
            tInfo.PushEnd = ((TjcTogetherInfo)(T.DataRead.TogetherInfo)).PushEnd;
            //拦焦车：拦到位，拦炉门已摘，焦槽锁闭，人工允推
            Vehicle L = job ? TJobCarLst[1] : NonJobCarLst[1];
            tInfo.LJobCarReady = L.IsReady;
            tInfo.LRoomDoorOpen = ((LjcTogetherInfo)(L.DataRead.TogetherInfo)).DoorOpen;
            tInfo.TroughLocked = ((LjcTogetherInfo)(L.DataRead.TogetherInfo)).TroughLocked;
            tInfo.LAllowPush = ((LjcTogetherInfo)(L.DataRead.TogetherInfo)).AllowPush;
            //熄焦车：熄到位，焦罐旋转/车门关闭，人工允推，水熄/干熄，焦罐号，1#罐有无，2#罐有无
            Vehicle X = job ? TJobCarLst[2] : NonJobCarLst[2];
            tInfo.XJobCarReady = X.IsReady;
            tInfo.CanReady = ((XjcTogetherInfo)(X.DataRead.TogetherInfo)).CanReady;
            tInfo.XAllowPush = ((XjcTogetherInfo)(X.DataRead.TogetherInfo)).AllowPush;
            tInfo.Dry = ((XjcTogetherInfo)(X.DataRead.TogetherInfo)).Dry;
            bool carNum = X.CarNum == 1 ? false : true;
            if (tInfo.CarNum != carNum)
            {
                tInfo.CarNum = carNum;
            }
            tInfo.CanNum = ((XjcTogetherInfo)(X.DataRead.TogetherInfo)).CanNum;
            tInfo.TimeAllow = tInfo.IsTimeAllow();
            tInfo.FstCan = ((XjcTogetherInfo)(X.DataRead.TogetherInfo)).FstCan;
            tInfo.SecCan = ((XjcTogetherInfo)(X.DataRead.TogetherInfo)).SecCan;
            //一级允推，二级允推，平煤请求
            tInfo.FstAllow = tInfo.GetFstAllow();
            tInfo.SecAllow = tInfo.GetSecAllow();
        }
        public void GetMTogetherInfo(DwMTogetherInfo info, bool job)
        {
            Vehicle t = job ? MJobCarLst[0] : MNonJobCarLst[0];//推焦车
            Vehicle m = job ? MJobCarLst[1] : MNonJobCarLst[1];//装煤车
            info.Pinging = ((TjcTogetherInfo)t.DataRead.TogetherInfo).PingBegin;//1正在平煤
            info.TReady = ((Tjc)t).MReady;//2推焦车就位
            info.TMDoorOpen = ((TjcTogetherInfo)t.DataRead.TogetherInfo).PMDoorOpen;//3小炉门
            info.LReady = false;//4拦焦车就位
            info.PingRequest = false;//5请求平煤
            info.MReady = m.IsReady;//6煤车就位
            info.AllowGet = false;//9允许取煤
            info.MLock = false;//10装煤联锁，装煤车取自己的点
            info.TDoorClosed = CokeRoom.StokingPlan.Count > 0 ? !CokeRoom.RoomDoorLst[CokeRoom.StokingPlan[0].RoomNum - 1].TDoorOpen : true;//11机侧推焦炉门开
            info.LDoorClosed = CokeRoom.StokingPlan.Count > 0 ? !CokeRoom.RoomDoorLst[CokeRoom.StokingPlan[0].RoomNum - 1].LDoorOpen : true;//12焦侧出焦炉门开
            info.LidOpen = ((McTogetherInfo)m.DataRead.TogetherInfo).LidOpen;//13炉盖
            info.ReadyToPing = info.IsPingReady();//7准备平煤
            info.AllowPing = info.IsAllowPing();//8允许装煤
            info.SleeveReady = ((McTogetherInfo)m.DataRead.TogetherInfo).SleeveReady;
        }
        /// <summary>
        /// 得到装煤车联锁信息用的信息：推炉门已关，推小炉门打开，焦侧炉门已关
        /// </summary>
        private void GetRoomDoorStatus()
        {
            for (int i = 0; i < CarsLst.Count; i++)
            {
                if (i < 2)
                {
                    if (CarsLst[i].RoomNum > 0 && CarsLst[i].RoomNum <= 110)
                    {
                        CokeRoom.RoomDoorLst[CarsLst[i].RoomNum - 1].TDoorOpen = ((TjcTogetherInfo)CarsLst[i].DataRead.TogetherInfo).DoorOpen;
                        CokeRoom.RoomDoorLst[CarsLst[i].RoomNum - 1].TMDoorOpen = ((TjcTogetherInfo)CarsLst[i].DataRead.TogetherInfo).PMDoorOpen;//小炉门
                        continue;
                    }
                }
                if (i < 4 && i >= 2)
                {
                    if (CarsLst[i].RoomNum > 0 && CarsLst[i].RoomNum <= 110)
                    {
                        CokeRoom.RoomDoorLst[CarsLst[i].RoomNum - 1].LDoorOpen = ((LjcTogetherInfo)CarsLst[i].DataRead.TogetherInfo).DoorOpen;
                        continue;
                    }
                }
                if (i < 8 && i >= 6)
                {
                    if (CarsLst[i].RoomNum > 0 && CarsLst[i].RoomNum <= 110)
                    {
                        CokeRoom.RoomDoorLst[CarsLst[i].RoomNum - 1].LidOpen = ((McTogetherInfo)CarsLst[i].DataRead.TogetherInfo).LidOpen;
                        continue;
                    }
                }
            }
        }
        /// <summary>
        /// 解析DataRead
        /// 获得各车数据
        /// 获得工作车
        /// </summary>
        private void GetVehicalInfo()
        {
            //**得到8辆车的DataRead，存储在recvDataRead
            GetDataRead(OPCInfo.KepDataRead);
            //**解析8辆车的DataRead（需要在一个定时器里重复解析，以更新数据）
            DecodeCarsDataRead();
            //**得到8辆车的炉号，显示炉号
            GetAllCarRoomNum();
            //**得到8辆车的箭头指示
            GetSoftwareArrows();
            //**获得各车炉号后，判断并得到四大工作车
            GetAllJobAndNonJobCar();
            //**得到装煤工作车和非工作车
            GetMJobAndNonJobCar();
            //得到工作车的联锁信息
            GetCarTogetherInfo(JobCarTogetherInfo, true);
            GetMTogetherInfo(MJobCarTogetherInfo, true);
            //得到非工作车的联锁信息********
            GetCarTogetherInfo(NonJobCarTogetherInfo, false);
            GetMTogetherInfo(MNonJobCarTogetherInfo, false);
            //20171203 增加UI界面熄焦车移动所需要的XjcMoveHelper信息(因为熄焦车的移动不单和炉号相关，还和罐号相关）
            //仅为UI界面Xjc的移动服务；
            X1.GetMoveHelper();
            X2.GetMoveHelper();
        }
        /// <summary>
        /// 向各车发送数据
        /// </summary>
        private void WrtOpc()
        {
            for (int i = 0; i < 8; i++)
            {
                if (!Setting.AreaFlag && (i == 4 || i == 5))
                {//3、4#炉区对两辆ABPLC的数据写入
                    OPCInfo.ABDataWrite[i - 4] = GetABDataWrite(i).ToArray();
                    continue;
                }
                OPCInfo.KepDataWrite[i] = GetDataWrite(i).ToArray();
            }
        }
        /// <summary>
        /// 更新计划信息
        /// </summary>
        private void UpdatePlan()
        {
            //**更新计划炉号
            T1.ProcessingPushInfo();
            T2.ProcessingPushInfo();
            //**记录装煤时间（更新到RoomPlanInfo中）
            T1.ProcessingStokingInfo();
            T2.ProcessingStokingInfo();
        }
        /// <summary>
        /// 更新用于UI界面的PushPlan和StokingPlan显示信息：炉号，生产时间
        /// </summary>
        private void GetCurrentPlan()
        {
            PushPlan.GetCopyFrom(CokeRoom.PushPlan.Count > 0 ? CokeRoom.PushPlan[0] : new TPushPlan(1, 1, 1, Convert.ToDateTime(DateTime.Now)));
            StokingPlan.GetCopyFrom(CokeRoom.StokingPlan.Count > 0 ? CokeRoom.StokingPlan[0] : new MStokingPlan(PushPlan));
        }
        private ushort[] IntToUShortArr(int intValue)
        {
            byte[] bArr = BitConverter.GetBytes(intValue);
            return new ushort[] { BitConverter.ToUInt16(bArr, 0), BitConverter.ToUInt16(bArr, 2) };
        }
        #region Test
        Random r = new Random();

        /// <summary>
        /// 测试主界面各种信号：车辆移动，推杆、平杆、焦槽的移动，炉号的显示 20171021
        /// </summary>
        private void _BindingTest()
        {
            //炉号
            T1.RoomNum = 106;
            T2.RoomNum = 16;
            L1.RoomNum = 106;
            L2.RoomNum = 16;
            X1.RoomNum = 106;
            X2.RoomNum = 16;
            M1.RoomNum = 106;
            M2.RoomNum = 16;
            //推杆、平杆
            ((TjcDataRead)T1.DataRead).PushPoleLength = 2000;
            ((TjcDataRead)T2.DataRead).PushPoleLength = 2000;
            ((TjcDataRead)T1.DataRead).PingPoleLength = 1000;
            ((TjcDataRead)T2.DataRead).PingPoleLength = 1000;
            //炉前暂停
            ((TjcTogetherInfo)T1.DataRead.TogetherInfo).PushPolePause = true;
            ((TjcTogetherInfo)T2.DataRead.TogetherInfo).PushPolePause = false;
            //焦槽
            ((LjcTogetherInfo)L1.DataRead.TogetherInfo).TroughLocked = false;
            ((LjcTogetherInfo)L2.DataRead.TogetherInfo).TroughLocked = true;
            //熄焦车 熄焦模式,焦罐
            ((XjcTogetherInfo)X1.DataRead.TogetherInfo).Dry = false;
            ((XjcTogetherInfo)X2.DataRead.TogetherInfo).Dry = true;
            ((XjcTogetherInfo)X1.DataRead.TogetherInfo).CanNum = false;
            ((XjcTogetherInfo)X2.DataRead.TogetherInfo).CanNum = true;
            //工作车 箭头
            TJobCarLst[0].Arrows = 0;
            TJobCarLst[1].Arrows = 0;
            TJobCarLst[2].Arrows = 0;
            MJobCarLst[1].Arrows = 0;
            //TLockInfo
            JobCarTogetherInfo.TRoomDoorOpen = Convert.ToBoolean(r.Next(0, 2));
            JobCarTogetherInfo.TroughLocked = Convert.ToBoolean(r.Next(0, 2));
            JobCarTogetherInfo.CanReady = Convert.ToBoolean(r.Next(0, 2));
        }


        #endregion
        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~Communication() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
