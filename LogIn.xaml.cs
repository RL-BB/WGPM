using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WGPM.R.LinqHelper;
using WGPM.R.UI;

namespace WGPM
{
    /// <summary>
    /// LogIn.xaml 的交互逻辑
    /// </summary>
    public partial class LogIn : Window
    {
        public LogIn()
        {
            InitializeComponent();
            Loaded += LogIn_Loaded;
        }
        private string IpLocal { get { return GetLocalIP(); } }
        private string IpArea12 { get { return "192.168.0.3"; } }
        private string IpArea34 { get { return "192.168.0.4"; } }
        private void LogIn_Loaded(object sender, RoutedEventArgs e)
        {
            string ip = GetLocalIP();
            //1、2#炉区"192.168.0.3"
            char ipLetter = ip.Last();
            if (ipLetter == '4')
            {//字符4的意义：ip地址的最后一位 
                cboCokeArea.SelectedIndex = 1;
            }
            //cboCokeArea.SelectedIndex = IpLocal == IpArea34 ? 1 : 0;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Setting.AreaFlag = cboCokeArea.SelectedIndex == 0 ? true : false;
            MainWindow mw = new MainWindow();
            mw.Show();
            this.Close();
        }
       
        private string GetLocalIP()
        {
            string ip = string.Empty;
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            for (int i = 0; i < ips.Length; i++)
            {
                if (ips[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ip = ips[i].ToString();
                    break;
                }
            }
            return ip;
        }
    }
}
