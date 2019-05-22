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
        float kTrans;
        Complex32 Z1;
        Complex32 Z2;
        Mode mode1;
        Mode mode2;
        int group;
        public GraphTransformer2w(string node1, string node2,float kTrans,Complex32 Z1,Complex32 Z2,Mode mode1,Mode mode2,int group):base()
        {
            nodes.Add(node1);
            nodes.Add(node2);
            this.kTrans = kTrans;
            this.Z1 = Z1;
            this.Z2 = Z2;
            this.group = group;
            this.mode1 = mode1;
            this.mode2 = mode2;
        }
        public override void generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            throw new NotImplementedException();
        }
        public override List<bool> getPhaseNodes()
        {
            return new List<bool>() { true,true };
        }
    }
}
