using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.ACGraph;
using MathNet.Numerics;

namespace ElectricalPowerSystems.PowerGraph
{
    public class GraphAirLinePiSection : GraphElement
    {
        public float R;
        public float L;
        public float B;
        public float G;
        public float Bp;
        public GraphAirLinePiSection(string node1,string node2,float R,float L,float B,float G,float Bp) : base()
        {
            nodes.Add(node1);
            nodes.Add(node2);
            this.R = R;
            this.L = L;
            this.B = B;
            this.G = G;
            this.Bp = Bp;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new AirLineScheme(nodes,acGraph,this);
        }
    }
    public class AirLineScheme : PowerElementScheme
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



        public AirLineScheme(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphAirLinePiSection airLine)
        {
            inA = nodes[0].A;
            inB = nodes[0].B;
            inC = nodes[0].C;

            outA = nodes[1].A;
            outB = nodes[1].B;
            outC = nodes[1].C;
            generate(acGraph, airLine);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphAirLinePiSection airLine)
        {
            int A1 = acGraph.allocateNode();
            int B1 = acGraph.allocateNode();
            int C1 = acGraph.allocateNode();
            int nG = acGraph.allocateNode();
            RA = acGraph.createResistor(inA, A1, airLine.R);
            RB = acGraph.createResistor(inB, B1, airLine.R);
            RC = acGraph.createResistor(inC, C1, airLine.R);
            LA = acGraph.createInductor(A1, outA, airLine.L);
            LB = acGraph.createInductor(B1, outB, airLine.L);
            LC = acGraph.createInductor(C1, outC, airLine.L);
            iBAB = acGraph.createCapacitor(inA, inB, airLine.Bp * 0.5f);
            iBBC = acGraph.createCapacitor(inB, inC, airLine.Bp * 0.5f);
            iBCA = acGraph.createCapacitor(inC, inA, airLine.Bp * 0.5f);
            oBAB = acGraph.createCapacitor(outA, outB, airLine.Bp * 0.5f);
            oBBC = acGraph.createCapacitor(outB, outC, airLine.Bp * 0.5f);
            oBCA = acGraph.createCapacitor(outC, outA, airLine.Bp * 0.5f);
            iGA = acGraph.createResistor(inA, nG, airLine.G * 0.5f);
            iGB = acGraph.createResistor(inB, nG, airLine.G * 0.5f);
            iGC = acGraph.createResistor(inC, nG, airLine.G * 0.5f);
            oGA = acGraph.createResistor(outA, nG, airLine.G * 0.5f);
            oGB = acGraph.createResistor(outB, nG, airLine.G * 0.5f);
            oGC = acGraph.createResistor(outC, nG, airLine.G * 0.5f);
            iBA = acGraph.createCapacitor(inA, nG, airLine.B * 0.5f);
            iBB = acGraph.createCapacitor(inB, nG, airLine.B * 0.5f);
            iBC = acGraph.createCapacitor(inC, nG, airLine.B * 0.5f);
            oBA = acGraph.createCapacitor(outA, nG, airLine.B * 0.5f);
            oBB = acGraph.createCapacitor(outB, nG, airLine.B * 0.5f);
            oBC = acGraph.createCapacitor(outC, nG, airLine.B * 0.5f);
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
            result.currents.Add(new PowerGraphManager.ABCValue[] {
                new PowerGraphManager.ABCValue{
                A=-solution.currents[RA],
                B=-solution.currents[RB],
                C=-solution.currents[RC]
                }
                ,
                new PowerGraphManager.ABCValue{
                A=solution.currents[LA],
                B=solution.currents[LB],
                C=solution.currents[LC]
                }
            });
            //throw new NotImplementedException();
        }
    }
}
