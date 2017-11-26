using System;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows;
using WGPM.R.OPCCommunication;
using WGPM.R.UI.UIConverter;
using System.Windows.Controls;

namespace WGPM.R.UI
{
    /// <summary>
    ///     title.xaml 的交互逻辑
    /// </summary>
    public partial class Title
    {
        public Title()
        {
            InitializeComponent();
            _Binding();
        }
        private void _Binding()
        {
            Binding myBinding = new Binding("DateTime");
            myBinding.Source = Communication.UITime;
            UITimeConverter converter = new UITimeConverter();
            converter.Date = true;
            myBinding.Converter = converter;
            txtTime.SetBinding(TextBox.TextProperty, myBinding);
        }
    }
    class SysTime : DependencyObject
    {
        public DateTime DateTime
        {
            get { return (DateTime)GetValue(DateTimeProperty); }
            set { SetValue(DateTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DateTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DateTimeProperty =
            DependencyProperty.Register("DateTime", typeof(DateTime), typeof(SysTime), new PropertyMetadata(DateTime.Now));


    }
}