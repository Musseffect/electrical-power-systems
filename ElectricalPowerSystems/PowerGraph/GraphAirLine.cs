using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.ACGraph;
using MathNet.Numerics;

namespace ElectricalPowerSystems.PowerGraph
{
    public class GraphAirLine : GraphElement
    {
        float R;
        float L;
        float B;
        float G;
        float Bp;
        public GraphAirLine(string node1,string node2,float R,float L,float B,float G,float Bp) : base()
        {
            nodes.Add(node1);
            nodes.Add(node2);
            this.R = R;
            this.L = L;
            this.B = B;
            this.G = G;
            this.Bp = Bp;
        }
        public override void generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            int A1 = acGraph.allocateNode();
            int B1 = acGraph.allocateNode();
            int C1 = acGraph.allocateNode();
            int nG = acGraph.allocateNode();
            acGraph.createResistor(nodes[0].A,A1, R);
            acGraph.createResistor(nodes[0].B,B1, R);
            acGraph.createResistor(nodes[0].C,C1, R);
            acGraph.createInductor(A1,nodes[1].A, L);
            acGraph.createInductor(B1,nodes[1].B, L);
            acGraph.createInductor(C1,nodes[1].C, L);
            acGraph.createCapacitor(nodes[0].A, nodes[0].B, Bp*0.5f);
            acGraph.createCapacitor(nodes[0].B, nodes[0].C, Bp*0.5f);
            acGraph.createCapacitor(nodes[0].C, nodes[0].A, Bp*0.5f);
            acGraph.createCapacitor(nodes[1].A, nodes[1].B, Bp*0.5f);
            acGraph.createCapacitor(nodes[1].B, nodes[1].C, Bp*0.5f);
            acGraph.createCapacitor(nodes[1].C, nodes[1].A, Bp * 0.5f);
            acGraph.createResistor(nodes[0].A, nG, G*0.5f);
            acGraph.createResistor(nodes[0].B, nG, G*0.5f);
            acGraph.createResistor(nodes[0].C, nG, G*0.5f);
            acGraph.createResistor(nodes[1].A, nG, G*0.5f);
            acGraph.createResistor(nodes[1].B, nG, G*0.5f);
            acGraph.createResistor(nodes[1].C, nG, G * 0.5f);
            acGraph.createCapacitor(nodes[0].A, nG, B*0.5f);
            acGraph.createCapacitor(nodes[0].B, nG, B*0.5f);
            acGraph.createCapacitor(nodes[0].C, nG, B*0.5f);
            acGraph.createCapacitor(nodes[1].A, nG, B*0.5f);
            acGraph.createCapacitor(nodes[1].B, nG, B*0.5f);
            acGraph.createCapacitor(nodes[1].C, nG, B*0.5f);
            acGraph.createGround(nG);
        }
        public override List<bool> getPhaseNodes()
        {
            return new List<bool>() { false, false };
        }
    }
}
