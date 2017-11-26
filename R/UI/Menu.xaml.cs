using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WGPM.R.Sys.SysParms;

namespace WGPM.R.UI
{
    /// <summary>
    ///     Menu.xaml 的交互逻辑
    /// </summary>
    public partial class Menu
    {
        public Menu()
        {
            InitializeComponent();
            MainGrid_MouseLeave(null, null);
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            SysParms.UITag = Convert.ToInt32(btn.Tag);
        }
        private void MainGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            MainGrid.Opacity = 1;
            MainGrid.Margin = new Thickness(0, 0, 0, 0);
        }
        private void MainGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            MainGrid.Opacity = 0;
            MainGrid.Margin = new Thickness(0, -90, 0, 0);
        }
    }
}