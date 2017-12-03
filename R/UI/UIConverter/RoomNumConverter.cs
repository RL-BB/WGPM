using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using WGPM.R.Vehicles;

namespace WGPM.R.UI.UIConverter
{
    class RoomNumConverter : IValueConverter
    {
        public int CarNum { get; set; }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int room = (int)value;
            string strRoom = room.ToString("000");
            string car = (CarNum == 1 ? 1 : 2) + (Setting.AreaFlag ? 0 : 2) + "#\n";
            if (room > 0 && room <= 110)
            {
                return car + strRoom;
            }
            else
            {
                return car + "000";
            }
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class MoveConverter : IValueConverter
    {
        public int CarNum { get; set; }
        /// <summary>
        /// Tjc  DefaultMargin.Left=28,938
        /// Ljc  DefaultMargin.Left=28,938=84*2+770
        /// 数字42是根据TjcBody的Width=56，pushPole.Margin.Left=42 作对比得到的
        /// 数字889=42+84(煤塔.Width)+7*109(110#炭化室和1#炭化室的间隔数量)
        /// </summary>
        public double DefalutLeft { get; set; }
        public double DefaultTop { get; set; }
        //private Thickness margin = new System.Windows.Thickness(0, 320, 0, 0);
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //DefaultMargin
            Thickness margin = new Thickness(0, 0, 0, 0);
            margin.Top = DefaultTop;
            int roomNum = (int)value;
            if (CarNum == 0) CarNum = 1;
            if (roomNum == 0)
            {
                margin.Left = DefalutLeft;
            }
            else /*if (roomNum > 0 && roomNum <= 110)*/
            {
                if (roomNum <= 55)
                {
                    margin.Left = (CarNum == 1 ? DefalutLeft : (DefalutLeft - 84 - 110 * 7 - 7)) + roomNum * 7;
                }
                else
                {
                    //MT\DT.Width=84,Room.Width=7, 7*55 即单个炉区在UI上的长度
                    margin.Left = (CarNum == 1 ? (DefalutLeft + 84 + 55 * 7) : (DefalutLeft - 7 - 55 * 7)) + (roomNum - 55) * 7;
                }
            }
            return margin;
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 来自PLC的推杆和平杆的长度 转换为 UI界面上推杆和平杆的位移
    /// 把推杆的炉前暂停状态整合进来
    /// </summary>
    class PoleMoveConverter : IValueConverter
    {
        /// <summary>
        /// 炉前暂停的位移
        /// </summary>
        private double PauseDistance { get { return -10; } }
        public bool IsPushPole { get; set; }
        public bool Pause { get; set; }
        private Thickness pushPoleMargin = new Thickness(42, 0, 0, 0);
        private Thickness pingPoleMargin = new Thickness(7, 0, 0, 0);
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int poleLength = (int)value;
            Thickness margin = IsPushPole ? pushPoleMargin : pingPoleMargin;
            //6600是与PLC约定的推杆的最大长度；同理，2000是与PLC约定的平杆的最大长度
            margin.Top = -(IsPushPole ? 150 : 120) * poleLength / (double)(IsPushPole ? 6600 : 2000);
            //对炉前暂停的处理
            margin.Top = IsPushPole && Pause ? PauseDistance : margin.Top;
            return margin;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class TroughMoveConverter : IValueConverter
    {
        /// <summary>
        /// 导焦槽的最大位移为14
        /// 导焦槽信号锁闭后，导焦槽靠近至炭化室（topMargin=200）
        /// 导焦槽topMargin=130，导焦槽Height=56
        /// 14 =200-130-56；
        /// </summary>
        private int Distance { get { return 14; } }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isLocked = (bool)value;
            Thickness margin = new Thickness(24.5, 0, 0, 0);
            margin.Top = isLocked ? Distance : 0;
            return margin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class XjcMoveConverter : IValueConverter
    {
        public int CarNum { get; set; }
        /// <summary>
        /// Tjc  DefaultMargin.Left=28,938
        /// Ljc  DefaultMargin.Left=28,938=84*2+770
        /// 数字42是根据TjcBody的Width=56，pushPole.Margin.Left=42 作对比得到的
        /// 数字889=42+84(煤塔.Width)+7*109(110#炭化室和1#炭化室的间隔数量)
        /// </summary>
        public double DefalutLeft { get; set; }
        public double DefaultTop { get; set; }
        /// <summary>
        /// 干熄焦的焦罐 在车头的右侧
        /// </summary>
        public bool Right { get; set; }
        /// <summary>
        /// 电机车两个焦罐的偏差
        /// 其中靠近车头的干焦罐 为1#焦罐 fstCan
        /// </summary>
        public double Deviation { get; set; }
        public bool Dry { get; set; }
        public bool CanNum { get; set; }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness margin = new Thickness(0, 0, 0, 0);
            margin.Top = DefaultTop;
            XjcMoveHelper helper = (XjcMoveHelper)value;
            if (CarNum == 0) CarNum = 1;
            if (helper.RoomNum == 0)
            {
                margin.Left = DefalutLeft;
            }
            else /*if (roomNum > 0 && roomNum <= 110)*/
            {
                if (helper.RoomNum <= 55)
                {
                    margin.Left = (CarNum == 1 ? DefalutLeft : (DefalutLeft - 84 - 110 * 7 - 7)) + helper.RoomNum * 7;
                }
                else
                {
                    //MT\DT.Width=84,Room.Width=7, 7*55 即单个炉区在UI上的长度
                    margin.Left = (CarNum == 1 ? (DefalutLeft + 84 + 55 * 7) : (DefalutLeft - 7 - 55 * 7)) + (helper.RoomNum - 55) * 7;
                }
            }
            margin.Left = helper.Dry && helper.CanNum ? (margin.Left + helper.Deviation) : margin.Left;
            return margin;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
