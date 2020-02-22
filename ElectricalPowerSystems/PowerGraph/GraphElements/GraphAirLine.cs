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

        public AirLineScheme(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphAirLinePiSection airLine):base()
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
            int A1 = acGraph.AllocateNode();
            int B1 = acGraph.AllocateNode();
            int C1 = acGraph.AllocateNode();
            int nG = acGraph.AllocateNode();
            AddElement(acGraph.CreateResistorWithCurrent(inA, A1, airLine.R));
            AddElement(acGraph.CreateResistorWithCurrent(inB, B1, airLine.R));
            AddElement(acGraph.CreateResistorWithCurrent(inC, C1, airLine.R));
            AddElement(acGraph.CreateInductor(A1, outA, airLine.L));
            AddElement(acGraph.CreateInductor(B1, outB, airLine.L));
            AddElement(acGraph.CreateInductor(C1, outC, airLine.L));
            AddElement(acGraph.CreateCapacitor(inA, inB, airLine.Bp * 0.5f));
            AddElement(acGraph.CreateCapacitor(inB, inC, airLine.Bp * 0.5f));
            AddElement(acGraph.CreateCapacitor(inC, inA, airLine.Bp * 0.5f));
            AddElement(acGraph.CreateCapacitor(outA, outB, airLine.Bp * 0.5f));
            AddElement(acGraph.CreateCapacitor(outB, outC, airLine.Bp * 0.5f));
            AddElement(acGraph.CreateCapacitor(outC, outA, airLine.Bp * 0.5f));
            AddElement(acGraph.CreateResistor(inA, nG, airLine.G * 0.5f));
            AddElement(acGraph.CreateResistor(inB, nG, airLine.G * 0.5f));
            AddElement(acGraph.CreateResistor(inC, nG, airLine.G * 0.5f));
            AddElement(acGraph.CreateResistor(outA, nG, airLine.G * 0.5f));
            AddElement(acGraph.CreateResistor(outB, nG, airLine.G * 0.5f));
            AddElement(acGraph.CreateResistor(outC, nG, airLine.G * 0.5f));
            AddElement(acGraph.CreateCapacitor(inA, nG, airLine.B * 0.5f));
            AddElement(acGraph.CreateCapacitor(inB, nG, airLine.B * 0.5f));
            AddElement(acGraph.CreateCapacitor(inC, nG, airLine.B * 0.5f));
            AddElement(acGraph.CreateCapacitor(outA, nG, airLine.B * 0.5f));
            AddElement(acGraph.CreateCapacitor(outB, nG, airLine.B * 0.5f));
            AddElement(acGraph.CreateCapacitor(outC, nG, airLine.B * 0.5f));
            acGraph.CreateGround(nG);
        }
    }
}
