using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
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

namespace WGPM.R.UI
{
    /// <summary>
    /// CurLotterForQuery.xaml 的交互逻辑
    /// </summary>
    public partial class CurLotterForQuery : UserControl
    {
        public CurLotterForQuery()
        {
            InitializeComponent();
            Loaded += CurPlotter_Loaded;
        }
        public delegate void PlotterPushCurDelegate();
        public delegate void PlotterPingCurDeleggate();
        public PlotterPushCurDelegate Pushing;
        public PlotterPingCurDeleggate Pinging;
        public bool plotterPush;
        public bool plotterPing;
        public int TCarIndex { get; set; }
        //推焦电流和杆长
        public ObservableDataSource<Point> pushPoleDS = new ObservableDataSource<Point>();
        public ObservableDataSource<Point> pushCurDS = new ObservableDataSource<Point>();
        //平煤电流和杆长
        ObservableDataSource<Point> pingPoleDS = new ObservableDataSource<Point>();
        ObservableDataSource<Point> pingCurDS = new ObservableDataSource<Point>();
        LineGraph pushPoleGraph;
        LineGraph pushCurGraph;
        LineGraph pingPoleGraph;
        LineGraph pingCurGraph;
        List<Point> pushPoleArr;
        List<Point> pushCurArr;
        List<Point> pingPoleArr;
        List<Point> pingCurArr;
        private void CurPlotter_Loaded(object sender, RoutedEventArgs e)
        {
            //pushPoleGraph = plotter.AddLineGraph(pushPoleDS, Colors.Green, 2, "推焦杆长");
            //pushCurGraph = plotter.AddLineGraph(pushCurDS, Colors.Red, 2, "推焦电流");
            //pingPoleGraph = plotter.AddLineGraph(pingPoleDS, Colors.Yellow, 2, "平煤杆长");
            //pingCurGraph = plotter.AddLineGraph(pingCurDS, Colors.Black, 2, "平煤电流");
            plotter.Viewport.FitToView();
        }
        private void Push()
        {
            plotter.Children.Remove(pushCurGraph);
            plotter.Children.Remove(pushPoleGraph);
            pushCurDS = new ObservableDataSource<Point>(pushCurArr);
            pushPoleDS = new ObservableDataSource<Point>(pushPoleArr);
            pushPoleGraph = plotter.AddLineGraph(pushPoleDS, Colors.Green, 2, "推焦杆长");
            pushCurGraph = plotter.AddLineGraph(pushCurDS, Colors.Red, 2, "推焦电流");
            plotter.Viewport.FitToView();
            pushPoleDS.SetXYMapping(p => new Point(p.X / 5, p.Y));//数字5的意义：1000ms，记录电流的timer的Interval为200ms，即5次计数为1s，X轴的单位为秒
            pushCurDS.SetXYMapping(p => new Point(p.X / 5, p.Y));
            pushCurGraph.Cursor = Cursors.Hand;
        }
        private void Ping()
        {
            plotter.Children.Remove(pingPoleGraph);
            plotter.Children.Remove(pingCurGraph);
            pingPoleDS = new ObservableDataSource<Point>(pingPoleArr);
            pingCurDS = new ObservableDataSource<Point>(pingCurArr);
            pingPoleGraph = plotter.AddLineGraph(pingPoleDS, Colors.Yellow, 2, "平煤杆长");
            pingCurGraph = plotter.AddLineGraph(pingCurDS, Colors.Black, 2, "平煤电流");
            plotter.Viewport.FitToView();

            pingPoleDS.SetXYMapping(p => new Point(p.X / 5, p.Y));//数字5的意义：1000ms，记录电流的timer的Interval为200ms，即5次计数为1s，X轴的单位为秒
            pingCurDS.SetXYMapping(p => new Point(p.X / 5, p.Y));
            pingCurGraph.Cursor = Cursors.Hand;
        }
        private void GetPoint(string pushPole, string pushCur, string pingPole, string pingCur)
        {
            bool pushCurFlag = pushPole == null || pushCur == null ? false : true;
            bool pingCurFlag = pingPole == null || pingCur == null ? false : true;
            pushPoleArr = new List<Point>();
            pushCurArr = new List<Point>();
            if (pushCurFlag)
            {
                byte[] psPole = Convert.FromBase64String(pushPole);
                byte[] psCur = Convert.FromBase64String(pushCur);

                for (int i = 0; i < psPole.Length; i++)
                {
                    Point p = new Point(i + 1, psPole[i]);
                    pushPoleArr.Add(p);
                    if (psCur.Length > psPole.Length) continue;
                    Point c = new Point(i + 1, psCur[i]);
                    pushCurArr.Add(c);
                }
            }
            pingPoleArr = new List<Point>();
            pingCurArr = new List<Point>();
            if (pingCurFlag)
            {
                byte[] pgPole = Convert.FromBase64String(pingPole);
                byte[] pgCur = Convert.FromBase64String(pingCur);
                for (int i = 0; i < pgPole.Length; i++)
                {
                    Point p = new Point(i + 1, pgPole[i]);
                    pingPoleArr.Add(p);
                    if (pgCur.Length > pgPole.Length) continue;
                    Point c = new Point(i + 1, pgCur[i]);
                    pingCurArr.Add(c);
                }
            }
        }
        public void Plotter(string pushPole, string pushCur, string pingPole, string pingCur)
        {
            GetPoint(pushPole, pushCur, pingPole, pingCur);
            Push();
            Ping();
        }
    }
}
