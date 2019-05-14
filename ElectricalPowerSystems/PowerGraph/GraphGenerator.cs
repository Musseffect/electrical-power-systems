using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.ACGraph;

namespace ElectricalPowerSystems.PowerGraph
{
    class GraphGenerator:GraphElement
    {
        float voltage;
        Mode mode;
        public GraphGenerator(string node,float voltage, Mode mode):base()
        {
            nodes.Add(node);
            this.voltage = voltage;
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
                return new List<bool>() { true};
        }
    }
}
