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

namespace WGPM.R.UI.VehicalsData
{
    /// <summary>
    /// Node.xaml 的交互逻辑
    /// </summary>
    public partial class Node : UserControl
    {
        public Node()
        {
            InitializeComponent();
            Loaded += Node_Loaded;
        }
        public string NameModel { get; set; }
        public string Car1Model { get; set; }
        public string Car2Model { get; set; }
        private void Node_Loaded(object sender, RoutedEventArgs e)
        {
            nameModel.Name = NameModel;
            car1Model.Name = Car1Model;
            car2Model.Name = Car2Model;
        }
    }
}
