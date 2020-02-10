using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.ACGraph;

namespace ElectricalPowerSystems.PowerGraph
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
        public override void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraphSolution solution)
        {
            throw new NotImplementedException();
        }
    }
}
