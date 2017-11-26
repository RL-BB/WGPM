using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace WGPM.R.UI.UIConverter
{
    class IntToColorConverter : IValueConverter
    {
        public Brush DefaultColor { get; set; }
        //public Brush TrueToColor { get; set; }
        public Brush TrueToColor { get { return Brushes.Red; } }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ushort arrows = (ushort)value;
            bool flag = Convert.ToBoolean(arrows);
            return !flag ? TrueToColor : DefaultColor;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}