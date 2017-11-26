using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Threading;
using WGPM.R.OPCCommunication;
using WGPM.R.RoomInfo;

/// <summary>
///  @"Server=RL\MYSERVER;Integrated Security=False;Database=WGPM;User ID=sa;Password=sql2008r2;Persist Security Info=True";
///   @"Data Source=RL\MYSERVER;Initial Catalog=WGPM;Integrated Security=True";
///   INSERT INTO [Cvacs].[dbo].[LockInfo]([计划炉号],[实际炉号],[实际推焦时间],[预定出焦时间],[时段],[班组],[联锁状态信息],[联锁类型],[工作车序列],[对中序列],[推焦车地址],[拦焦车地址],[熄焦车地址],[装煤车地址],[车辆通讯状态])VALUES ()
///   INSERT INTO [Cvacs].[dbo].[LockInfo] Values()
/// @"Data Source=MXPG85V95XX07DB;Initial Catalog=WGPM;Persist Security Info=True;User ID=sa;Password=sql2008r2";
/// </summary>
namespace WGPM.R.SqlHelper
{
    /// <summary>
    /// 由T/L/X三大工作车得到联锁信息：联锁点位转换为一个数据
    /// 考虑是否增加解析的方法：用于查询统计时的观察
    /// </summary>
    class PushTogetherInfo
    {
        /// <summary>
        /// 存储推焦时 相关车辆信息
        /// </summary>
        /// <param name="actualRoomNum">推焦车炉号</param>
        /// <param name="index">CokeRoom.PushPlan中最接近推焦车所在炉号的计划的索引</param>
        /// <param name="now">实际推焦时间</param>
        /// <param name="job">工作车与否</param>
        public PushTogetherInfo(byte actualRoomNum, int index, string now, bool job)
        {
            int lstIndex = index >= 0 ? index : 0;
            PlanRoomNum = CokeRoom.PushPlan[lstIndex].RoomNum;
            ActualRoomNum = actualRoomNum;
            ActualPushTime = Convert.ToDateTime(now);
            PlanToPushTime = CokeRoom.PushPlan[lstIndex].PushTime;
            StokingTime = Convert.ToDateTime("2012年4月3日 13:14");//默认值

            Period = CokeRoom.PushPlan[lstIndex].Period;
            Group = CokeRoom.PushPlan[lstIndex].Group;
            DwTogetherInfo tInfo = job ? OPCCommunication.Communication.JobCarTogetherInfo : OPCCommunication.Communication.NonJobCarTogetherInfo;
            PushLockInfo = tInfo.InfoToInt;
            PingLockInfo = 333;

            List<Vehicles.Vehicle> lst = job ? OPCCommunication.Communication.TJobCarLst : OPCCommunication.Communication.NonJobCarLst;
            PushCarList = lst[0].CarNum * 100 + lst[1].CarNum * 10 + lst[2].CarNum;
            PingCarList = 333;
            //推拦熄煤箭头:20170601 暂设置为固定值16;当工作推焦车出焦时，有三工作车得到箭头；非工作车时，置零
            ArrowsList = 16;
            TAddr = lst[0].DataRead.PhysicalAddr;
            LAddr = lst[1].DataRead.PhysicalAddr;
            XAddr = lst[2].DataRead.PhysicalAddr;
            ZAddr = 0;
            Communication = 123;//20170601 通讯状态暂未处理
        }
        //1计划炉号
        public byte PlanRoomNum;
        //2实际炉号
        public byte ActualRoomNum;
        //3实际推焦时间
        public DateTime ActualPushTime;
        //4预定出焦时间
        public DateTime PlanToPushTime;
        // 实际装煤时间
        public DateTime StokingTime;
        //5时段
        public int Period;
        //6班组
        public int Group;
        //7联锁状态信息：推焦请求，推焦联锁，推到位(对中与否)，推炉门已摘，推焦开始，推焦结束，拦到位，拦焦车炉门已摘，导焦槽锁闭，拦人工允推，熄到位，焦罐Ready（旋转，锁闭）,水熄/干熄(0,1),焦罐号，1#罐有无，2#罐有无，一级允推，二级允推，平煤请求
        public int PushLockInfo;
        // 平煤联锁信息
        public int PingLockInfo;
        //8工作车序列：如3#推焦车，2#拦焦车，1#熄焦车,则 JobCarList=3*100+2*10+1，即321
        public int PushCarList;
        // 平煤工作车序列
        public int PingCarList;
        //9对中序列，箭头指示用4bit来表示，0000，高位为左箭头，低位为右箭头，如1100左2,0100左1,0010右1,0011右2,0000为对中，四辆车需求16位，ushort数据类型
        public short ArrowsList;
        //10推焦车地址
        public int TAddr;
        //11拦焦车地址
        public int LAddr;
        //12熄焦车地址
        public int XAddr;
        //13装煤车地址
        public int ZAddr;
        //14车辆通讯状态：从低位到高位分别表示2推 2拦 2熄 2煤车 的通讯状态；如00001000表示只有2#拦焦车通讯正常，其他断开
        public byte Communication;
        //SqlCommand参数
        public SqlParameter[] PushParms
        {
            get
            {
                SqlParameter[] parms =
                {
                    new SqlParameter("PlanRoomNum",PlanRoomNum),
                    new SqlParameter("ActualRoomNum",ActualRoomNum),
                    new SqlParameter("ActualPushTime",ActualPushTime),
                    new SqlParameter("PlanToPushTime",PlanToPushTime),
                    new SqlParameter("StokingTime",StokingTime),
                    new SqlParameter("Period",Period),
                    new SqlParameter("Group",Group),
                    new SqlParameter("PushLockInfo",PushLockInfo),
                    new SqlParameter("PingLockInfo",PingLockInfo),
                    new SqlParameter("PushJobCarList",PushCarList),
                    new SqlParameter("PingJobCarList",PingCarList),
                    new SqlParameter("ArrowsList",ArrowsList),
                    new SqlParameter("TAddr",TAddr),
                    new SqlParameter("LAddr",LAddr),
                    new SqlParameter("XAddr",XAddr),
                    new SqlParameter("ZAddr",ZAddr),
                    new SqlParameter("Communication",Communication)
                };
                return parms;
            }
        }
        //数据库执行命令语句
        public string PushCommandText
        {
            get
            {//insert [表名]([列名4],[列名4],[列名4],[列名4])VALUES('值1','值2','值3','值4');
                return @"INSERT INTO [WGPM].[dbo].[LockInfo]VALUES(@PlanRoomNum,@ActualRoomNum,@ActualPushTime,
                        @PlanToPushTime,@StokingTime,@Period,@Group,@PushLockInfo,@PingLockInfo,
                        @PushJobCarList,@PingJobCarList,@ArrowsList,@TAddr,@LAddr,@XAddr,@ZAddr,@Communication)";
            }
        }
        public void RecPushInfoToDB()
        {
            SqlRecordHelper r = new SqlRecordHelper(PushCommandText, PushParms);
            r.RecordInfoToDB();
        }
        /// <summary>
        /// 20170529 装煤信息未处理，需增加Update语句
        /// </summary>
        private void RecPingInfoToDB()
        {
            //推焦车位置，炉号
            //平煤杆信息：长度，电流
            //装煤车位置
            //装煤车
        }
    }
    class PingInfo
    {
        public PingInfo() { }
        public PingInfo(string time, int carLst, int roomNum)
        {
            StokingTime = Convert.ToDateTime(time);
            PingCarList = carLst;
            ZAddr = Addrs.MRoomNumDic[roomNum];
            RoomNum = roomNum;
            ActualPushTime = CokeRoom.BurnStatus[roomNum].ActualPushTime;
        }
        public DateTime StokingTime;//装煤时间：平煤结束时的时间
        public int PingCarList;//平煤开始信号到达后记录 推焦车车号和装煤车车号 及装煤车物理地址
        public int ZAddr;
        public int RoomNum;
        public DateTime ActualPushTime;
        public string UpdateCommandText
        {
            get
            {
                return @"UPDATE [LockInfo] SET [实际装煤时间] =@StokingTime,[Ping工作车序列]=@PingCarList,
                        [装煤车地址]=@ZAddr WHERE [计划炉号]=@RoomNum And [实际推焦时间]=@ActualPushTime";
            }
        }
        public SqlParameter[] PingParms
        {
            get
            {
                SqlParameter[] parms =
               {
                    new SqlParameter("StokingTime",StokingTime),
                    new SqlParameter("PingCarList",PingCarList),
                    new SqlParameter("ZAddr",ZAddr),
                    new SqlParameter("RoomNum",RoomNum),
                    new SqlParameter("ActualPushTime",ActualPushTime)
                };
                return parms;
            }
        }
        public void RecPingInfoToDB()
        {
            SqlRecordHelper r = new SqlRecordHelper(UpdateCommandText, PingParms);
            r.RecordInfoToDB();
        }
    }
    class PlanDB
    {
        //1炉号
        public int RoomNum;
        //2预定出焦时间
        public DateTime PlanToPushTime;
        // (上次)装煤时间
        public DateTime StokingTime;
        //3计划结焦时间
        public int PlanBurnTime;
        //4规定结焦时间
        public int StandardBurnTime;
        //5时段
        public int Period;
        //6班组
        public int Group;
        //7日期
        public DateTime Date;
        //
        public SqlParameter[] Parms
        {
            get
            {
                SqlParameter[] parms =
                {
                    new SqlParameter("RoomNum",RoomNum),
                    new SqlParameter("PlanToPushTime",PlanToPushTime),
                    new SqlParameter("StokingTime",StokingTime),
                    new SqlParameter("PlanBurnTime",PlanBurnTime),
                    new SqlParameter("StandardBurnTime",StandardBurnTime),
                    new SqlParameter("Period",Period),
                    new SqlParameter("Group",Group),
                };
                return parms;
            }
        }
        //数据库执行命令语句
        public string InsertCommandText
        {
            get
            {//insert [表名]([列名4],[列名4],[列名4],[列名4])VALUES('值1','值2','值3','值4'); ***注意表名
                return @"INSERT INTO [WGPM].[dbo].[testPlan]VALUES(@RoomNum,@PlanToPushTime,@StokingTime,@PlanBurnTime,@StandardBurnTime,@Period,@Group)";
            }
        }
        public string DelCommandText
        {
            get
            {
                return "delete From [WGPM].[dbo].[Plan] where [预定出焦时间]>=@startTime and [预定出焦时间]<@endTime";
            }
        }
        public string UpdateCommandText
        {
            get
            {//insert [表名]([列名4],[列名4],[列名4],[列名4])VALUES('值1','值2','值3','值4');
                return @"UPDATE [WGPM].[dbo].[Plan]VALUES(@RoomNum,@PlanToPushTime,@PlanBurnTime,@StandardBurnTime,
                       @Period,@Group)";
            }
        }
        public void RecToDB()
        {
            SqlRecordHelper rec = new SqlRecordHelper();
            rec.RecordInfoToDB();
        }
        public void UpdateToDB()
        {

        }
        public void IniValue(TPushPlan plan, int burnTime)
        {
            RoomNum = plan.RoomNum;
            PlanToPushTime = plan.PushTime;
            StokingTime = plan.StokingTime;
            PlanBurnTime = plan.BurnTime;
            StandardBurnTime = burnTime;
            Period = plan.Period;
            Group = plan.Group;
        }
    }
    class SqlRecordHelper
    {
        public SqlRecordHelper() { }
        public SqlRecordHelper(string commandText, SqlParameter[] parms)
        {
            this.insertCommandText = commandText;
            this.parms = parms;
        }
        public SqlRecordHelper(string commandText, string delText, List<SqlParameter[]> parmsList)
        {
            this.insertCommandText = commandText;
            this.delCommandText = delText;
            this.parmsList = parmsList;
        }
        private string insertCommandText;
        private string delCommandText;
        private SqlParameter[] parms;
        private List<SqlParameter[]> parmsList;
        //数据库连接字符串
        public string ConnectionStr
        {
            get
            {
                //笔记本连接字符串
                return @"Server=RL\MYSERVER;Integrated Security=False;Database=WGPM;User ID=sa;Password=sql2008r2;Persist Security Info=True";
                //台式连接字符串
                //return @"Data Source=MXPG85V95XX07DB;Initial Catalog=WGPM;Persist Security Info=True;User ID=sa;Password=sql2008r2";
            }
        }
        public void RecordInfoToDB()
        {
            Thread thread = new Thread(() =>
            {
                using (SqlConnection conn = new SqlConnection(ConnectionStr))
                {
                    conn.Open();
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = insertCommandText;
                        for (int i = 0; i < parms.Length; i++)
                        {
                            command.Parameters.Add(parms[i]);
                        }
                        command.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            })
            { IsBackground = true };
            thread.Start();
        }
        public void RecordPlanInfoToDB(int period)
        {
            Thread thread = new Thread(() =>
            {
                using (SqlConnection conn = new SqlConnection(ConnectionStr))
                {
                    conn.Open();
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        //查询已有计划
                        //删除已有计划
                        //command.CommandText = delCommandText;
                        //DateTime[] time = GetDelParms(period);
                        //command.Parameters.Add("startTime", time[0]);
                        //command.Parameters.Add("endTime", time[1]);
                        //command.ExecuteNonQuery();
                        //command.Parameters.Clear();
                        //添加新计划
                        command.CommandText = insertCommandText;
                        for (int i = 0; i < parmsList.Count; i++)
                        {
                            for (int j = 0; j < parmsList[i].Length; j++)
                            {
                                if (i == 0)
                                {
                                    command.Parameters.Add(parmsList[i][j]);
                                }
                                else
                                {
                                    command.Parameters[j] = parmsList[i][j];
                                }
                            }
                            command.ExecuteNonQuery();
                        }
                    }
                }
            })
            { IsBackground = true };
            thread.Start();
        }
        private DateTime[] GetDelParms(int period)
        {
            DateTime startTime;
            DateTime endTime;
            int h = DateTime.Now.Hour;
            switch (period)
            {
                case 1://白班
                    startTime = DateTime.Now.Date.AddHours(8);
                    endTime = DateTime.Now.Date.AddHours(16);
                    if (h >= 20)
                    {
                        startTime = DateTime.Now.Date.AddDays(1).AddHours(8);
                        endTime = DateTime.Now.Date.AddDays(1).AddHours(16);
                    }
                    break;
                case 2://白中：夜中编辑计划白中计划时，需在当前的时间下增加一天
                    startTime = DateTime.Now.Date.AddHours(16);
                    endTime = DateTime.Now.Date.AddHours(20);
                    if (h >= 20)
                    {//只有在夜班的20:00-00:00时间段内编辑计划时，计划时间范围需增加一天
                        startTime = DateTime.Now.Date.AddDays(1).AddHours(8);
                        endTime = DateTime.Now.Date.AddDays(1).AddHours(16);
                    }
                    break;
                case 3://夜中：白班或夜中有可能编辑夜中计划
                    startTime = DateTime.Now.Date.AddHours(20);
                    endTime = DateTime.Now.Date.AddDays(1);
                    break;
                case 4://夜班:需注意夜班修改自己的计划时
                    startTime = DateTime.Now.Date.AddDays(1);
                    endTime = DateTime.Now.Date.AddDays(1).AddHours(8);
                    if (h <= 8)
                    {
                        startTime = DateTime.Now.Date;
                        endTime = DateTime.Now.Date.AddHours(8);
                    }
                    break;
                default:
                    startTime = DateTime.Now.Date.AddHours(8);
                    endTime = DateTime.Now.Date.AddHours(16);
                    break;
            }
            DateTime[] dt = { startTime, endTime };
            return dt;
        }
    }
}
