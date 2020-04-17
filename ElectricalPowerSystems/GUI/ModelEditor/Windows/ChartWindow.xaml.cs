using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace ElectricalPowerSystems.GUI.ModelEditor.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window, INotifyPropertyChanged
    {
        private ZoomingOptions _zoomingMode;
        public ZoomingOptions ZoomingMode
        {
            get { return _zoomingMode; }
            set
            {
                _zoomingMode = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        public SeriesCollection SeriesCollection
        {
            get;
            set;
        }
        public ChartWindow()
        {
            InitializeComponent();
            SeriesCollection = new SeriesCollection();
            ZoomingMode = ZoomingOptions.X;
            DataContext = this;
        }
        public void AddLineSeries(double[] x, double[] t,string title)
        {
            List<ScatterPoint> list = new List<ScatterPoint>();
            for (int i = 0; i < t.Length; i++)
                list.Add(new ScatterPoint(t[i], x[i]));
            SeriesCollection.Add(new LineSeries {
                Title = title,
                Values = new ChartValues<ScatterPoint>(list),
                LineSmoothness = 0
            });
        }
        public void Plot(List<double[]> values, List<double> time, string[] variables)
        {
            for (int i = 0; i < variables.Length; i++)
            {
                List<ScatterPoint> list = new List<ScatterPoint>();
                for (int j = 0; j < time.Count; j++)
                    list.Add(new ScatterPoint(time[j], values[j][i]));
                SeriesCollection.Add(new LineSeries
                {
                    Title = variables[i],
                    Values = new ChartValues<ScatterPoint>(list),
                    LineSmoothness = 0,
                    StrokeThickness=1.0,
                    PointGeometry =null,
                    Fill = Brushes.Transparent
                });
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void ToogleZoomingMode(object sender, RoutedEventArgs e)
        {
            switch (ZoomingMode)
            {
                case ZoomingOptions.None:
                    ZoomingMode = ZoomingOptions.X;
                    break;
                case ZoomingOptions.X:
                    ZoomingMode = ZoomingOptions.Y;
                    break;
                case ZoomingOptions.Y:
                    ZoomingMode = ZoomingOptions.Xy;
                    break;
                case ZoomingOptions.Xy:
                    ZoomingMode = ZoomingOptions.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void ResetZoomOnClick(object sender, RoutedEventArgs e)
        {
            //Use the axis MinValue/MaxValue properties to specify the values to display.
            //use double.Nan to clear it.

            XAxis.MinValue = double.NaN;
            XAxis.MaxValue = double.NaN;
            YAxis.MinValue = double.NaN;
            YAxis.MaxValue = double.NaN;
        }

        private void LegendListBox_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(LegendListBox, (DependencyObject)e.OriginalSource) as ListBoxItem;
            if (item == null) return;

            var series = (LineSeries)item.Content;
            series.Visibility = series.Visibility == Visibility.Visible
                ? Visibility.Hidden
                : Visibility.Visible;
        }
    }
    public class OpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible
                ? 1d
                : .2d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
