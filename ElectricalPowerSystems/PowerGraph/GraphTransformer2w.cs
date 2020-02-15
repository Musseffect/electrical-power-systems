using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{
    public class GraphTransformer2w : GraphElement
    {
        public float kTrans;
        public Complex32 Z1;
        public Complex32 Z2;
        public TransformerWinding winding1;
        public TransformerWinding winding2;
        public GraphTransformer2w(string node1, string node2,float kTrans,Complex32 Z1,Complex32 Z2, TransformerWinding winding1, TransformerWinding winding2) :base()
        {
            nodes.Add(node1);
            nodes.Add(node2);
            this.kTrans = kTrans;
            this.Z1 = Z1;
            this.Z2 = Z2;
            this.winding1 = winding1;
            this.winding2 = winding2;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new Transformer2wScheme(nodes,acGraph,this);
            /*int[] node1 = generateWindingConnection(mode1, nodes[0]);
            int[] node2 = generateWindingConnection(mode2, nodes[1]);
            acGraph.createTransformer(node1[0], node1[1], node2[0], node2[1], kTrans);
            acGraph.createTransformer(node1[2], node1[3], node2[2], node2[3], kTrans);
            acGraph.createTransformer(node1[4], node1[5], node2[4], node2[5], kTrans);
            throw new NotImplementedException();*/
        }
    }
    public class Transformer2wScheme : PowerElementScheme
    {
        int inA;
        int inB;
        int inC;
        int outA;
        int outB;
        int outC;

        int w1A;
        int w1B;
        int w1C;
        int w2A;
        int w2B;
        int w2C;

        int iA1;
        int iB1;
        int iC1;
        int oA1;
        int oB1;
        int oC1;

        int iRA;
        int iRB;
        int iRC;

        int iLA;
        int iLB;
        int iLC;
        int oRA;
        int oRB;
        int oRC;
        int oLA;
        int oLB;
        int oLC;
        int T1;
        int T2;
        int T3;


        public Transformer2wScheme(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphTransformer2w t)
        {
            inA = nodes[0].A;
            inB = nodes[0].B;
            inC = nodes[0].C;

            outA = nodes[1].A;
            outB = nodes[1].B;
            outC = nodes[1].C;
            generate(acGraph, t);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphTransformer2w t)
        {
            w1A = acGraph.allocateNode();
            w1B = acGraph.allocateNode();
            w1C = acGraph.allocateNode();
            w2A = acGraph.allocateNode();
            w2B = acGraph.allocateNode();
            w2C = acGraph.allocateNode();
            int[] node1 = t.winding1.generateWinding(w1A,w1B,w1C,acGraph);
            int[] node2 = t.winding2.generateWinding(w2A,w2B,w2C,acGraph);

            T1 = acGraph.createTransformer(node1[0], node1[1], node2[0], node2[1], t.kTrans);
            T2 = acGraph.createTransformer(node1[2], node1[3], node2[2], node2[3], t.kTrans);
            T3 = acGraph.createTransformer(node1[4], node1[5], node2[4], node2[5], t.kTrans);
            iA1 = acGraph.allocateNode();
            iB1 = acGraph.allocateNode();
            iC1 = acGraph.allocateNode();
            oA1 = acGraph.allocateNode();
            oB1 = acGraph.allocateNode();
            oC1 = acGraph.allocateNode();
            iRA = acGraph.createResistorWithCurrent(inA, iA1, t.Z1.Real);
            iRB = acGraph.createResistorWithCurrent(inB, iB1, t.Z1.Real);
            iRC = acGraph.createResistorWithCurrent(inC, iC1, t.Z1.Real);
            iLA = acGraph.createInductor(iA1, w1A, t.Z1.Imaginary);
            iLB = acGraph.createInductor(iB1, w1B, t.Z1.Imaginary);
            iLC = acGraph.createInductor(iC1, w1C, t.Z1.Imaginary);
            oRA = acGraph.createResistorWithCurrent(outA, oA1, t.Z2.Real);
            oRB = acGraph.createResistorWithCurrent(outB, oB1, t.Z2.Real);
            oRC = acGraph.createResistorWithCurrent(outC, oC1, t.Z2.Real);
            oLA = acGraph.createInductor(oA1, w2A, t.Z2.Imaginary);
            oLB = acGraph.createInductor(oB1, w2B, t.Z2.Imaginary);
            oLC = acGraph.createInductor(oC1, w2C, t.Z2.Imaginary);
        }
        override public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution)
        {
            Complex32 ipA = solution.currents[iRA] * (solution.voltages[inA] - solution.voltages[w1A]);
            Complex32 ipB = solution.currents[iRB] * (solution.voltages[inB] - solution.voltages[w1B]);
            Complex32 ipC = solution.currents[iRC] * (solution.voltages[inC] - solution.voltages[w1C]);
            Complex32 opA = solution.currents[oRA] * (solution.voltages[outA] - solution.voltages[w2A]);
            Complex32 opB = solution.currents[oRB] * (solution.voltages[outB] - solution.voltages[w2B]);
            Complex32 opC = solution.currents[oRC] * (solution.voltages[outC] - solution.voltages[w2C]);
            PowerGraphManager.ABCValue power = new PowerGraphManager.ABCValue();
            power.A = opA + ipA;
            power.B = opB + ipB;
            power.C = opC + ipC;
            result.powers.Add(power);
            result.currents.Add(new PowerGraphManager.ABCValue[] { new PowerGraphManager.ABCValue() {
                A=-solution.currents[iRA],B=-solution.currents[iRB],C=-solution.currents[iRC]
            },new PowerGraphManager.ABCValue{
                A=-solution.currents[oRA],B=-solution.currents[oRB],C=-solution.currents[oRC]
            } });

        }
    }
}
