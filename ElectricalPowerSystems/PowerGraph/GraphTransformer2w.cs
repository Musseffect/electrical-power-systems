using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{
    public enum TransformerMode
    {
        D1 = 1,
        D3 = 3,
        D5 = 5,
        D7 = 7,
        D9 = 9,
        D11 = 11,
        Y0 = 0,
        Y2 = 2,
        Y4 = 4,
        Y6 = 6,
        Y8 = 8,
        Y10 = 10
    }
    public class GraphTransformer2w : GraphElement
    {
        public float kTrans;
        public Complex32 Z1;
        public Complex32 Z2;
        public TransformerMode mode1;
        public TransformerMode mode2;
        public GraphTransformer2w(string node1, string node2,float kTrans,Complex32 Z1,Complex32 Z2, TransformerMode mode1, TransformerMode mode2):base()
        {
            nodes.Add(node1);
            nodes.Add(node2);
            this.kTrans = kTrans;
            this.Z1 = Z1;
            this.Z2 = Z2;
            this.mode1 = mode1;
            this.mode2 = mode2;
        }
        static public int[] generateWindingConnection(TransformerMode mode,ABCNode node)
        {
            int a, b, c, n;
            a = node.A;
            b = node.B;
            c = node.C;
            n = node.N;
            switch (mode)
            {
                case TransformerMode.D1:
                        return new int[]{a,c,
                            b,a,
                            c,b};
                case TransformerMode.D3:
                        return new int[]{b,c,
                            c,a,
                            a,b};
                case TransformerMode.D5:
                        return new int[]{b,a,
                            c,b,
                            a,c};
                case TransformerMode.D7:
                        return new int[]{b,c,
                            c,a,
                            a,b};
                case TransformerMode.D9:
                        return new int[]{c,b,
                            a,c,
                            b,a};
                case TransformerMode.D11:
                        return new int[]{a,b,
                            b,c,
                            c,a};
                case TransformerMode.Y0:
                        return new int[]{a,n,
                            b,n,
                            c,n};
                case TransformerMode.Y2:
                        return new int[]{n,c,
                            n,a,
                            n,b};
                case TransformerMode.Y4:
                        return new int[]{b,n,
                            c,n,
                            a,n};
                case TransformerMode.Y6:
                        return new int[]{n,a,
                            n,b,
                            n,c};
                case TransformerMode.Y8:
                        return new int[]{c,n,
                            a,n,
                            b,n};
                    break;
                case TransformerMode.Y10:
                        return new int[]{n,b,
                            n,c,
                            n,a};
            }
            throw new Exception("Mode is'nt specified in GraphTransformer2w");
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new Transformer2wScheme(nodes,acGraph,this);
            int[] node1 = generateWindingConnection(mode1, nodes[0]);
            int[] node2 = generateWindingConnection(mode2, nodes[1]);
            acGraph.createTransformer(node1[0], node1[1], node2[0], node2[1], kTrans);
            acGraph.createTransformer(node1[2], node1[3], node2[2], node2[3], kTrans);
            acGraph.createTransformer(node1[4], node1[5], node2[4], node2[5], kTrans);
            throw new NotImplementedException();
        }
        public override List<bool> getPhaseNodes()
        {
            return new List<bool>() { (Convert.ToInt32(mode1)%2) == 0, (Convert.ToInt32(mode2) % 2) == 0 };
        }
    }
}
