using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.ACGraph;

namespace ElectricalPowerSystems.PowerGraph
{
    class GraphSwitch:GraphElement
    {
        public bool stateA;
        public bool stateB;
        public bool stateC;
        public GraphSwitch(string node1,string node2,bool stateA,bool stateB,bool stateC)
        {
            nodes.Add(node1);
            this.stateA = stateA;
            this.stateB = stateB;
            this.stateC = stateC;
        }

        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new SwitchScheme(nodes, acGraph, this);
        }
    }
    class SwitchScheme : PowerElementScheme
    {
        int inA;
        int inB;
        int inC;

        int outA;
        int outB;
        int outC;
        public SwitchScheme(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphSwitch element) :base()
        {
            inA = nodes[0].A;
            inB = nodes[0].B;
            inC = nodes[0].C;
            outA = nodes[1].A;
            outB = nodes[1].B;
            outC = nodes[1].C;
            generate(acGraph,element);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphSwitch element)
        {
            acGraph.CreateSwitch(inA, outA, element.stateA);
            acGraph.CreateSwitch(inB, outB, element.stateB);
            acGraph.CreateSwitch(inC, outC, element.stateB);
        }
    }
}
