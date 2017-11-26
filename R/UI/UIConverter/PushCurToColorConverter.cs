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

    class PushCurConverter : IValueConverter
    {
        public int FstLevel
        {
            get { return 250; }
            set { }
        }
        public int SecLevel { get { return 220; } }
        public Brush FstLevelColor { get { return Brushes.Red; } }
        public Brush SecLevelColor { get { return Brushes.Green; } }
        public Brush DefaultColor { get; set; }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int cur = (int)value;
            if (cur >= FstLevel)
            {
                return FstLevelColor;
            }
            else if (cur >= SecLevel)
            {
                return SecLevelColor;
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

