using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ElectricalPowerSystems
{
    /// <summary>
    /// Логика взаимодействия для ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        public SeriesCollection SeriesCollection { get; set; }
        public ChartWindow()
        {
            InitializeComponent();
            SeriesCollection = new SeriesCollection();
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
        public void AddMultiple(List<double[]> values, List<double> time, string[] variables)
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
                    LineSmoothness = 0
                });
            }
        }
    }
}
