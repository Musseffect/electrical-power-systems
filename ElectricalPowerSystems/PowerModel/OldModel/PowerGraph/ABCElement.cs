using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.OldModel.PowerGraph
{
    public class ABCNode
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
    }
    class ABCElement
    {
        GraphElement elementDescription;
        public List<ABCNode> Nodes { get; set; }
        public ABCElement(GraphElement element)
        {
            Nodes = new List<ABCNode>();
            elementDescription = element;
        }
        public GraphElement GetElementDescription()
        {
            return elementDescription;
        }
    }
}
