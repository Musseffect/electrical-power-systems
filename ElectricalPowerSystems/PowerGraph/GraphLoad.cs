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
        Complex32 resistanceA;
        Complex32 resistanceB;
        Complex32 resistanceC;
        Mode mode;
        public GraphLoad(string node,Complex32 resA,Complex32 resB,Complex32 resC,Mode mode)
        {
            nodes.Add(node);
            this.mode = mode;
            resistanceA = resA;
            resistanceB = resB;
            resistanceC = resC;
        }
        public override void generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            int A1 = acGraph.allocateNode(), B1 = acGraph.allocateNode(), C1 = acGraph.allocateNode();
            if (mode == Mode.Delta)
            {
                acGraph.createResistor(nodes[0].C, A1, resistanceA.Real);
                acGraph.createResistor(nodes[0].A, B1, resistanceB.Real);
                acGraph.createResistor(nodes[0].B, C1, resistanceC.Real);
                acGraph.createInductor(A1, nodes[0].A, resistanceA.Imaginary);
                acGraph.createInductor(B1, nodes[0].B, resistanceB.Imaginary);
                acGraph.createInductor(C1, nodes[0].C, resistanceC.Imaginary);
            }
            else
            {
                acGraph.createResistor(nodes[0].N, A1, resistanceA.Real);
                acGraph.createResistor(nodes[0].N, B1, resistanceB.Real);
                acGraph.createResistor(nodes[0].N, C1, resistanceC.Real);
                acGraph.createInductor(A1, nodes[0].A, resistanceA.Imaginary);
                acGraph.createInductor(B1, nodes[0].B, resistanceB.Imaginary);
                acGraph.createInductor(C1, nodes[0].C, resistanceC.Imaginary);
                acGraph.createGround(nodes[0].N);
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
