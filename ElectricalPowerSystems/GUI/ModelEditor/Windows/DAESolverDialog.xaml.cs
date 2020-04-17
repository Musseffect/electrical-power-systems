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
using System.Windows.Shapes;

namespace ElectricalPowerSystems.GUI.ModelEditor.Windows
{

    enum METHOD
    {
        RADAUIIA3 = 0,
        RADAUIIA5 =1,
        BDF = 2,
        TRAPEZOID = 3
    }
    /// <summary>
    /// Логика взаимодействия для DAESolverDialog.xaml
    /// </summary>
    public partial class DAESolverDialog : Window
    {
        public int Iterations { get { return (int)IterationsInput.Value; } }
        public double Step { get { return StepInput.Value.Value; } }
        public double FAbsTol { get { return FAbsTolInput.Value.Value; } }
        public int SelectedMethod { get { return MethodInput.SelectedIndex; }  }
        public double Alpha { get { return AlphaInput.Value.Value; } }
        public DAESolverDialog()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
