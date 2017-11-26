using System.Windows;
using WGPM.R.Sys.SysParms;

namespace WGPM
{
    public partial class Windows
    {
        public Windows()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SysParms.SizeChangeFunction(MainGrid, 1024, 768);
        }
    }
}