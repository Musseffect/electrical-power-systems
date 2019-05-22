using ElectricalPowerSystems.ACGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Test
{
    class Test
    {
        static public void testCircuitModel()
        {
            CircuitModelAC model = new CircuitModelAC();
            model.addVoltageSource("a2", "a1", 10.0f, 50.0f, 10.0f);
            model.addVoltageSource("a1", "a3", 10.0f, 50.0f, 0.5f);
            model.addResistor("a1", "a3", 5.0f);
            model.addResistor("a1", "a4", 15.0f);
            model.addResistor("a4", "a2", 4.0f);
            model.addGround("a2");
            model.Solve();
        }
        static public void testPowerModel()
        {
            PowerGraph.PowerGraphManager graph = new PowerGraph.PowerGraphManager();
            float voltage = 100.0f;
            graph.addElement(new PowerGraph.GraphGenerator("a1",voltage,PowerGraph.Mode.Wye));
            graph.addElement(new PowerGraph.GraphTransformer2w("a1","a2",1.0f,1.0f,1.0f));
            graph.addElement(new PowerGraph.GraphAirLine("a2","a3",1.0f,1.0f,1.0f,1.0f,1.0f));
            graph.addElement(new PowerGraph.GraphTransformer2w("a3","a4", 1.0f, 1.0f, 1.0f));
            graph.addElement(new PowerGraph.GraphLoad("a4",1.0f,1.0f,1.0f,PowerGraph.Mode.Delta));
            List<string> errors=new List<string>();
            graph.solve(ref errors);
        }
    }
}
