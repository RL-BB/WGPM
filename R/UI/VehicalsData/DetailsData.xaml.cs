using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WGPM.R.OPCCommunication;
using WGPM.R.Vehicles;
using WGPM.R.CommHelper;
using System.Windows.Threading;

namespace WGPM.R.UI.VehicalsData
{

    /// <summary>
    /// T.xaml 的交互逻辑
    /// </summary>
    public partial class DetailsData : UserControl
    {
        public DetailsData()
        {
            InitializeComponent();
            Loaded += T_Loaded;
        }

        private void T_Loaded(object sender, RoutedEventArgs e)
        {
            CboItems();
            Headers();
            RowItems();
            Update();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        DispatcherTimer timer = new DispatcherTimer();
        private void CboItems()
        {
            ComboBoxItem[] items = new ComboBoxItem[]
            {
                new ComboBoxItem() {IsSelected=true },
                new ComboBoxItem()
            };
            items[0].Content = (Setting.AreaFlag ? 1 : 3) + "#推焦车";
            items[0].Content = (Setting.AreaFlag ? 2 : 4) + "#推焦车";
            cbo.Items.Add(items);
        }
        DataRow[] rows = new DataRow[30];
        DataTable dt;
        /// <summary>
        /// 0:T,1:L;
        /// 2:X,3:M
        /// </summary>
        public int CarTypye { get; set; }
        private void Headers()
        {
            dt = new DataTable();
            dt.Columns.Add("Items");
            dt.Columns.Add("Data");
        }
        private void RowItems()
        {
            for (int i = 0; i < rows.Length; i++)
            {
                dt.Rows.Add(rows[i]);
            }
            int index = 0;
            rows[index++][0] = "炉号";//1
            rows[index++][0] = "实时物理地址";//2
            rows[index++][0] = "中心物理地址";//3
            rows[index++][0] = "MainWireless";//4
            rows[index++][0] = "CommWireless";//5
            rows[index++][0] = "CommPLC";//6
            rows[index++][0] = "CommDecode";//7
            rows[index++][0] = "CommTouch";//8
            rows[index++][0] = "CountPLC";//9
            rows[index++][0] = "CountDecode";//10
            rows[index++][0] = "CountTouch";//11

        }
        private void TRowItems()
        {
            int index = 11;
            rows[index++][0] = "PushPoleLength";//
            rows[index++][0] = "PingPoleLength";//
            rows[index++][0] = "PushCur";//
            rows[index++][0] = "PingCur";//
            rows[index++][0] = "炉门已摘";//
            rows[index++][0] = "推焦联锁";//
            rows[index++][0] = "摘门联锁";//
            rows[index++][0] = "推焦开始";//
            rows[index++][0] = "推焦结束";//
            rows[index++][0] = "平煤开始";//
            rows[index++][0] = "平煤结束";//
            rows[index++][0] = "推焦杆前进";//
            rows[index++][0] = "炉前暂停";//
            rows[index++][0] = "推焦完成标志";//
            rows[index][0] = "小炉门已摘";//
        }
        private void LRowItems()
        {
            int index = 11;
            rows[index++][0] = "人工允推";//
            rows[index++][0] = "炉门已摘";//
            rows[index++][0] = "焦槽锁闭";//
            rows[index++][0] = "摘门联锁";//
            rows[index][0] = "禁止推焦";//

        }
        private void XRowItems()
        {
            int index = 11;
            rows[index++][0] = "人工允推";//
            rows[index++][0] = "CanReady";//
            rows[index++][0] = "罐号";//
            rows[index++][0] = "水熄/干熄";//
            rows[index++][0] = "禁止推焦";//
            rows[index++][0] = "1#罐有无";//
            rows[index][0] = "1#罐有无";//
        }
        private void MRowItems()
        {
            int index = 11;
            rows[index++][0] = "装煤联锁";//
            rows[index++][0] = "请求平煤";//
            rows[index++][0] = "内导套到位";//
            rows[index++][0] = "炉盖打开";//
            rows[index++][0] = "闸板开";//
            rows[index++][0] = "除尘就绪";//
            rows[index++][0] = "装煤开始";//
            rows[index++][0] = "装煤结束";//
            rows[index++][0] = "取盖连锁";//
            rows[index][0] = "煤车变频器";//
        }
        private void Update()
        {
            int index = 0;
            bool flag = cbo.SelectedIndex == 0;
            int carIndex = 2 * CarTypye + (flag ? 0 : 1);
            Tjc t = (Tjc)Communication.CarsInfo[carIndex];
            rows[0][index++] = t.RoomNum;//
            TjcDataRead dr = (TjcDataRead)t.DataRead;
            rows[0][index++] = dr.PhysicalAddr;//
            rows[0][index++] = Communication.CommLst[0].CommStatus[CarTypye];//地面无线模块
            //索引值0-3对应IP分配表的设备顺序：PLC，触摸屏，无线模块，解码器（或TLXM四个无线模块）
            bool[] comm = Communication.CommLst[flag ? 1 : 2].CommStatus;
            rows[0][index++] = comm[2];//
            rows[0][index++] = comm[0];//
            rows[0][index++] = comm[1];//
            rows[0][index++] = comm[3];//
            rows[0][index++] = dr.PLCCount;//
            rows[0][index++] = dr.DecodeCount;//
            rows[0][index++] = dr.TouchCount;//

            rows[0][index++] = dr.PushPoleLength;//
            rows[0][index++] = dr.PingPoleLength;//
            rows[0][index++] = dr.PushCur;//
            rows[0][index++] = dr.PingCur;//
            TjcTogetherInfo info = (TjcTogetherInfo)t.DataRead.TogetherInfo;
            rows[0][index++] = info.TRoomDoorOpen;//
            rows[0][index++] = info.PushTogether;//
            rows[0][index++] = info.PickDoorTogether;//
            rows[0][index++] = info.PushBegin;//
            rows[0][index++] = info.PushEnd;//
            rows[0][index++] = info.PingBegin;//
            rows[0][index++] = info.PingEnd;//
            rows[0][index++] = info.PushPoleForward;//
            rows[0][index++] = info.PushPolePause;//
            rows[0][index++] = info.PushEndFlag;//
            rows[0][index++] = info.TMDoorOpen;//
            dg.ItemsSource = null;
            dg.ItemsSource = dt.DefaultView;
        }
    }
}
