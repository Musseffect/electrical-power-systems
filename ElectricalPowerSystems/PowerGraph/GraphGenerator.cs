using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.ACGraph;

namespace ElectricalPowerSystems.PowerGraph
{
    public class GraphGenerator:GraphElement
    {
        public float voltage;
        Mode mode;
        public GraphGenerator(string node,float voltage, Mode mode):base()
        {
            nodes.Add(node);
            this.voltage = voltage;
            this.mode = mode;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes,ACGraph.ACGraph acGraph)
        {
            if (mode == Mode.Delta)
            {
                return new GeneratorSchemeD(nodes,acGraph,this);
                acGraph.createVoltageSource(nodes[0].C,nodes[0].A,voltage, 0.0f, PowerGraphManager.powerFrequency);
                acGraph.createVoltageSource(nodes[0].A,nodes[0].B,voltage, (float)(Math.PI) * 2.0f / 3.0f, PowerGraphManager.powerFrequency);
                acGraph.createVoltageSource(nodes[0].B,nodes[0].C,voltage, (float)(Math.PI) * 4.0f / 3.0f, PowerGraphManager.powerFrequency);
            } else
            {
                return new GeneratorSchemeY(nodes, acGraph, this);
                acGraph.createVoltageSource(nodes[0].N,nodes[0].A,this.voltage,0.0f, PowerGraphManager.powerFrequency);
                acGraph.createVoltageSource(nodes[0].N,nodes[0].B,this.voltage,(float)(Math.PI)*2.0f/3.0f, PowerGraphManager.powerFrequency);
                acGraph.createVoltageSource(nodes[0].N,nodes[0].C,this.voltage,(float)(Math.PI)*4.0f/3.0f, PowerGraphManager.powerFrequency);
                acGraph.createGround(nodes[0].N);
            }
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
