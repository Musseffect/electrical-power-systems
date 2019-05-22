using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
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
        public abstract List<bool> getPhaseNodes();
        public abstract void generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph);
    }
}
