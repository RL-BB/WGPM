using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace WGPM.R.UI.UIConverter
{
    class TimeToStringConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int BurnTime = (int)value;
            return (BurnTime / 60).ToString("00") + ":" + (BurnTime % 60).ToString("00");
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = (string)value;
            if (s.Length == 5)
            {
                return Convert.ToInt32(s.Substring(0, 2)) * 60 + Convert.ToInt32(s.Substring(3, 2));
            }
            else
            {
                return "00:00";
            }
        }
    }
    class BurnTimeToColorConverter : IValueConverter
    {
        public int ValidBoundary { get { return Setting.BurnTime + 60 * 4; } }
        public Brush FstLevelColor { get { return Brushes.Red; } }
        public Brush DefaultColor { get; set; }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int t = (int)value;
            if (t >= ValidBoundary)
            {
                return FstLevelColor;
            }
            else
            {
                return DefaultColor;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
