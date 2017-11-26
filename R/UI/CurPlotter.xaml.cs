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
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Windows.Threading;
using WGPM.R.OPCCommunication;
using WGPM.R.Vehicles;

namespace WGPM.R.UI
{
    /// <summary>
    /// CurPlotter.xaml 的交互逻辑
    /// </summary>
    public partial class CurPlotter : UserControl
    {
        public CurPlotter()
        {
            InitializeComponent();
            Loaded += CurPlotter_Loaded;
        }
        public delegate void PlotterPushCurDelegate();
        public delegate void PlotterPingCurDeleggate();
        public PlotterPushCurDelegate Pushing;
        public PlotterPingCurDeleggate Pinging;
        Tjc T;
        public bool NextPush;
        public bool NextPing;
        public int TCarIndex { get; set; }
        DispatcherTimer plotterTimer = new DispatcherTimer();
        //推焦电流和杆长
        ObservableDataSource<Point> pushPoleDS = new ObservableDataSource<Point>();
        ObservableDataSource<Point> pushCurDS = new ObservableDataSource<Point>();
        //平煤电流和杆长
        ObservableDataSource<Point> pingPoleDS = new ObservableDataSource<Point>();
        ObservableDataSource<Point> pingCurDS = new ObservableDataSource<Point>();
        LineGraph pushPoleGraph;
        LineGraph pushCurGraph;
        LineGraph pingPoleGraph;
        LineGraph pingCurGraph;
        private void CurPlotter_Loaded(object sender, RoutedEventArgs e)
        {
            T = (Tjc)Communication.CarsInfo[TCarIndex];

            pushPoleGraph = plotter.AddLineGraph(pushPoleDS, Colors.Green, 2, "推焦杆长");
            pushCurGraph = plotter.AddLineGraph(pushCurDS, Colors.Red, 2, "推焦电流");
            pingPoleGraph = plotter.AddLineGraph(pingPoleDS, Colors.Yellow, 2, "平煤杆长");
            pingCurGraph = plotter.AddLineGraph(pingCurDS, Colors.Black, 2, "平煤电流");
            plotter.Viewport.FitToView();
            plotterTimer.Tick += PlotterTimer_Tick;
            plotterTimer.Interval = TimeSpan.FromMilliseconds(200);
            plotterTimer.Start();
        }
        private void PlotterTimer_Tick(object sender, EventArgs e)
        {
            Pushing = T.Pushing ? (PlotterPushCurDelegate)StartPush : EndPush;
            Pinging = T.Pinging ? (PlotterPingCurDeleggate)StartPing : EndPing;
            Pushing();
            Pinging();
        }
        private void StartPush()
        {
            if (NextPush)
            {
                NextPush = false;
                plotter.Children.Remove(pushPoleGraph);
                plotter.Children.Remove(pushCurGraph);
                pushCurDS = new ObservableDataSource<Point>();
                pushPoleDS = new ObservableDataSource<Point>();
                pushPoleGraph = plotter.AddLineGraph(pushPoleDS, Colors.Green, 2, "推焦杆长");
                pushCurGraph = plotter.AddLineGraph(pushCurDS, Colors.Red, 2, "推焦电流");
                plotter.Viewport.FitToView();
            }
            pushPoleDS.AppendAsync(Dispatcher, T.PushLenPoint);
            pushPoleDS.SetXYMapping(p => new Point(p.X / 5, p.Y));//数字5的意义：1000ms，记录电流的timer的Interval为200ms，即5次计数为1s，X轴的单位为秒
            pushCurDS.AppendAsync(Dispatcher, T.PushCurPoint);
            pushCurDS.SetXYMapping(p => new Point(p.X / 5, p.Y));
            ToolTipService.SetToolTip(pushCurGraph, "最大推焦电流:" + T.PushCurMax + ",平均推焦电流:" + T.PushCurAvg);
            pushCurGraph.Cursor = Cursors.Hand;
        }
        private void EndPush()
        {
            NextPush = true;
        }
        private void StartPing()
        {
            if (NextPing)
            {
                NextPing = false;
                plotter.Children.Remove(pingPoleGraph);
                plotter.Children.Remove(pingCurGraph);
                pingPoleDS = new ObservableDataSource<Point>();
                pingCurDS = new ObservableDataSource<Point>();
                pingPoleGraph = plotter.AddLineGraph(pingPoleDS, Colors.Yellow, 2, "平煤杆长");
                pingCurGraph = plotter.AddLineGraph(pingCurDS, Colors.Black, 2, "平煤电流");
                plotter.Viewport.FitToView();
            }
            pingPoleDS.AppendAsync(Dispatcher, T.PingLenPoint);
            pingPoleDS.SetXYMapping(p => new Point(p.X / 5, p.Y));//数字5的意义：1000ms，记录电流的timer的Interval为200ms，即5次计数为1s，X轴的单位为秒
            pingCurDS.AppendAsync(Dispatcher, T.PingCurPoint);
            pingCurDS.SetXYMapping(p => new Point(p.X / 5, p.Y));
            ToolTipService.SetToolTip(pingCurGraph, "最大平煤电流:" + T.PingCurMax + ",平均平煤电流:" + T.PingCurAvg);
            pingCurGraph.Cursor = Cursors.Hand;
        }
        private void EndPing()
        {
            NextPing = true;
        }
    }
}
