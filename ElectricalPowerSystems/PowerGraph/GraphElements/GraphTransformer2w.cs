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
        public Complex32 Zp;
        public float Rc;
        public float Xm;
        public Complex32 Zs;
        public TransformerWinding winding1;
        public TransformerWinding winding2;
        public GraphTransformer2w(string node1, string node2,float kTrans,Complex32 Zp,float Rc,float Xm,Complex32 Zs, TransformerWinding winding1, TransformerWinding winding2) :base()
        {
            nodes.Add(node1);
            nodes.Add(node2);
            this.kTrans = kTrans;
            this.Zp = Zp;
            this.Rc = Rc;
            this.Xm = Xm;
            this.Zs = Zs;
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

        public Transformer2wScheme(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphTransformer2w t):base()
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
            /*w1A = inA;
            w1B = inB;
            w1C = inC;
            w2A = outA;
            w2B = outB;
            w2C = outC;*/

            int w1A = acGraph.allocateNode();
            int w1B = acGraph.allocateNode();
            int w1C = acGraph.allocateNode();
            int w2A = acGraph.allocateNode();
            int w2B = acGraph.allocateNode();
            int w2C = acGraph.allocateNode();

            int[] node1 = t.winding1.generateWinding(inA, inB, inC, acGraph);
            int[] node2 = t.winding2.generateWinding(outA, outB, outC, acGraph);

            acGraph.createTransformer(w1A, node1[1], w2A, node2[1], t.kTrans);
            acGraph.createTransformer(w1B, node1[3], w2B, node2[3], t.kTrans);
            acGraph.createTransformer(w1C, node1[5], w2C, node2[5], t.kTrans);

            AddElement(acGraph.createImpedanceWithCurrent(node1[0], w1A, t.Zp));
            AddElement(acGraph.createImpedanceWithCurrent(node1[2], w1B, t.Zp));
            AddElement(acGraph.createImpedanceWithCurrent(node1[4], w1C, t.Zp));

            AddElement(acGraph.createResistor(w1A, node1[1], t.Rc));
            AddElement(acGraph.createResistor(w1B, node1[3], t.Rc));
            AddElement(acGraph.createResistor(w1C, node1[5], t.Rc));

            AddElement(acGraph.createInductor(w1A, node1[1], t.Xm/PowerGraphManager.powerFrequency));
            AddElement(acGraph.createInductor(w1B, node1[3], t.Xm/PowerGraphManager.powerFrequency));
            AddElement(acGraph.createInductor(w1C, node1[5], t.Xm/PowerGraphManager.powerFrequency));

            AddElement(acGraph.createImpedanceWithCurrent(node2[0], w2A, t.Zs));
            AddElement(acGraph.createImpedanceWithCurrent(node2[2], w2B, t.Zs));
            AddElement(acGraph.createImpedanceWithCurrent(node2[4], w2C, t.Zs));
        }
    }
}
