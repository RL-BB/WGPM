using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WGPM.R.RoomInfo;

namespace WGPM.R.UI
{
    /// <summary>
    /// BurnStatus.xaml 的交互逻辑
    /// </summary>
    public partial class BurnStatus : UserControl
    {
        public int Area;
        private TextBox[] rooms = new TextBox[55];
        private DispatcherTimer timer = new DispatcherTimer();
        public BurnStatus()
        {

            InitializeComponent();
            Loaded += BurnStatus_Loaded;
        }

        private void BurnStatus_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 55; i++)
            {
                rooms[i] = new TextBox
                {
                    Height = 85,
                    Width = 970 / (55 * 1.5),
                    RenderTransformOrigin = new Point(0, 0),
                    RenderTransform = new RotateTransform { Angle = 180 },
                    Effect = new DropShadowEffect { Direction = 510 },
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Background = Brushes.Blue,
                    Foreground = Brushes.White,
                    Cursor = Cursors.Hand,
                    BorderBrush = Brushes.White,
                    IsTabStop = false,
                    IsReadOnly = true,
                    Tag = ((Area - 1) * 55 + i + 1).ToString(CultureInfo.InvariantCulture)
                };
                ToolTipService.SetToolTip(rooms[i], (Area - 1) * 55 + i + 1 + "#");
                Canvas.SetTop(rooms[i], 196);
                Canvas.SetLeft(rooms[i], 45 + (i + 1) * 970 / (55 + 1));
                CanvasMain.Children.Add(rooms[i]);
                rooms[i].MouseEnter += BurnStatus_MouseEnter;
                rooms[i].MouseLeave += BurnStatus_MouseLeave;
            }
            TimeSpan span1 = TimeSpan.FromMinutes(19 * 60);//由结焦时间得来
            TimeSpan span2 = TimeSpan.FromMinutes(19 * 60 / 2);
            txtTime.Text = (span1.Days * 24 + span1.Hours).ToString("00") + ":" + span1.Minutes.ToString("00");
            txtHalfTime.Text = (span2.Days * 24 + span2.Hours).ToString("00") + ":" + span2.Minutes.ToString("00");
            txtRoom.Visibility = Visibility.Hidden;

            timer.Start();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(5);
            //Timer_Tick(null, null);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 55; i++)
            {
                int num1 = i + 1 + 55 * (Area - 1);
                rooms[i].Background = CokeRoom.GetRoomsColor(CokeRoom.BurnStatus[num1].BurnStatus);
                rooms[i].Height = CokeRoom.GetRoomsHeight(CokeRoom.BurnStatus[num1].BurnStatus);
            }
        }

        private void BurnStatus_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null)
            {
                txt.BorderBrush = Brushes.White;
            }
        }

        private void BurnStatus_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt == null)
            {
                return;
            }
            txt.BorderBrush = Brushes.LightBlue;
            txtRoomNum.Text = txt.Tag.ToString();
            int num = Convert.ToInt32(txt.Tag);
            TimeSpan span = TimeSpan.FromMinutes((DateTime.Now - CokeRoom.BurnStatus[num].StokingTime).TotalMinutes);
            txtBurnTime.Text = (span.Days * 24 + span.Hours).ToString("00") + ":" + span.Minutes.ToString("00");
            txtStokingTime.Text = CokeRoom.BurnStatus[num].StokingTime.ToString("MM-dd HH:mm");
        }
    }
}
