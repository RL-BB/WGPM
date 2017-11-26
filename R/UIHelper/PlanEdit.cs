using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGPM.R.RoomInfo;
using WGPM.R.XML;

namespace WGPM.R.UI
{
    public partial class PlanEdit
    {

        /// <summary>
        /// 第一炉计划，并显示在TextBox中
        /// </summary>
        /// <param name="area"></param>
        /// <param name="period"></param>
        /// <param name="group"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private TPushPlan SetFirstPlan(byte roomNum, DateTime time)
        {
            try
            {
                TPushPlan p = new TPushPlan();
                p.Group = cboGroup.SelectedIndex;
                p.Period = cboPeriod.SelectedIndex;
                p.RoomNum = roomNum;
                p.PushTime = time;
                p.BurnTime = p.GetBurnTime();
                return p;
            }
            catch (Exception)
            {
                return null;
            }

        }
        private void TxtDisplayFstPlan(TPushPlan plan)
        {
            txtRoom.Text = plan.RoomNum.ToString("000");
            //txtPushTime.Text = plan.TxtPushTime;
            //txtBurnTime.Text = plan.StrBurnTime;
        }
        /// <summary>
        /// 先得到，由输入的第一炉计划生成当前时段的计划
        /// </summary>
        /// <param name="plan"></param>
        private void GeneratePlanBy(TPushPlan plan)
        {
            if (editingTPlan.Count == 0)
            {
                editingTPlan.Add(plan);

            }
            for (int i = 0; i < 110; i++)
            {
                TPushPlan pGenerate = new TPushPlan();

                pGenerate.PushTime = editingTPlan.Last().PushTime.AddMinutes(BreakTime);
                //bool right = IsTimeRight(cboPeriod.SelectedIndex, pGenerate.PushTime);
                //下一炉的出焦时间超过时段规定则跳出生成
                if (!IsTimeRight(cboPeriod.SelectedIndex, pGenerate.PushTime)) break;
                bool area = cboArea.SelectedIndex <= 0;//炉区
                pGenerate.RoomNum = GetNextRoomNum(editingTPlan.Last().RoomNum);
                pGenerate.Group = plan.Group;
                pGenerate.Period = plan.Period;
                pGenerate.StokingTime=CokeRoom.BurnStatus[pGenerate.RoomNum].StokingTime;
                //DateTime stokingTime = (area ? CokeRoom.BurnStatus12 : CokeRoom.BurnStatus34)[pGenerate.RoomNum].StokingTime;
                pGenerate.BurnTime = (int)(pGenerate.PushTime - pGenerate.StokingTime).TotalMinutes;
                editingTPlan.Add(pGenerate);
            }
            editingTPlan.Sort(TPushPlan.CompareByTime);
        }
        private byte GetNextRoomInfo12(int roomNum)
        {
            int nextRoom = roomNum + 5;
            if (nextRoom >= 56 && nextRoom <= 60)
            {//具体规则：1#52→2#59→1#54→2#56→1#51→2#58→1#53→2#60→1#55→2#57→1#52；
                nextRoom = nextRoom + 2 <= 60 ? nextRoom + 2 : nextRoom + 2 - 5;
            }
            if (nextRoom >= 111 && nextRoom <= 115)
            {
                int r = nextRoom % 10;
                nextRoom = r != 0 ? r : 5;
            }
            return (byte)nextRoom;
        }
        /// <summary>
        /// 3、4#炉区编辑计划时炉号的递增方法
        /// </summary>
        /// <param name="roomNum"></param>
        /// <returns></returns>
        private byte GetNextRoomNum(int roomNum)
        {
            int nextRoom = roomNum + 5;
            if (nextRoom <= 110)
            {
                roomNum = nextRoom;
            }
            else
            {
                roomNum = (roomNum % 5 + 2);
                if (roomNum > 5)
                {
                    roomNum = roomNum - 5;
                }
            }
            return (byte)roomNum;
        }
        /// <summary>
        /// 根据四个小时段得到生成计划的出焦时间范围
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        private static string GetStartTimeFromPeirod(int period)
        {
            string pTime = null;
            switch (period)
            {
                case 1:
                    pTime = "08:01";
                    break;
                case 2:
                    pTime = "16:01";
                    break;
                case 3:
                    pTime = "20:01";
                    break;
                case 4:
                    pTime = "00:01";
                    break;
                default:
                    break;
            }
            return pTime;
        }
        /// <summary>
        /// 判断时间是否在相应时段内
        /// </summary>
        /// <param name="period">时段</param>
        /// <param name="t">当前录入的时间</param>
        /// <returns>录入的时间是否有效</returns>
        private bool IsTimeRight(int period, DateTime t)
        {
            int h = t.Hour;
            bool right = false;
            switch (period)
            {
                case 1://白班
                    right = (h >= 8 && h < 20) ? true : false;
                    break;
                case 2://白中
                    right = (h >= 20 || h < 8) ? true : false;
                    break;
                default:
                    break;
            }
            return right;
        }
        private bool IsPrintTimeRight(int period, DateTime t)
        {
            int h = t.Hour;
            bool right = false;
            switch (period)
            {
                case 0:
                    right = (h >= 8 && h < 20) ? true : false;
                    break;
                case 1:
                    right = (h >= 20 || h < 8) ? true : false;
                    break;
                default:
                    break;
            }
            return right;
        }

        #region ComboBoxChanged
        private List<TPushPlan> GetItemsSource(int period, int group)
        {
            if (CokeRoom.PushPlan == null)
            {
                return null;
            }
            List<TPushPlan> lstPlan = new List<TPushPlan>();
            bool Group = group == 0;
            bool Period = period == 0;
            if (Group && Period)
            {//全部班组、时段
                lstPlan.AddRange(CokeRoom.PushPlan);
            }
            else if (Group && !Period)
            {//全部班组，非全部时段（白，白中，夜中，夜）
                for (int i = 0; i < CokeRoom.PushPlan.Count; i++)
                {
                    if (period == CokeRoom.PushPlan[i].Period)
                    {
                        lstPlan.Add(CokeRoom.PushPlan[i]);
                    }
                }
            }
            else if (!Group && Period)
            {//非全部班组（甲乙丙丁），全部时段
                for (int i = 0; i < CokeRoom.PushPlan.Count; i++)
                {
                    if (group == CokeRoom.PushPlan[i].Group)
                    {
                        lstPlan.Add(CokeRoom.PushPlan[i]);
                    }
                }
            }
            else
            {//班组、时段都非“全部”
                for (int i = 0; i < CokeRoom.PushPlan.Count; i++)
                {
                    if ((group == CokeRoom.PushPlan[i].Group) && (period == CokeRoom.PushPlan[i].Period))
                    {
                        lstPlan.Add(CokeRoom.PushPlan[i]);
                    }
                }
            }
            return lstPlan;
        }
        /// <summary>
        /// 计划打印时段：得到待打印的计划
        /// </summary>
        /// <param name="printArea">炉区</param>
        /// <param name="printPeriod">时段</param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private List<TPushPlan> GetItemsSource(int printArea, short printPeriod, bool isEdit)
        {
            List<TPushPlan> plan = isEditing ? editingTPlan : displayPlan;
            List<TPushPlan> lstPlan = new List<TPushPlan>();
            for (int i = 0; i < plan.Count; i++)
            {//1#,3#炉区 炉号<=55；2#、4#炉区炉号>=56
             //1#,3#炉区,对2取余的意义：SelectedIndex=0,1,2,3,4
             //先判断时段和时间是否符合？
                if (!IsPrintTimeRight(printPeriod, plan[i].PushTime))
                {
                    continue;
                }
                if (printArea == 0)
                {//1、3#炉区
                    if (plan[i].RoomNum <= 55)
                    {
                        lstPlan.Add(plan[i]);
                    }
                }
                else
                {//2/4#炉区
                    if (plan[i].RoomNum >= 56)
                    {
                        lstPlan.Add(plan[i]);
                    }
                }
            }
            return lstPlan;
        }
        #endregion
        /// <summary>
        /// 验证TextBox的输入值是否合法
        /// </summary>
        /// <param name="text">TextBox的输入值</param>
        /// <returns>不合法返回值为-1，合法则得到转换后的值</returns>
        private int GetInputRoomNum(string text)
        {
            int num = 0;
            bool legal = int.TryParse(text, out num);
            if (!legal)
            {
                num = -1;
            }
            return num;
        }
        /// <summary>
        /// 验证TextBox的输入值PushTime or BurnTime是否合法，并得到该值
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private DateTime GetInputDateTime(string text, DateTime? dpDateTime)
        {
            string dpTime = ((DateTime)dpDateTime).ToString("d") + " " + text;
            DateTime dt;
            bool legal = DateTime.TryParse(dpTime, out dt);
            if (!legal)
            {
                dt = Convert.ToDateTime("1988-04-15");
            }
            else
            {
                if (dt.Hour < 8) dt = dt.AddDays(1);
            }
            return dt;
        }
        /// <summary>
        /// 给dgPlan的ItemsSource赋值
        /// </summary>
        /// <param name="lstPlan"></param>
        private void AssignDataGridItemsSource(int period, int group)
        {
            displayPlan = GetItemsSource(period, group);
            dgPlan.ItemsSource = null;
            dgPlan.ItemsSource = displayPlan;
        }
    }
}
