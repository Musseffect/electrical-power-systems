using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
