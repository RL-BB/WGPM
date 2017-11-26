using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace WGPM.R.CommHelper
{
    /// <summary>
    /// 直译：检查通讯
    /// </summary>
    public class CommExamine
    {
        public CommExamine(string[] ipArr)
        {
            this.ipArr = ipArr;
            commStatus = new bool[ipArr.Length];
            SucceedCount = new byte[ipArr.Length];
        }
        /// <summary>
        /// 传入的IPArr
        /// </summary>
        public string[] IPArr { get { return ipArr; } }
        private string[] ipArr;
        /// <summary>
        /// 后台ping的结果，bool，索引值0-3对应IP分配表的设备顺序：PLC，触摸屏，无线模块，解码器（或TLXM四个无线模块）
        /// </summary>
        public bool[] CommStatus { get { return commStatus; } }
        private bool[] commStatus;
        public byte[] SucceedCount;
        /// <summary>
        /// 后台Ping之后的结果：pingReply.Status==IPStatus.Success
        /// </summary>
        public void GetCommStatus()
        {
            PingOptions options = new PingOptions { DontFragment = true };
            byte[] buffer = { 0 };
            const int timeout = 120;
            try
            {
                for (int i = 0; i < ipArr.Length; i++)
                {
                    Ping pingClient = new Ping();
                    PingReply reply = pingClient.Send(IPAddress.Parse(ipArr[i]), timeout, buffer, options);
                    bool succeed = reply.Status == IPStatus.Success;
                    if (reply != null)
                    {
                        //commStatus[i] = reply.Status == IPStatus.Success;
                        if (succeed)
                        {
                            if (SucceedCount[i] < 3) SucceedCount[i]++;
                        }
                        else
                        {
                            SucceedCount[i] = 0;
                            continue;
                        }
                    }
                    commStatus[i] = succeed;
                }
            }
            catch (Exception)
            {

                return;
            }

        }
        /// <summary>
        /// 在cmd中对相应的IP地址进行ping操作
        /// </summary>
        /// <param name="tag">可以认为是IPArr的索引</param>
        public void ExcutePing(string tag)
        {
            int index = Convert.ToInt32(tag);
            Process p = new Process
            {
                StartInfo =
                {
                    FileName=@"cmd.exe",
                    UseShellExecute=true,
                    RedirectStandardInput=false,
                    RedirectStandardOutput=false,
                    CreateNoWindow=true,
                    Arguments=@"/k ping "+IPArr[index]+" -t"
                }
            };
            p.Start();
            p.Close();
        }
    }
}
