using ElectricalPowerSystems.ACGraph;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{
    class GraphLoad:GraphElement
    {
        Complex32 resistanceA;
        Complex32 resistanceB;
        Complex32 resistanceC;
        Mode mode;
        public GraphLoad(string node,Mode mode)
        {
            nodes.Add(node);
            this.mode = mode;
        }

        public override void generateACGraph(ACGraph.ACGraph acGraph)
        {
            throw new NotImplementedException();
        }

        public override List<bool> getPhaseNodes()
        {
            if (mode == Mode.Delta)
                return new List<bool>() { false };
            else
                return new List<bool>() { true };
        }
    }
}
