using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.ACGraph;
using MathNet.Numerics;

namespace ElectricalPowerSystems.PowerGraph
{
    public enum Fault
    {
        Fault2PhaseAB,
        Fault2PhaseBC,
        Fault2PhaseAC,
        Fault1PhaseAGround,
        Fault1PhaseBGround,
        Fault1PhaseCGround,
        Fault3Phase
    }
    //TODO: derive classes for each fault type with phase-to-phase and phase-to-ground complex resistances and other stuff
    public class GraphFault : GraphElement
    {
        Fault faultType;
        public GraphFault(string node,Fault faultType):base()
        {
            throw new NotImplementedException();
            nodes.Add(node);
            this.faultType = faultType;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new Fault2PhaseScheme(nodes,acGraph,this);
        }

        public override List<bool> getPhaseNodes()
        {
            return new List<bool>() { true };
        }
        public Fault getFaultType()
        {
            return faultType;
        }
    }

    public class Fault2PhaseScheme : PowerElementScheme
    {
        int inoutA;
        int inoutB;
        int inoutC;
        int inoutN;

        Fault faultType;
        Dictionary<string, int> elements;
        public Fault2PhaseScheme(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphFault fault)
        {
            inoutA = nodes[0].A;
            inoutB = nodes[0].B;
            inoutC = nodes[0].C;
            inoutN = nodes[0].N;
            generate(acGraph, fault);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphFault fault)
        {
            this.faultType = fault.getFaultType();
            switch (fault.getFaultType())
            {
                case Fault.Fault2PhaseAB:
                    elements.Add("ABLine",acGraph.createLine(inoutA,inoutB));
                    break;
                case Fault.Fault2PhaseBC:
                    elements.Add("BCLine", acGraph.createLine(inoutB, inoutC));
                    break;
                case Fault.Fault2PhaseAC:
                    elements.Add("ACLine", acGraph.createLine(inoutA, inoutC));
                    break;
                case Fault.Fault1PhaseAGround:
                    acGraph.createGround(inoutA);
                    break;
                case Fault.Fault1PhaseBGround:
                    acGraph.createGround(inoutB);
                    break;
                case Fault.Fault1PhaseCGround:
                    acGraph.createGround(inoutC);
                    break;
                case Fault.Fault3Phase:
                    elements.Add("ABLine", acGraph.createLine(inoutA, inoutB));
                    elements.Add("BCLine", acGraph.createLine(inoutB, inoutC));
                    break;
            }
            /*int A1 = acGraph.allocateNode(), B1 = acGraph.allocateNode(), C1 = acGraph.allocateNode();
            RA = acGraph.createResistor(inN, A1, load.resistanceA.Real);
            RB = acGraph.createResistor(inN, B1, load.resistanceB.Real);
            RC = acGraph.createResistor(inN, C1, load.resistanceC.Real);
            LA = acGraph.createInductor(A1, inA, load.resistanceA.Imaginary);
            LB = acGraph.createInductor(B1, inB, load.resistanceB.Imaginary);
            LC = acGraph.createInductor(C1, inC, load.resistanceC.Imaginary);*/
            //acGraph.createGround(inN);
        }
        override public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution)
        {
            throw new NotImplementedException();
            /*Complex32 opA = solution.currents[RA] * (solution.voltages[inA]);
            Complex32 opB = solution.currents[RB] * (solution.voltages[inB]);
            Complex32 opC = solution.currents[RC] * (solution.voltages[inC]);
            PowerGraphManager.ABCValue power = new PowerGraphManager.ABCValue();
            power.A = opA;
            power.B = opB;
            power.C = opC;
            result.powers.Add(power);*/
        }
    }



}

