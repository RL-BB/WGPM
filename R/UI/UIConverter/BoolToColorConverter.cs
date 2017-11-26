using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WGPM.R.UI.UIConverter
{
    class BoolToColorConverter : IValueConverter
    {
        public Brush DefaultColor { get; set; }
        //public Brush TrueToColor { get; set; }
        public Brush TrueToColor { get; set; }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = (bool)value;
            return flag ? TrueToColor : DefaultColor;
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
