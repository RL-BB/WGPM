using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WGPM.R.OPCCommunication
{
    class CommStatus
    {
        public CommStatus(bool[] arr)
        {
            //①PLC，②触摸屏Touch，③无线模块Wireless，④解码器Decode
            int index = 0;
            PLC = arr[index++];
            Touch = arr[index++];
            Wireless = arr[index++];
            Decoder = arr[index];
        }
        public bool Wireless { get; set; }
        public bool PLC { get; set; }
        public bool Touch { get; set; }
        public bool Decoder { get; set; }
    }
}
