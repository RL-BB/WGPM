using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WGPM.R.UI.UIConverter
{
    class CanReadyTextConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool carNum = (bool)value;//false 对应1#、3#；true 对应2#，4#
            return Setting.AreaFlag ? (carNum? "车门关闭" : "焦罐旋转") : (carNum ? "焦罐旋转" : "车门关闭");
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
