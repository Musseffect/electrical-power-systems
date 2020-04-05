using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.OldModel.PowerGraph
{
    public enum Mode
    {
        Delta,
        Wye
    }
    public abstract class GraphElement
    {
        protected List<string> nodes;
        public List<string> Nodes { get { return nodes; } }
        public GraphElement()
        {
            nodes = new List<string>();
        }
        public abstract PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph);
    }
}
