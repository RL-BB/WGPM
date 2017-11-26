using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WGPM.R.Logger
{
    class Log
    {
        public static readonly ILog LogErr = LogManager.GetLogger("Log_Err");
        public static readonly ILog LogSys = LogManager.GetLogger("Log_Sys");
        public static readonly ILog LogOpc = LogManager.GetLogger("Log_Opc");
    }
}
