using ElectricalPowerSystems.ACGraph;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace ElectricalPowerSystems.PowerGraph
{
    public class GraphLoad :GraphElement
    {   
        public Complex32 resistanceA;
        public Complex32 resistanceB;
        public Complex32 resistanceC;
        Mode mode;
        public GraphLoad(string node,Complex32 resA,Complex32 resB,Complex32 resC,Mode mode)
        {
            nodes.Add(node);
            this.mode = mode;
            resistanceA = resA;
            resistanceB = resB;
            resistanceC = resC;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            if (mode == Mode.Delta)
            {
                return new LoadSchemeD(nodes,acGraph,this);
            }
            else
            {
                return new LoadSchemeY(nodes,acGraph,this);
            }
        }
        public override List<bool> getPhaseNodes()
        {
            if (mode == Mode.Delta)
                return new List<bool>() { false };
            else
                return new List<bool>() { true };
        }
    }
}
