using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{
    class GraphTransformer2w : GraphElement
    {
        GraphTransformer2w(string node1, string node2):base()
        {
            nodes.Add(node1);
            nodes.Add(node2);
        }
    }
}
