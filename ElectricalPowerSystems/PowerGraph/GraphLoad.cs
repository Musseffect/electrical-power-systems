using ElectricalPowerSystems.ACGraph;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace ElectricalPowerSystems.PowerGraph
{
    public class GraphLoadPQWye : GraphElement
    {
        public Complex32 s;
        public GroundingType grounding;
        public GraphLoadPQWye(string node, Complex32 s, GroundingType grounding)
        {
            nodes.Add(node);
            this.s = s;
            this.grounding = grounding;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new LoadSchemePQWye(nodes, acGraph, this);
        }
    }
    public class GraphLoadPQDelta : GraphElement
    {
        public Complex32 s;
        public GraphLoadPQDelta(string node, Complex32 s)
        {
            nodes.Add(node);
            this.s = s;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new LoadSchemePQDelta(nodes, acGraph, this);
        }
    }
    public class GraphLoadWye :GraphElement
    {   
        public Complex32 resistanceA;
        public Complex32 resistanceB;
        public Complex32 resistanceC;
        public GroundingType grounding;
        public GraphLoadWye(string node,Complex32 resA,Complex32 resB,Complex32 resC,GroundingType grounding)
        {
            nodes.Add(node);
            this.grounding = grounding;
            resistanceA = resA;
            resistanceB = resB;
            resistanceC = resC;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new LoadSchemeWye(nodes,acGraph,this);
        }
    }

    public class GraphLoadDelta : GraphElement
    {
        public Complex32 resistanceA;
        public Complex32 resistanceB;
        public Complex32 resistanceC;
        public GraphLoadDelta(string node, Complex32 resA, Complex32 resB, Complex32 resC)
        {
            nodes.Add(node);
            resistanceA = resA;
            resistanceB = resB;
            resistanceC = resC;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new LoadSchemeDelta(nodes, acGraph, this);
        }
    }
    public class LoadSchemePQDelta : PowerElementScheme
    {
        public LoadSchemePQDelta(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphLoadPQDelta load)
        {
            throw new NotImplementedException();
        }
        public override void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraphSolution solution)
        {
            throw new NotImplementedException();
        }
    }
    public class LoadSchemePQWye : PowerElementScheme
    {
        public LoadSchemePQWye(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphLoadPQWye load)
        {
            throw new NotImplementedException();
        }
        public override void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraphSolution solution)
        {
            throw new NotImplementedException();
        }
    }
    public class LoadSchemeDelta : PowerElementScheme
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
        public LoadSchemeDelta(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphLoadDelta load)
        {
            inA = nodes[0].A;
            inB = nodes[0].B;
            inC = nodes[0].C;
            generate(acGraph, load);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphLoadDelta load)
        {
            int A1 = acGraph.allocateNode(), B1 = acGraph.allocateNode(), C1 = acGraph.allocateNode();

            RCA = acGraph.createResistor(inC, A1, load.resistanceA.Real);
            RAB = acGraph.createResistor(inA, B1, load.resistanceB.Real);
            RBC = acGraph.createResistor(inB, C1, load.resistanceC.Real);
            LCA = acGraph.createInductor(A1, inA, load.resistanceA.Imaginary);
            LAB = acGraph.createInductor(B1, inB, load.resistanceB.Imaginary);
            LBC = acGraph.createInductor(C1, inC, load.resistanceC.Imaginary);
        }
        override public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution)
        {
            Complex32 opA = solution.currents[RCA] * (solution.voltages[inC] - solution.voltages[inA]);
            Complex32 opB = solution.currents[RAB] * (solution.voltages[inA] - solution.voltages[inB]);
            Complex32 opC = solution.currents[RBC] * (solution.voltages[inB] - solution.voltages[inC]);
            PowerGraphManager.ABCValue power = new PowerGraphManager.ABCValue();
            power.A = opA;
            power.B = opB;
            power.C = opC;
            result.powers.Add(power);
            result.currents.Add(new PowerGraphManager.ABCValue[] {
                new PowerGraphManager.ABCValue{
                A=solution.currents[RCA] - solution.currents[RAB],
                B=solution.currents[RAB] - solution.currents[RBC],
                C=solution.currents[RBC] - solution.currents[RCA]
                }
            });
        }
    }
    public class LoadSchemeWye : PowerElementScheme
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
        public LoadSchemeWye(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphLoadWye load)
        {
            inA = nodes[0].A;
            inB = nodes[0].B;
            inC = nodes[0].C;
            inN = acGraph.allocateNode();
            generate(acGraph, load);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphLoadWye load)
        {
            int A1 = acGraph.allocateNode(), B1 = acGraph.allocateNode(), C1 = acGraph.allocateNode();
            RA = acGraph.createResistor(inN, A1, load.resistanceA.Real);
            RB = acGraph.createResistor(inN, B1, load.resistanceB.Real);
            RC = acGraph.createResistor(inN, C1, load.resistanceC.Real);
            LA = acGraph.createInductor(A1, inA, load.resistanceA.Imaginary);
            LB = acGraph.createInductor(B1, inB, load.resistanceB.Imaginary);
            LC = acGraph.createInductor(C1, inC, load.resistanceC.Imaginary);
            //acGraph.createGround(inN);
        }
        override public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution)
        {
            Complex32 opA = solution.currents[RA] * (solution.voltages[inN] - solution.voltages[inA]);
            Complex32 opB = solution.currents[RB] * (solution.voltages[inN] - solution.voltages[inB]);
            Complex32 opC = solution.currents[RC] * (solution.voltages[inN] - solution.voltages[inC]);
            PowerGraphManager.ABCValue power = new PowerGraphManager.ABCValue();
            power.A = opA;
            power.B = opB;
            power.C = opC;
            result.powers.Add(power);
            result.currents.Add(new PowerGraphManager.ABCValue[] {
                new PowerGraphManager.ABCValue{
                A=solution.currents[RA],
                B=solution.currents[RB],
                C=solution.currents[RC]
                }
            });
            //throw new NotImplementedException();
        }
    }

}
