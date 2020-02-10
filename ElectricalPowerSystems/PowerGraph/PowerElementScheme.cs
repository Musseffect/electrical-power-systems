using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{
    public abstract class PowerElementScheme
    {
        /*public List<ABCNode> nodes;
        public List<int> schemeElements;
        public GraphElement element;*/
        abstract public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution);
    }
}
