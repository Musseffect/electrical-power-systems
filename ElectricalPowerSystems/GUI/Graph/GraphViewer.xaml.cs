using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ElectricalPowerSystems.GUI
{
    /// <summary>
    /// Логика взаимодействия для GraphViewer.xaml
    /// </summary>
    public partial class GraphViewer : UserControl
    {
        public GraphViewer()
        {
            InitializeComponent();
        }
        private void NodeDrag(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            if (thumb == null)
                return;
            var node = thumb.DataContext as Node;
            if (node == null)
                return;
            node.X +=e.HorizontalChange;
            node.Y +=e.VerticalChange;

        }
    }
}
