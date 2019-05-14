using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{
    class GraphTransformer2w : GraphElement
    {
        float kTrans;
        Complex32 R1;
        Complex32 R2;
        GraphTransformer2w(string node1, string node2,float kTrans,Complex32 R1,Complex32 R2):base()
        {
            nodes.Add(node1);
            nodes.Add(node2);
            this.kTrans = kTrans;
            this.R1 = R1;
            this.R2 = R2;
        }
        public override List<bool> getPhaseNodes()
        {
            return new List<bool>() { true,true };
        }
    }
}
