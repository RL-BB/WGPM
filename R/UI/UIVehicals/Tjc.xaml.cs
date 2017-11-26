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
    /// Tjc.xaml 的交互逻辑
    /// </summary>
    public partial class Tjc : UserControl
    {
        public Tjc()
        {
            InitializeComponent();
            SetZIndex();
        }
        public string CarNum { get; set; }
        private void SetZIndex()
        {
            //Canvas.SetZIndex(elemnt,value)
            //给定 element 的 value 越大，element 在前景中出现的可能性就越大
            Canvas.SetZIndex(mark, 3);
            Canvas.SetZIndex(pushPole, 2);
            Canvas.SetZIndex(pingPole, 2);
            Canvas.SetZIndex(TjcBody, 1);
        }
    }
}
