using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.GUI
{
    class GraphViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        public List<Node> nodes;
        public List<Node> Nodes { get { return nodes; }
            set {
                nodes = value;
                OnPropertyChanged();
            }
        }
        public GraphViewModel()
        {
            nodes = NodesSource.createNodes();
        }
    }
}
