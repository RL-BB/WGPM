using WGPM.R.XML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using WGPM.R.UI;

namespace WGPM.R.RoomInfo
{
    /// <summary>
    /// 实现的功能：
    /// 由物理地址得到当前车（当前焦罐（1#，2#））所对应的炉号和软件上的地址
    /// </summary>
    class Addrs
    {
        public Addrs()
        {
            #region 补充解释
            //GetProtocolAddr(fileName,DictionaryName)
            //其中fileName是自定义的“地址解析”config文件的Name，物理地址对应炉号和软件上车的地址
            //DictionaryName为存放“(主)物理地址对应炉号和软件上车的地址”的字典
            //从OPC上得到物理地址时，根据该字典得到对应的车的位置（炉号）和软件上车的位置（SoftwareAddr）
            //**以下方法中调用的.Config文件中，每个XmlElement中Attributes的第一个成员是MainPhysicalAddr
            //即实际车的物理地址：PhysicalAddr/10000；
            #endregion
            string area = Setting.AreaFlag ? @"\Xjc_12" : @"\Xjc_34";
            #region 物理地址(MainPart)对应炉号和软件上车的位置
            //获取推焦车侧码牌对应的地址映射：对应的实际炉号和车在软件上移动时需要的Addr；
            GetProtocalAddr(@"Config\MainAddr\TAddr.config", out tAddrDic);
            //同拦焦车
            GetProtocalAddr(@"Config\MainAddr\LAddr.config", out lAddrDic);

            //定义1#熄焦车靠近车头的焦罐为FstCan(FirstCan)，远离车头的焦罐为SecCan(SecondCan)
            GetProtocalAddr(@"Config\MainAddr" + area + @"\XFstCanAddr.config", out xFstCanAddrDic);
            GetProtocalAddr(@"Config\MainAddr" + area + @"\X1SecCanAddr.config", out x1SecCanAddrDic);
            //定义2#熄焦车(X2)的干熄焦焦罐为FstCan
            x2FstCanAddrDic = xFstCanAddrDic;
            GetProtocalAddr(@"Config\MainAddr" + area + @"\X2SecCanAddr.config", out x2SecCanAddrDic);
            //获取装煤车上传的物理地址对应的炉号和
            GetProtocalAddr(@"Config\MainAddr\MAddr.config", out mAddrDic);
            #endregion
            #region 炉号对应的中心地址
            GetMiddleAddr(@"Config\RoomNum\TRoomNum.config", out tRoomNumDic);
            GetMiddleAddr(@"Config\RoomNum\PRoomNum.config", out pRoomNumDic);//推焦车平煤炉号对应的：炉号-中心地址
            GetMiddleAddr(@"Config\RoomNum\LRoomNum.config", out lRoomNumDic);
            GetMiddleAddr(@"Config\RoomNum" + area + @"\XFstCanRoomNum.config", out xFstCanRoomNumDic);
            GetMiddleAddr(@"Config\RoomNum" + area + @"\X1SecCanRoomNum.config", out x1SecCanRoomNumDic);
            GetMiddleAddr(@"Config\RoomNum" + area + @"\X2SecCanRoomNum.config", out x2SecCanRoomNumDic);
            GetMiddleAddr(@"Config\RoomNum\MRoomNum.config", out mRoomNumDic);
            #endregion

        }
        #region 读取Config中MainPhysicalAddr对应的炉号和软件上车的位置
        public static Dictionary<int, ProtocolAddr> TAddrDic { get { return tAddrDic; } }
        private static Dictionary<int, ProtocolAddr> tAddrDic;
        public static Dictionary<int, ProtocolAddr> LAddrDic { get { return lAddrDic; } }
        public static Dictionary<int, ProtocolAddr> lAddrDic;
        public static Dictionary<int, ProtocolAddr> XFstCanAddrDic { get { return xFstCanAddrDic; } }
        public static Dictionary<int, ProtocolAddr> xFstCanAddrDic;
        public static Dictionary<int, ProtocolAddr> X1SecCanAddrDic { get { return x1SecCanAddrDic; } }
        public static Dictionary<int, ProtocolAddr> x1SecCanAddrDic;
        public static Dictionary<int, ProtocolAddr> X2FstCanAddrDic { get { return x2FstCanAddrDic; } }//干熄罐
        public static Dictionary<int, ProtocolAddr> x2FstCanAddrDic;
        public static Dictionary<int, ProtocolAddr> X2SecCanAddrDic { get { return x2SecCanAddrDic; } }//水熄罐
        public static Dictionary<int, ProtocolAddr> x2SecCanAddrDic;
        public static Dictionary<int, ProtocolAddr> MAddrDic { get { return mAddrDic; } }
        public static Dictionary<int, ProtocolAddr> mAddrDic;
        #endregion
        #region 炉号对应的中心地址，主要用来判断对位情况
        public static Dictionary<int, int> PRoomNumDic { get { return pRoomNumDic; } }
        private static Dictionary<int, int> pRoomNumDic;
        public static Dictionary<int, int> TRoomNumDic { get { return tRoomNumDic; } }
        private static Dictionary<int, int> tRoomNumDic;
        public static Dictionary<int, int> LRoomNumDic { get { return lRoomNumDic; } }
        public static Dictionary<int, int> lRoomNumDic;
        public static Dictionary<int, int> XFstCanRoomNumDic { get { return xFstCanRoomNumDic; } }
        public static Dictionary<int, int> xFstCanRoomNumDic;
        public static Dictionary<int, int> X1SecCanRoomNumDic { get { return x1SecCanRoomNumDic; } }
        public static Dictionary<int, int> x1SecCanRoomNumDic;
        public static Dictionary<int, int> X2SecCanRoomNumDic { get { return x2SecCanRoomNumDic; } }//水熄罐
        public static Dictionary<int, int> x2SecCanRoomNumDic;
        public static Dictionary<int, int> MRoomNumDic { get { return mRoomNumDic; } }
        public static Dictionary<int, int> mRoomNumDic;
        #endregion
        public void GetProtocalAddr(string addrFileName, out Dictionary<int, ProtocolAddr> addrDic)
        {
            //Dictionary<物理地址，协议地址>
            addrDic = new Dictionary<int, ProtocolAddr>();
            //获取地址的时候的config下的第一个XmlNode的节点为<Addr/>,每个XmlElement的节点为<add/>
            int addr = 0;
            OperateConfig config = new OperateConfig(addrFileName);
            #region 每个XmlElement(XmlNode)对象的Attributes的Index是从0开始的，可以使用XmlNode.Item(idex)方法来访问
            foreach (XmlNode xmlNode in config.XmlNodeList)
            {
                if (xmlNode != null)
                {
                    ProtocolAddr protocalAddr = new ProtocolAddr();
                    //码牌物理地址
                    string physicalAddr = xmlNode.Attributes.Item(0).Value;
                    addr = Convert.ToInt32(physicalAddr);
                    //实际炉号
                    string roomNum = xmlNode.Attributes.Item(1).Value;
                    protocalAddr.RoomNum = Convert.ToByte(roomNum);
                    //软件上的位置
                    string softwareAddr = xmlNode.Attributes.Item(2).Value;
                    protocalAddr.SoftwareAddr = Convert.ToInt32(softwareAddr);
                    addrDic.Add(addr, protocalAddr);
                }
            }
            #endregion
        }
        public void GetMiddleAddr(string roomNumFileName, out Dictionary<int, int> roomNumDic)
        {
            roomNumDic = new Dictionary<int, int>();
            int roomNum = 0;
            int physicalAddr = 0;
            //获取地址的时候的config下的第一个XmlNode的节点为<Addr/>,每个XmlElement的节点为<add/>
            OperateConfig config = new OperateConfig(roomNumFileName);
            #region 每个XmlElement(XmlNode)对象的Attributes的Index是从0开始的，可以使用XmlNode.Item(idex)方法来访问
            foreach (XmlNode xmlNode in config.XmlNodeList)
            {
                if (xmlNode != null)
                {
                    //码牌物理地址
                    string num = xmlNode.Attributes.Item(0).Value;
                    roomNum = Convert.ToInt32(num);
                    //实际炉号
                    string addr = xmlNode.Attributes.Item(1).Value;
                    physicalAddr = Convert.ToInt32(addr);
                    roomNumDic.Add(roomNum, physicalAddr);
                }
            }
            #endregion
        }
    }
    class ProtocolAddr
    {
        public byte RoomNum { get; set; }
        public int SoftwareAddr { get; set; }
    }
}
