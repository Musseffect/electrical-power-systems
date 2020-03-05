using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ElectricalPowerSystems.GUI
{
    class NetworkViewModel
    {
        public List<Node> Nodes { get; set; }
        public NetworkViewModel()
        {
        }
    }
}
