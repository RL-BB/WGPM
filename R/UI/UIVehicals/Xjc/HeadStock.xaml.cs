using System;
using System.Collections.Generic;
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

namespace WGPM.R.UI.UIVehicals.Xjc
{
    /// <summary>
    /// Body.xaml 的交互逻辑
    /// 熄焦车的车头
    /// </summary>
    public partial class HeadStock : UserControl
    {
        public HeadStock()
        {
            InitializeComponent();
        }
        private void SetZIndex()
        {
            Canvas.SetZIndex(body, 1);
            Canvas.SetZIndex(mark, 2);
        }
    }
}
