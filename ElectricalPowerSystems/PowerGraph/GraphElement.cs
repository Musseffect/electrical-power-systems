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
    public abstract class GraphElement
    {
        protected List<string> nodes;
        public List<string> Nodes { get { return nodes; } }
        public GraphElement()
        {
            nodes = new List<string>();
        }
        public abstract List<bool> getPhaseNodes();
        public abstract void generateACGraph(ACGraph.ACGraph acGraph);
    }
}
