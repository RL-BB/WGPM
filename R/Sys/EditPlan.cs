using System;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WGPM.R.XML;
using WGPM.R.RoomInfo;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF;
using NPOI.POIFS.FileSystem;
using NPOI.HSSF.UserModel;

namespace WGPM.R.Sys
{
    class EditPlan
    {
        public static int GetNextRoomInfo12(int roomNum)
        {
            int nextRoom = roomNum + 5;
            if (nextRoom <= 50 || (nextRoom > 60 && nextRoom <= 110))
            {
                roomNum = nextRoom;
            }
            else if (nextRoom >= 56 && nextRoom <= 60)
            {
                roomNum = nextRoom == 56 ? 60 : (nextRoom - 1);
            }
            else if (nextRoom >= 111 && nextRoom <= 115)
            {
                if (roomNum != 106)
                {
                    roomNum = roomNum % 100 - 7;
                    roomNum = roomNum == 0 ? 5 : roomNum;
                }
                else
                {
                    roomNum = 4;
                }
            }
            return roomNum;
        }
        /// <summary>
        /// 3、4#炉区编辑计划时炉号的递增方法
        /// </summary>
        /// <param name="roomNum"></param>
        /// <returns></returns>
        public static int GetNextRoomInfo34(int roomNum)
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
            return roomNum;
        }


        /// <summary>
        /// 删除计划（isEditing==false时，更改Config文件的Attribute:Valid的值）
        /// </summary>
        /// <param name="lst">DataGrid.SelectedItems</param>
        /// <param name="plan">editingPlan或CokeRoom.PushPlan</param>
        /// <param name="isEditing">是否处于编辑计划状态</param>
        /// <returns>ItemsSource</returns>
        public static void DeleteTPlan(IList lst, List<TPushPlan> plan, bool isEditing, int area)
        {
            //删除计划分为编辑计划时删除和非编辑状态时删除
            //编辑计划时删除计划，只删除editingPlan
            //非编辑计划时删除计划，需删除CokeRoom.PushPlan和Config文件中的信息，Valid置0；
            IList editItems = lst;
            if (lst != null && lst.Count > 0)
            {
                //List<TPushPlan> lstPlan = new List<TPushPlan>();
                for (int i = 0; i < editItems.Count; i++)
                {
                    TPushPlan p = editItems[i] as TPushPlan;
                    int index = plan.FindIndex(x => x.RoomNum == p.RoomNum);
                    if (index >= 0)
                    {
                        plan.RemoveAt(index);
                    }
                }
                if (!isEditing)
                {
                    //非编辑状态下，推焦计划更新至Config文件：RoomPlanInfo.config；
                    string path = @"Config\RoomPlanInfo.config";
                    OperateConfig config = new OperateConfig(path);
                    for (int i = 0; i < editItems.Count; i++)
                    {
                        TPushPlan p = editItems[i] as TPushPlan;
                        //非编辑计划时,要更新Config中的Attribute--valid的值
                        config.SetPlanAttributeValue(p.RoomNum, "Valid", "0");
                    }
                    config.XmlDoc.Save(config.Path);
                    plan.Sort(TPushPlan.CompareByTime);
                }
            }
        }
        /// <summary>
        /// 删改装煤计划
        /// </summary>
        /// <param name="lst">DataGrid.SelectedItems</param>
        /// <param name="mPlan">装煤计划（editingMPlan或CokeRoom.StokingPlan）</param>
        /// <returns></returns>
        public static void DeleteMplan(IList lst, List<MStokingPlan> mPlan)
        {// mPlan是引用类型，在方法里的操作等同于在使用位置的操作
            IList mLst = lst;
            for (int i = 0; i < mLst.Count; i++)
            {
                TPushPlan p = mLst[i] as TPushPlan;
                int index = mPlan.FindIndex(x => x.RoomNum == p.RoomNum);
                if (index >= 0)
                {
                    mPlan.RemoveAt(index);
                }
            }
            mPlan.Sort(MStokingPlan.CompareByTime);
        }
        public static TPushPlan UpdateEditingPlan(int roomNum, int period, int group, DateTime dt)
        {
            TPushPlan plan = new TPushPlan();
            plan.RoomNum = (byte)roomNum;
            plan.Period = period;
            plan.Group = group;
            plan.PushTime = dt;
            return plan;
        }

    }
}
