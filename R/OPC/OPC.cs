using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OPCAutomation;
using System.Windows.Threading;
using System.Net;
using System.Collections;
using WGPM.R.UI;

namespace WGPM.R.KepServer
{
    internal class OPC
    {
        #region OPC变量
        public const byte KepReadByteLength = 18; //定义READ中byte数组的大小为50，读的字节长度，为18，ushort
        public const byte KepWriteByteLength = 28; //定义WRITE中byte数组的大小为70，写的字节长度，为28，ushort
        public const byte KepByteLength = 6; //读写中分别有几项，分别为8项，推拦熄煤分别有两个，T1，T2，L1，L1，X1，X1，M1，M1
        private string StrHostIP = "";
        private readonly OPCItem[] kepItem = new OPCItem[16];
        private readonly bool opcConnected;
        public bool IsGetData = false;
        public ushort[][] KepDataRead = new ushort[8][]; //从opc读到的数据,***此处为二维数组***
        //从opc写入的数据，读写中分别有8项，推拦熄煤分别有两个，T1，T2，L1，L1，X1，X1，M1，M1
        // ***此处为二维数组***,一维为车（车型+车号），二维为字节数量（此处为70个字节，对应OPC，KEPServerEx软件中设置的内容）
        public ushort[][] KepDataWrite = new ushort[8][];//写有八项
        public int[][] ABDataWrite = new int[2][];
        public ushort[][] ServerWrt = new ushort[1][];
        private OPCGroup kepGroup;
        private OPCGroups kepGroups;
        private OPCItems kepItems;
        private OPCServer kepServer;
        private string kepServerName;
        private string strHostName = "";

        #endregion
        //计时器用来AsyncWrite到PLC
        private readonly DispatcherTimer _timerOpc = new DispatcherTimer();
        //开始连接
        public OPC()
        {
            try
            {
                GetLocalServer();
            }
            catch (Exception err)
            {
                //C.LogOpc.Info("初始化GetLocalServer出错：" + err.Message);
            }
            try
            {
                //string remoteIp = Setting.AreaFlag ? "192.168.0.3" : "192.168.0.4";
                if (!ConnectRemoteServer("127.0.0.1", kepServerName))
                {
                    return;
                }
                opcConnected = true;
                _timerOpc.Tick += TimerOpc_Tick;
                _timerOpc.Interval = TimeSpan.FromMilliseconds(200);
                _timerOpc.Start();
                //RecurBrowse(KepServer.CreateBrowser());
                //if (!CreateGroup())//R 20170308这条语句存在的意义似乎就是为了执行CreateGroup()方法，使用if语句的意义看不到；
                //{
                //}
                CreateGroup();//R20170323
            }
            catch (Exception err)
            {
                //C.LogOpc.Info("初始化出错：" + err.Message);
            }
        }
        //定时器，发送数据到OPC;R 20170314 发送数据到和PLC直接建立通讯的OPC服务器，相当于写数据到PLC
        private void TimerOpc_Tick(object sender, EventArgs e)
        {
            try
            {
                #region 发送到opc
                if (!opcConnected) return;
                for (int i = 8; i <= 15; i++)
                {
                    //发送
                    int[] temp = { 0, kepItem[i].ServerHandle };//R 20170318注意格式
                    Array serverHandles = temp;
                    //object[] valueTemp = { "", KepDataWrite[i - 8] };
                    IEnumerable wrtArr = null;
                    wrtArr = Setting.AreaFlag ? KepDataWrite[i - 8] : ((i == 12 || i == 13) ? (IEnumerable)ABDataWrite[i - 12] : KepDataWrite[i - 8]);
                    object[] valueTemp = { "", wrtArr };
                    Array values = valueTemp;
                    Array errors;
                    int cancelId;
                    kepGroup.AsyncWrite(1, ref serverHandles, ref values, out errors, i, out cancelId);
                    //GC.Collect();//
                }
                #endregion
            }
            catch (Exception err)
            {
                //C.LogOpc.Info("OPC数据发送错误：" + err.Message);
            }
        }
        //获取本地OPC服务器
        private void GetLocalServer()
        {
            //通过IP来获取计算机名称，可用在局域网内
            //StrHostIP = Setting.AreaFlag ? "192.168.0.3" : "192.168.0.4";
            IPHostEntry ipHostEntry = Dns.GetHostEntry(StrHostIP);
            strHostName = ipHostEntry.HostName;
            //获取本地计算机上的OPCServerName
            try
            {
                kepServer = new OPCServer();
                object serverList = kepServer.GetOPCServers(strHostName);
                foreach (string turn in (Array)serverList)
                {
                    kepServerName = turn;
                }
            }
            catch (Exception err)
            {
                //C.LogOpc.Info("枚举本地OPC服务器出错：" + err.Message);
            }
        }
        private bool CreateGroup()
        {
            try
            {
                kepGroups = kepServer.OPCGroups;
                kepGroup = kepGroups.Add("OPCDOTNETGROUP");
                SetGroupProperty();
                kepItems = kepGroup.OPCItems;
                kepItems.DefaultIsActive = true;
                kepGroup.DataChange += KepGroup_DataChange;//数据change事件，每当项数据有变化时执行的事件
                kepGroup.AsyncWriteComplete += KepGroup_AsyncWriteComplete;//异步写 完成事件
                kepGroup.AsyncReadComplete += GroupAsyncReadComplete;//异步读 完成事件
                #region R 20170314 OpcItems.AddItem(ItemID,ClientHandle),其中0~15分别对应T,L,X,M四大车共计8辆车的发送(Write)和接受(Read);ItemID为从OPC服务器上约定好的Name
                int index = 0;
                //Read
                kepItem[index++] = kepItems.AddItem("T1.T1.R", 0);
                kepItem[index++] = kepItems.AddItem("T2.T2.R", 1);
                kepItem[index++] = kepItems.AddItem("L1.L1.R", 2);
                kepItem[index++] = kepItems.AddItem("L2.L2.R", 3);
                kepItem[index++] = kepItems.AddItem("X1.X1.R", 4);
                kepItem[index++] = kepItems.AddItem("X2.X2.R", 5);
                kepItem[index++] = kepItems.AddItem("M1.M1.R", 6);
                kepItem[index++] = kepItems.AddItem("M2.M2.R", 7);
                //Write
                kepItem[index++] = kepItems.AddItem("T1.T1.W", 8);
                kepItem[index++] = kepItems.AddItem("T2.T2.W", 9);
                kepItem[index++] = kepItems.AddItem("L1.L1.W", 10);
                kepItem[index++] = kepItems.AddItem("L2.L2.W", 11);
                kepItem[index++] = kepItems.AddItem("X1.X1.W", 12);
                kepItem[index++] = kepItems.AddItem("X2.X2.W", 13);
                kepItem[index++] = kepItems.AddItem("M1.M1.W", 14);
                kepItem[index++] = kepItems.AddItem("M2.M2.W", 15);
                #region 之前丰城在用
                //kepItem[1] = kepItems.AddItem("T.T2.READ", 1);
                //kepItem[2] = kepItems.AddItem("L.L1.READ", 2);
                //kepItem[3] = kepItems.AddItem("L.L2.READ", 3);
                //kepItem[4] = kepItems.AddItem("X.X1.READ", 4);
                //kepItem[5] = kepItems.AddItem("X.X2.READ", 5);
                //kepItem[6] = kepItems.AddItem("M.M1.READ", 6);
                //kepItem[7] = kepItems.AddItem("M.M2.READ", 7);
                //kepItem[8] = kepItems.AddItem("T.T1.WRITE", 8);
                //kepItem[9] = kepItems.AddItem("T.T2.WRITE", 9);
                //kepItem[10] = kepItems.AddItem("L.L1.WRITE", 10);
                //kepItem[11] = kepItems.AddItem("L.L2.WRITE", 11);
                //kepItem[12] = kepItems.AddItem("X.X1.WRITE", 12);
                //kepItem[13] = kepItems.AddItem("X.X2.WRITE", 13);
                //kepItem[14] = kepItems.AddItem("M.M1.WRITE", 14);
                //kepItem[15] = kepItems.AddItem("M.M2.WRITE", 15);
                #endregion
                #endregion
            }
            catch (Exception err)
            {
                //C.LogOpc.Info("创建组出现错误：" + err.Message);
                return false;
            }
            return true;
        }
        //数据变化事件
        /// <summary>
        ///     每当项数据有变化时执行的事件
        /// </summary>
        /// <param name="transactionID">处理ID</param>
        /// <param name="numItems">项个数</param>
        /// <param name="clientHandles">项客户端句柄</param>
        /// <param name="itemValues">TAG值</param>
        /// <param name="qualities">品质</param>
        /// <param name="timeStamps">时间戳</param>
        private void KepGroup_DataChange(int transactionID, int numItems, ref Array clientHandles, ref Array itemValues,
            ref Array qualities, ref Array timeStamps)
        {
            try
            {
                for (int index = 1; index <= numItems; index++)
                {
                    byte temp = Convert.ToByte(clientHandles.GetValue(index));
                    //if (temp > 7) continue;//R20170323 定义0-7位Read，8-15为写，name当temp=8时，应该break？！
                    if (temp <= 7)
                    {
                        int[] data = (int[])itemValues.GetValue(index);
                        ushort[] uData = new ushort[data.Length];
                        for (int i = 0; i < data.Length; i++)
                        {
                            uData[i] = (ushort)data[i];
                        }
                        if (uData == null) continue;
                        KepDataRead[temp] = uData;
                        IsGetData = true;
                    }
                }
            }
            catch (Exception err)
            {
                //C.LogOpc.Info("OPC数据获取错误：" + err.Message);
            }
        }
        //设置组属性
        private void SetGroupProperty()
        {
            kepServer.OPCGroups.DefaultGroupIsActive = true;
            kepServer.OPCGroups.DefaultGroupDeadband = 0;
            kepGroup.UpdateRate = 250;
            kepGroup.IsActive = true;
            kepGroup.IsSubscribed = true;
        }
        //连接远程OPC服务器
        private bool ConnectRemoteServer(string remoteServerIP, string remoteServerName)
        {
            try
            {
                kepServer.Connect(remoteServerName, remoteServerIP);

                if (kepServer.ServerState == (int)OPCServerState.OPCRunning)
                {
                    //C.LogOpc.Info("已连接到-" + _kepServer.ServerName);
                }
                else
                {
                    //这里你可以根据返回的状态来自定义显示信息，请查看自动化接口API文档
                    //C.LogOpc.Info("状态：" + _kepServer.ServerState);
                }
            }
            catch (Exception err)
            {
                //C.LogOpc.Info("连接远程服务器出现错误：" + err.Message);
                //return false;
            }
            return true;
        }

        //异步写 完成事件
        private static void KepGroup_AsyncWriteComplete(int transactionID, int numItems, ref Array clientHandles,
            ref Array errors)
        {
        }
        //异步读 完成事件
        private static void GroupAsyncReadComplete(int transactionID, int numItems, ref Array clientHandles,
            ref Array itemValues, ref Array qualities, ref Array timeStamps, ref Array errors)
        {
        }

    }
}
