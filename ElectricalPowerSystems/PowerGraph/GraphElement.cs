using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{
    enum Mode
    {
        Delta,
        Wye
    }
    class GraphElement
    {
        protected List<string> nodes;
        public List<string> Nodes { get { return nodes; } }
        public GraphElement()
        {
            nodes = new List<string>();
        }
    }
}
