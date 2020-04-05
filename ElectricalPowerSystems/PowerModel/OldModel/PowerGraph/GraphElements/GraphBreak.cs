using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.PowerModel.OldModel.ACGraph;

namespace ElectricalPowerSystems.PowerModel.OldModel.PowerGraph
{
    class GraphBreak : GraphElement
    {
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            throw new NotImplementedException();
        }
    }
    class BreakScheme : PowerElementScheme
    {
    }
}
