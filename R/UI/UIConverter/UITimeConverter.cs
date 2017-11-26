using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WGPM.R.UI.UIConverter
{
    class UITimeConverter : IValueConverter
    {
        /// <summary>
        /// 用来控制显示的是 yyyy-MM-dd HH:mm  还是HH:mm
        /// </summary>
        public bool Date { get; set; }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime time = (DateTime)value;
            return Date ? time.ToString("g") : time.ToString("t");
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
