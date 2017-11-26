using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WGPM.R.OPCCommunication;
using WGPM.R.RoomInfo;
using WGPM.R.Parms;
using System.Windows;

namespace WGPM.R.Vehicles
{
    class Ljc : Vehicle
    {
        public Ljc(ushort carNum)
        {
            base.CarNum = carNum;
            addrDic = Addrs.LAddrDic;
            GetArrows = GetLArrows;
            togetherInfoCount = 5;//拦焦车的联锁信息数据有5位
            DataRead = new LjcDataRead();
            DataRead.TogetherInfo = new LjcTogetherInfo(TogetherInfoCount);
            DataRead.TogetherInfo.DecodeTogetherInfo = ((LjcTogetherInfo)DataRead.TogetherInfo).DecodeTogetherInfoValue;
        }
        public ushort GetLArrows()
        {
            if (CokeRoom.PushPlan.Count != 0)
            {
                //计划炉号的中心地址
                int middle = Addrs.LRoomNumDic[CokeRoom.PushPlan[0].RoomNum];
                int actual = DataRead.PhysicalAddr;//当前车的位置
                if (middle - actual > StaticParms.LFstArrow)
                {//左俩箭头:1100,在另一块码牌上
                    Arrows = 3;
                }
                else if ((middle - actual <= StaticParms.LFstArrow) && (middle - actual > StaticParms.LArrow))
                {//左单箭头 两块码牌之间
                    Arrows = 2;
                }
                else if (Math.Abs(middle - actual) <= StaticParms.LArrow)
                {//对中 在码牌上
                    Arrows = 0;
                }
                else if ((middle - actual < -StaticParms.LArrow) && (middle - actual >= -StaticParms.LFstArrow))
                {//右单箭头
                    Arrows = 4;
                }
                else if (middle - actual < -StaticParms.LFstArrow)
                {//右俩箭头
                    Arrows = 12;
                }
            }
            return Arrows;
        }

    }
    class LjcDataRead : DataRead
    {
        public LjcDataRead()
        {
            DecodeDataReadValue = DecodeDataRead;
        }
    }
    class LjcTogetherInfo : TogetherInfo
    {
        public LjcTogetherInfo(int ljcTogetherInfoCount) : base(ljcTogetherInfoCount) { }
        /// <summary>
        /// 人工允推
        /// </summary>
        public bool LAllowPush { get { return allowPush; } }
        private bool allowPush;
        /// <summary>
        /// 炉门已摘
        /// </summary>
        public bool LRoomDoorOpen { get { return roomDoorOpen; } }
        private bool roomDoorOpen;
        /// <summary>
        /// 焦槽锁闭
        /// </summary>
        public bool TroughLocked
        {
            get { return (bool)GetValue(TroughLockedProperty); }
            set { SetValue(TroughLockedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TroughLocked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TroughLockedProperty =
            DependencyProperty.Register("TroughLocked", typeof(bool), typeof(LjcTogetherInfo), new PropertyMetadata(false));
        /// <summary>
        /// 摘门联锁
        /// </summary>
        public bool PickDoorTogether { get { return pickDoorTogether; } }
        private bool pickDoorTogether;
        /// <summary>
        /// 禁止推焦
        /// </summary>
        public bool UnallowPush { get { return allowPush; } }
        private bool unallowPush;
        /// <summary>
        /// 给List<bool> propertyValue赋值后调用此方法；
        /// </summary>
        public void DecodeTogetherInfoValue()
        {
            byte listIndex = 0;
            if (ToDecodeTogetherInfo.Count > 0)
            {
                allowPush = ToDecodeTogetherInfo[listIndex++];
                roomDoorOpen = ToDecodeTogetherInfo[listIndex++];
                TroughLocked = ToDecodeTogetherInfo[listIndex++];
                pickDoorTogether = ToDecodeTogetherInfo[listIndex++];
                unallowPush = ToDecodeTogetherInfo[listIndex++];
            }
        }
    }
}
