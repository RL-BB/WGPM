using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WGPM.R.Sys.SysParms
{
    class SysParms
    {
        public static int UITag { get; set; }
        /// <summary>
        ///     自适应最大化的情况,一般窗口最大化后不变就用这个
        /// </summary>
        /// <param name="x">设计窗口时的grid</param>
        /// <param name="width">grid的width</param>
        /// <param name="height">grid的height</param>
        public static void SizeChangeFunction(Grid x, Double width, Double height)
        {
            double d1 = SystemParameters.PrimaryScreenWidth / (width);
            double d2 = SystemParameters.PrimaryScreenHeight / (height);

            x.LayoutTransform = new ScaleTransform { ScaleX = d1, ScaleY = d2, CenterX = 0, CenterY = 0 };
        }
    }
}
