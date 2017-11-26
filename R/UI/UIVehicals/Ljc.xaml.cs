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

namespace WGPM.R.UI.UIVehicals
{
    /// <summary>
    /// Ljc.xaml 的交互逻辑
    /// </summary>
    public partial class Ljc : UserControl
    {
        public Ljc()
        {
            InitializeComponent();
            SetZIndex();
        }
        private void SetZIndex()
        {
            Canvas.SetZIndex(body, 1);
            Canvas.SetZIndex(trough, 2);
            Canvas.SetZIndex(mark, 3);
        }
    }
}
