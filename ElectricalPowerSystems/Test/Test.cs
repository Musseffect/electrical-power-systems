using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Test
{
    class Test
    {
        static public void testPowerModel()
        {
            PowerGraph.PowerGraphManager graph = new PowerGraph.PowerGraphManager();
            graph.addElement(new PowerGraph.GraphGenerator("a1",voltage));
            graph.addElement(new PowerGraph.GraphTransformer2w("a1","a2",));
            graph.addElement(new PowerGraph.GraphAirLine("a2","a3"));
            graph.addElement(new PowerGraph.GraphTransformer2w("a3","a4"),);
            graph.addElement(new PowerGraph.GraphLoad("a4",));
            List<string> errors=new List<string>();
            graph.solve(ref errors);
        }
    }
}
