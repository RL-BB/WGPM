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
    class ArrowsColorConverter : IValueConverter
    {
        public Brush DefaultColor { get; set; }
        public int Index { get; set; }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ushort arrows = (ushort)value;
            bool flag = Convert.ToBoolean(arrows & (ushort)Math.Pow(2, Index));
            return flag ? Brushes.Red : DefaultColor;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
