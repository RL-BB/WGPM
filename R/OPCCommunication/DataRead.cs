using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows;
using WGPM.R.TcpComm;
/// <summary>
/// 码牌：006
/// 当码牌离开6靠近7时，解码器给出的地址为65000
/// 当码牌离开6靠近5时，解码器给出的地址为55000
/// 存储物理地址对应炉号和软件上车的位置的Config，注意物理地址应为三位的整数
/// </summary>
namespace WGPM.R.OPCCommunication
{
    public delegate void DecodeDataReadDelegate(int index);
    public delegate void DecodeTogetherInfoDelegate();
    class DataRead : DependencyObject
    {
        public DataRead()
        {
            //**(协议中对上位机由有意义的内容)invalidCotent=9**的来源：(以Tjc为例)根据和PLC的协议，其中除联锁信息这一字节外其他有效内容数量=9
            //分别为1物理地址,2读码器灯状态,3解码器计数,4PLC计数,5TouchCount,6推焦电流,7平煤电流,8推杆长,9平杆长
            //BitConverter.ToInt32(decodeProtocalData, index),index为根据和PLC的协议得到的协议数据起始位置
            //在下面各属性Get方法中出现的数字即为protocalDataStartIndex，各属性均由两个字节构成，这一点由和PLC的协议中得到
            //对类中各属性的定义顺序必须与协议中对应的内容一致；顺序很重要，顺序很重要，顺序很重要！！！！
        }
        public ushort[] ToDecodeProtocolData { get; set; }
        //protocalDataStartIndex，由和PLC的协议中得到

        /// <summary>
        /// 12=4+2*4，4是物理地址的字节数，其他四个各占2个字节
        /// </summary>
        public int UncommomValidDataStartIndex { get { return 6; } }
        public TogetherInfo TogetherInfo { get; set; }
        /// <summary>
        /// 物理地址：起始位置13，length=2，1
        /// </summary>
        public int PhysicalAddr
        {
            get { return (int)GetValue(PhysicalAddrProperty); }
            set { SetValue(PhysicalAddrProperty, value); }
        }
        // Using a DependencyProperty as the backing store for PhysicalAddr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PhysicalAddrProperty =
            DependencyProperty.Register("PhysicalAddr", typeof(int), typeof(DataRead), new PropertyMetadata(0));


        /// <summary>
        /// 码牌006，中心地址0060216，非协议DataRead数据
        /// MainPhysicalAddr=006
        /// </summary>
        public int MainPhysicalAddr
        {
            get
            {
                if (PhysicalAddr == 0)
                {
                    return 666;
                }
                else
                    return PhysicalAddr / 10000;
            }
        }
        /// <summary>
        /// 读码器灯状态：起始位置15，length=2，2
        /// </summary>
        public int LightStatus { get { return lightStatus; } }
        private int lightStatus;
        /// <summary>
        /// 解码器计数：起始位置17，length=2，3
        /// </summary>
        public int DecodeCount { get { return decodeCount; } }
        private int decodeCount;
        private int decodeCount1;
        public bool Decode { get; set; }
        /// <summary>
        /// PLC计数：起始位置19，length=2，4
        /// </summary>
        public int PLCCount { get { return plcCount; } }
        private int plcCount;
        private int plcCount1;
        public bool PLC { get; set; }
        private DateTime plcTime;
        /// <summary>
        /// 触摸屏计数：起始位置21，length=2，5
        /// </summary> 
        public int TouchCount { get { return touchCount; } }
        private int touchCount;
        private int touchCount1;
        public bool Touch { get; set; }
        private DateTime touchTime;
        public DecodeDataReadDelegate DecodeOtherDataRead { get; set; }
        public DecodeDataReadDelegate DecodeDataReadValue { get; set; }
        /// <summary>
        /// 公共ValidDataRead(①物理地址;②读码器灯状态;③解码器计数;④PLC计数;⑤触摸屏计数)
        /// 
        /// </summary>
        public void DecodeDataRead(int index)
        {//①联锁信息;②物理地址;③读码器灯状态;④解码器计数;⑤PLC计数;⑥触摸屏计数;除物理地址4个字节外，其他为2个字节
            TogetherInfo.TogetherInfoValue = ToDecodeProtocolData[0];
            PhysicalAddr = DecodePhysicalAddr();//物理地址四个字节
            //lightStatus = ToDecodeProtocolData[2];
            //decodeCount = ToDecodeProtocolData[3];
            //plcCount = ToDecodeProtocolData[4];
            //touchCount = ToDecodeProtocolData[5];
        }
        public void DecodeXjcDataRead(int index)
        {
            TogetherInfo.TogetherInfoValue = ToDecodeProtocolData[0];
            PhysicalAddr = index==4? SocketHelper.Addr1: SocketHelper.Addr2;//物理地址，20170924：TCP/IP -Helper类 接收物理地址传递至此处
            lightStatus = 0;
            decodeCount = 0;
            plcCount = ToDecodeProtocolData[1];
            touchCount = ToDecodeProtocolData[2];
        }
        public int DecodePhysicalAddr()
        {
            List<byte> list = new List<byte>();
            byte[] s1 = BitConverter.GetBytes(ToDecodeProtocolData[1]);
            byte[] s2 = BitConverter.GetBytes(ToDecodeProtocolData[2]);
            list.AddRange(s1);
            list.AddRange(s2);
            int PA = BitConverter.ToInt32(list.ToArray(), 0);
            return PA;
        }
        private bool IsTouchComm()
        {
            bool comm = true;
            if (touchCount != touchCount1)
            {
                touchCount1 = touchCount;
            }
            else
            {
                comm = false;
            }
            return comm;
        }
        private bool IsPLCcomm()
        {
            bool comm = true;
            if (plcCount != plcCount1)
            {
                plcCount1 = plcCount;
            }
            else
            {
                comm = false;
            }
            return comm;
        }
        private bool IsDecodeComm()
        {
            bool comm = true;
            if (decodeCount != decodeCount1)
            {
                decodeCount1 = decodeCount;
            }
            else
            {
                comm = false;
            }
            return comm;
        }
    }
    class TogetherInfo:DependencyObject
    {
        /// <summary>
        /// 联锁信息有效位数(Bit的有效数量）
        /// </summary>
        /// <param name="togetherInfoCount"></param>
        public TogetherInfo(int togetherInfoCount)
        {
            toDecodeTogetherInfo = new List<bool>(togetherInfoCount);
        }
        public ushort TogetherInfoValue { get; set; }
        //用List<bool>的实例Porperties作为字段来对其他属性进行赋值
        public List<bool> ToDecodeTogetherInfo { get { return toDecodeTogetherInfo; } }
        public List<bool> toDecodeTogetherInfo;
        public DecodeTogetherInfoDelegate DecodeTogetherInfo { get; set; }
        /// <summary>
        /// 解析联锁信息数据:把一个ushort型数据ConvertTo一个List<bool>的list
        /// </summary>
        /// <param name="togetherInfo">联锁信息数据</param>
        /// <param name="togetherInfoCount">List<bool>的实例Properties的Count</param>
        /// <returns></returns>
        public void ConvertToBoolList()
        {
            toDecodeTogetherInfo = new List<bool>(toDecodeTogetherInfo.Capacity);
            for (int i = 0; i < toDecodeTogetherInfo.Capacity; i++)
            {
                //toDecodeTogetherInfo[i] = Convert.ToBoolean(TogetherInfoValue & (ushort)Math.Pow(2, i));
                toDecodeTogetherInfo.Add(Convert.ToBoolean(TogetherInfoValue & (ushort)Math.Pow(2, i)));
            }
        }
    }
}
