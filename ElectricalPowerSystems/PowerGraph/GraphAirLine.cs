using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.ACGraph;

namespace ElectricalPowerSystems.PowerGraph
{
    class GraphAirLine : GraphElement
    {
        GraphAirLine(string node1,string node2) : base()
        {
            nodes.Add(node1);
            nodes.Add(node2);
        }

        public override void generateACGraph(ACGraph.ACGraph acGraph)
        {
            throw new NotImplementedException();
        }

        public override List<bool> getPhaseNodes()
        {
            return new List<bool>() { false, false };
        }
    }
}
