using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{
    class ABCNode
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int N { get; set; }
    }
    class ABCElement
    {
        List<ABCNode> nodes;
        GraphElement elementDescription;
        public List<ABCNode> Nodes { get; set; }
        public ABCElement(GraphElement element)
        {
            nodes = new List<ABCNode>();
            elementDescription = element;
        }
        public GraphElement getElementDescription()
        {
            return elementDescription;
        }
    }
}
