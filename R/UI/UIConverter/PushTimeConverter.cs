using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WGPM.R.UI.UIConverter
{
    class PushTimeConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime time = (DateTime)value;
            return time.ToString("t");
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = (string)value;
            int h1 = Convert.ToInt32(s.Substring(0, 2));
            int m1 = Convert.ToInt32(s.Substring(3, 2));
            int hNow = DateTime.Now.Hour;
            int mNow = DateTime.Now.Minute;
            DateTime date = DateTime.Today;
            DateTime addDay = date.AddDays(1).AddHours(h1).AddMinutes(m1);
            DateTime notAddDay = date.AddHours(h1).AddMinutes(m1);
            if (hNow >= 8 && hNow < 20)
            {
                //8-20 NoDay;
                return (h1 >= 8) ? notAddDay : addDay;
            }
            if (hNow >= 20)
            {
                return h1 < 20 ? addDay : notAddDay;
            }
            else //hNow<8
            {
                return h1 >= 20 ? notAddDay.AddDays(-1) : notAddDay;
            }
        }
    }
}
