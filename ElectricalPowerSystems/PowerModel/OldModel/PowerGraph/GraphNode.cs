using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.OldModel.PowerGraph
{
    class GraphNode
    {
        int index;
        public int Index { get; }
        public GraphNode(int index)
        {
            this.index = index;
        }
    }
}
