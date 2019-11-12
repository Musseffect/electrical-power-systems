using ElectricalPowerSystems.ACGraph;
using ElectricalPowerSystems.PowerGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
voltageSource("4", "1", 1000.0, 0.0, 60.0);
voltageSource("4", "2", 1000.0, 120.0, 60.0);
voltageSource("4", "3", 1000.0, 240.0, 60.0);
resistor("8", "9", 1.0);
resistor("8", "10", 1.0);
resistor("8", "11", 1.0);
inductor("5","9",0);
inductor("6","10",0);
inductor("7","11",0);
elN=line("8","4");
elA=line("1","5");
elB=line("2","6");
elC=line("3","7");
ground("4");
current(elN);
current(elA);
current(elB);
current(elC);
     */



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
            PowerGraph.PowerGraphManager.powerFrequency = (float)(60.0 * 2.0 * Math.PI);
            List<int> elements=new List<int>();
            elements.Add(graph.addElement(new PowerGraph.GraphGenerator("a1", 100.0f, PowerGraph.Mode.Wye)));
            elements.Add(graph.addElement(new PowerGraph.GraphLoad("a1", 1.0f, 1.0f, 1.0f, PowerGraph.Mode.Wye)));
            /*elements.Add(graph.addElement(new PowerGraph.GraphGenerator("a1",1000.0f,PowerGraph.Mode.Wye)));
            elements.Add(graph.addElement(new PowerGraph.GraphTransformer2w("a1", "a2", 3.0f, 1.0f, 1.0f, PowerGraph.TransformerMode.Y0, PowerGraph.TransformerMode.Y0)));
            elements.Add(graph.addElement(new PowerGraph.GraphTransformer2w("a1", "a3", 0.5f, 1.0f, 1.0f, PowerGraph.TransformerMode.Y0, PowerGraph.TransformerMode.Y0)));
            elements.Add(graph.addElement(new PowerGraph.GraphLoad("a1", 1.0f, 1.0f, 1.0f, PowerGraph.Mode.Wye)));
            elements.Add(graph.addElement(new PowerGraph.GraphLoad("a2", 1.0f, 1.0f, 1.0f, PowerGraph.Mode.Wye)));
            elements.Add(graph.addElement(new PowerGraph.GraphLoad("a2", 1.0f, 1.0f, 1.0f, PowerGraph.Mode.Wye)));
            elements.Add(graph.addElement(new PowerGraph.GraphLoad("a2", 1.0f, 1.0f, 1.0f, PowerGraph.Mode.Wye)));
            elements.Add(graph.addElement(new PowerGraph.GraphLoad("a3", 1.0f, 1.0f, 1.0f, PowerGraph.Mode.Wye)));
            elements.Add(graph.addElement(new PowerGraph.GraphLoad("a3", 1.0f, 1.0f, 1.0f, PowerGraph.Mode.Wye)));
            elements.Add(graph.addElement(new PowerGraph.GraphLoad("a3", 1.0f, 1.0f, 1.0f, PowerGraph.Mode.Wye)));*/
            foreach (var element in elements)
            {
                graph.addOutput(new PowerOutput(OutputMode.FULL, element));
            }
            graph.addOutput(new VoltageOutput(OutputMode.FULL, "a1"));
            /*graph.addOutput(new VoltageOutput(OutputMode.FULL, "a1"));
            graph.addOutput(new VoltageOutput(OutputMode.FULL, "a2"));
            graph.addOutput(new VoltageOutput(OutputMode.FULL, "a3"));*/
            List<string> errors=new List<string>();
            List<string> output = new List<string>();
            graph.solve(ref errors,ref output);
            //send output to console stream
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            foreach(var line in output)
                Stdout.WriteLine(line);
            Stdout.Flush();
            Stdout.Close();
        }
    }
}
