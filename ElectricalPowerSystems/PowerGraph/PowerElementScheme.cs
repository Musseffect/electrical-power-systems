using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{
    public abstract class PowerElementScheme
    {
        /*public List<ABCNode> nodes;
        public List<int> schemeElements;
        public GraphElement element;*/
        abstract public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution);
    }
    public class LoadSchemeD :PowerElementScheme
    {
        int inA;
        int inB;
        int inC;
        int RAB;
        int LAB;
        int RBC;
        int LBC;
        int RCA;
        int LCA;
        public LoadSchemeD(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphLoad load)
        {
            inA = nodes[0].A;
            inB = nodes[0].B;
            inC = nodes[0].C;
            generate(acGraph, load);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphLoad load)
        {
            int A1 = acGraph.allocateNode(), B1 = acGraph.allocateNode(), C1 = acGraph.allocateNode();

            RCA = acGraph.createResistor(inC, A1, load.resistanceA.Real);
            RAB = acGraph.createResistor(inA, B1, load.resistanceB.Real);
            RBC = acGraph.createResistor(inB, C1, load.resistanceC.Real);
            LCA = acGraph.createInductor(A1, inA, load.resistanceA.Imaginary);
            LAB = acGraph.createInductor(B1, inB, load.resistanceB.Imaginary);
            LBC = acGraph.createInductor(C1, inC, load.resistanceC.Imaginary);
        }
        override public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result,ACGraph.ACGraphSolution solution)
        {
            Complex32 opA = solution.currents[RCA] * (solution.voltages[inA] - solution.voltages[inC]);
            Complex32 opB = solution.currents[RAB] * (solution.voltages[inB] - solution.voltages[inA]);
            Complex32 opC = solution.currents[RBC] * (solution.voltages[inC] - solution.voltages[inB]);
            PowerGraphManager.ABCValue power = new PowerGraphManager.ABCValue();
            power.A = opA;
            power.B = opB;
            power.C = opC;
            result.powers.Add(power);
            //throw new NotImplementedException();
        }
    }
    public class LoadSchemeY : PowerElementScheme
    {
        int inA;
        int inB;
        int inC;
        int inN;
        int A1;
        int B1;
        int C1;

        int RA;
        int LA;
        int RB;
        int LB;
        int RC;
        int LC;
        public LoadSchemeY(List<ABCNode> nodes,ACGraph.ACGraph acGraph,GraphLoad load)
        {
            inA = nodes[0].A;
            inB = nodes[0].B;
            inC = nodes[0].C;
            inN = nodes[0].N;
            generate(acGraph,load);
        }
        private void generate(ACGraph.ACGraph acGraph,GraphLoad load)
        {
            int A1 = acGraph.allocateNode(), B1 = acGraph.allocateNode(), C1 = acGraph.allocateNode();
            RA=acGraph.createResistor(inN, A1, load.resistanceA.Real);
            RB=acGraph.createResistor(inN, B1, load.resistanceB.Real);
            RC=acGraph.createResistor(inN, C1, load.resistanceC.Real);
            LA=acGraph.createInductor(A1, inA, load.resistanceA.Imaginary);
            LB=acGraph.createInductor(B1, inB, load.resistanceB.Imaginary);
            LC=acGraph.createInductor(C1, inC, load.resistanceC.Imaginary);
            //acGraph.createGround(inN);
        }
        override public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution)
        {
            Complex32 opA = solution.currents[RA] * (solution.voltages[inA]);
            Complex32 opB = solution.currents[RB] * (solution.voltages[inB]);
            Complex32 opC = solution.currents[RC] * (solution.voltages[inC]);
            PowerGraphManager.ABCValue power = new PowerGraphManager.ABCValue();
            power.A = opA;
            power.B = opB;
            power.C = opC;
            result.powers.Add(power);
            //throw new NotImplementedException();
        }
    }
    public class GeneratorSchemeD : PowerElementScheme
    {
        int outA;
        int outB;
        int outC;
        int VAB;
        int VBC;
        int VCA;
        public GeneratorSchemeD(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphGenerator generator)
        {
            outA = nodes[0].A;
            outB = nodes[0].B;
            outC = nodes[0].C;
            generate(acGraph, generator);
        }
        private void generate(ACGraph.ACGraph acGraph,GraphGenerator generator)
        {
            acGraph.createVoltageSource(outC, outA, generator.voltage, 0.0f, PowerGraphManager.powerFrequency);
            acGraph.createVoltageSource(outA, outB, generator.voltage, (float)(Math.PI) * 2.0f / 3.0f, PowerGraphManager.powerFrequency);
            acGraph.createVoltageSource(outB, outC, generator.voltage, (float)(Math.PI) * 4.0f / 3.0f, PowerGraphManager.powerFrequency);
        }
        override public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution)
        {
            Complex32 opA = solution.currents[VCA] * (solution.voltages[outA] - solution.voltages[outC]);
            Complex32 opB = solution.currents[VAB] * (solution.voltages[outB] - solution.voltages[outA]);
            Complex32 opC = solution.currents[VBC] * (solution.voltages[outC] - solution.voltages[outB]);
            PowerGraphManager.ABCValue power = new PowerGraphManager.ABCValue();
            power.A = opA;
            power.B = opB;
            power.C = opC;
            result.powers.Add(power);
        }
    }
    public class GeneratorSchemeY : PowerElementScheme
    {
        int outA;
        int outB;
        int outC;
        int outN;

        int VA;
        int VB;
        int VC;
        public GeneratorSchemeY(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphGenerator generator)
        {
            outA = nodes[0].A;
            outB = nodes[0].B;
            outC = nodes[0].C;
            outN = nodes[0].N;
            generate(acGraph,generator);
        }
        private void generate(ACGraph.ACGraph acGraph,GraphGenerator generator)
        {
            VA=acGraph.createVoltageSource(outN, outA, generator.voltage, 0.0f, PowerGraphManager.powerFrequency);
            VB=acGraph.createVoltageSource(outN, outB, generator.voltage, (float)(Math.PI) * 2.0f / 3.0f, PowerGraphManager.powerFrequency);
            VC=acGraph.createVoltageSource(outN, outC, generator.voltage, (float)(Math.PI) * 4.0f / 3.0f, PowerGraphManager.powerFrequency);
            acGraph.createGround(outN);
        }
        override public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution)
        {
            Complex32 opA = solution.currents[VA] * (solution.voltages[outA]);
            Complex32 opB = solution.currents[VB] * (solution.voltages[outB]);
            Complex32 opC = solution.currents[VC] * (solution.voltages[outC]);
            PowerGraphManager.ABCValue power = new PowerGraphManager.ABCValue();
            power.A = opA;
            power.B = opB;
            power.C = opC;
            result.powers.Add(power);
        }
    }
    public class AirLineSchemeD : PowerElementScheme
    {
        int inA;
        int inB;
        int inC;
        int outA;
        int outB;
        int outC;

        int RA;
        int RB;
        int RC;
        int LA;
        int LB;
        int LC;
        int iBAB;
        int iBBC;
        int iBCA;
        int oBAB;
        int oBBC;
        int oBCA;
        int iGA;
        int iGB;
        int iGC;
        int oGA;
        int oGB;
        int oGC;
        int iBA;
        int iBB;
        int iBC;
        int oBA;
        int oBB;
        int oBC;
        public AirLineSchemeD(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphAirLine airLine)
        {
            inA = nodes[0].A;
            inB = nodes[0].B;
            inC = nodes[0].C;

            outA = nodes[1].A;
            outB = nodes[1].B;
            outC = nodes[1].C;
            generate(acGraph,airLine);
        }
        private void generate(ACGraph.ACGraph acGraph,GraphAirLine airLine)
        {
            int A1 = acGraph.allocateNode();
            int B1 = acGraph.allocateNode();
            int C1 = acGraph.allocateNode();
            int nG = acGraph.allocateNode();
            RA=acGraph.createResistor(inA, A1, airLine.R);
            RB=acGraph.createResistor(inB, B1, airLine.R);
            RC=acGraph.createResistor(inC, C1, airLine.R);
            LA=acGraph.createInductor(A1, outA, airLine.L);
            LB=acGraph.createInductor(B1, outB, airLine.L);
            LC=acGraph.createInductor(C1, outC, airLine.L);
            iBAB=acGraph.createCapacitor(inA, inB, airLine.Bp * 0.5f);
            iBBC=acGraph.createCapacitor(inB, inC, airLine.Bp * 0.5f);
            iBCA=acGraph.createCapacitor(inC, inA, airLine.Bp * 0.5f);
            oBAB=acGraph.createCapacitor(outA, outB, airLine.Bp * 0.5f);
            oBBC=acGraph.createCapacitor(outB, outC, airLine.Bp * 0.5f);
            oBCA=acGraph.createCapacitor(outC, outA, airLine.Bp * 0.5f);
            iGA=acGraph.createResistor(inA, nG, airLine.G * 0.5f);
            iGB=acGraph.createResistor(inB, nG, airLine.G * 0.5f);
            iGC=acGraph.createResistor(inC, nG, airLine.G * 0.5f);
            oGA=acGraph.createResistor(outA, nG, airLine.G * 0.5f);
            oGB=acGraph.createResistor(outB, nG, airLine.G * 0.5f);
            oGC=acGraph.createResistor(outC, nG, airLine.G * 0.5f);
            iBA=acGraph.createCapacitor(inA, nG, airLine.B * 0.5f);
            iBB=acGraph.createCapacitor(inB, nG, airLine.B * 0.5f);
            iBC=acGraph.createCapacitor(inC, nG, airLine.B * 0.5f);
            oBA=acGraph.createCapacitor(outA, nG, airLine.B * 0.5f);
            oBB=acGraph.createCapacitor(outB, nG, airLine.B * 0.5f);
            oBC=acGraph.createCapacitor(outC, nG, airLine.B * 0.5f);
            acGraph.createGround(nG);
        }
        override public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution)
        {
            Complex32 opA = solution.currents[RA] * (solution.voltages[outA] - solution.voltages[inA]);
            Complex32 opB = solution.currents[RB] * (solution.voltages[outB] - solution.voltages[inB]);
            Complex32 opC = solution.currents[RC] * (solution.voltages[outC] - solution.voltages[inC]);
            PowerGraphManager.ABCValue power = new PowerGraphManager.ABCValue();
            power.A = opA;
            power.B = opB;
            power.C = opC;
            result.powers.Add(power);
            //throw new NotImplementedException();
        }
    }
    public class Transformer2wScheme : PowerElementScheme
    {
        int inA;
        int inB;
        int inC;
        int inN;
        int outA;
        int outB;
        int outC;
        int outN;

        int iA1;
        int iB1;
        int iC1;
        int iA2;
        int iB2;
        int iC2;
        int oA1;
        int oB1;
        int oC1;
        int oA2;
        int oB2;
        int oC2;

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
            inN = nodes[0].N;

            outA = nodes[1].A;
            outB = nodes[1].B;
            outC = nodes[1].C;
            outN = nodes[1].N;
            generate(acGraph,t);
        }
        private void generate(ACGraph.ACGraph acGraph,GraphTransformer2w t)
        {
            ABCNode iNode=new ABCNode();
            iNode.A = iA2;
            iNode.B = iB2;
            iNode.C = iC2;
            iNode.N = inN;
            ABCNode oNode=new ABCNode();
            oNode.A = oA2;
            oNode.B = oB2;
            oNode.C = oC2;
            oNode.N = outN;
            int[] node1 = GraphTransformer2w.generateWindingConnection(t.mode1, iNode);
            int[] node2 = GraphTransformer2w.generateWindingConnection(t.mode2, oNode);

            T1 = acGraph.createTransformer(node1[0], node1[1], node2[0], node2[1], t.kTrans);
            T2 = acGraph.createTransformer(node1[2], node1[3], node2[2], node2[3], t.kTrans);
            T3 = acGraph.createTransformer(node1[4], node1[5], node2[4], node2[5], t.kTrans);
            iRA = acGraph.createResistor(inA,iA1,t.Z1.Real);
            iRB = acGraph.createResistor(inB,iB1,t.Z1.Real);
            iRC = acGraph.createResistor(inC,iC1,t.Z1.Real);
            iLA = acGraph.createInductor(iA1,iA2,t.Z1.Imaginary);
            iLB = acGraph.createInductor(iB1,iB2, t.Z1.Imaginary);
            iLC = acGraph.createInductor(iC1,iC2, t.Z1.Imaginary);
            oRA = acGraph.createResistor(outA, oA1,t.Z2.Real);
            oRB = acGraph.createResistor(outB, oB1,t.Z2.Real);
            oRC = acGraph.createResistor(outC, oC1,t.Z2.Real);
            oLA = acGraph.createInductor(oA1, oA2,t.Z2.Imaginary);
            oLB = acGraph.createInductor(oB1, oB2,t.Z2.Imaginary);
            oLC = acGraph.createInductor(oC1, oC2,t.Z2.Imaginary);
        }
        override public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution)
        {
            Complex32 ipA = solution.currents[iRA] * (solution.voltages[iA2] - solution.voltages[inA]);
            Complex32 ipB = solution.currents[iRB] * (solution.voltages[iB2] - solution.voltages[inB]);
            Complex32 ipC = solution.currents[iRC] * (solution.voltages[iC2] - solution.voltages[inC]);
            Complex32 opA = solution.currents[oRA] * (solution.voltages[oA2] - solution.voltages[outA]);
            Complex32 opB = solution.currents[oRB] * (solution.voltages[oB2] - solution.voltages[outB]);
            Complex32 opC = solution.currents[oRC] * (solution.voltages[oC2] - solution.voltages[outC]);
            PowerGraphManager.ABCValue power = new PowerGraphManager.ABCValue();
            power.A = opA - ipA;
            power.B = opB - ipB;
            power.C = opC - ipC;
            result.powers.Add(power);
        }
    }




}
