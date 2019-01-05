using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    class PowerModel
    {
        class Element
        {
            public int[] nodes;
            public Element()
            {
                nodes = null;
            }
        }
        class Element3N : Element
        {
            public Element3N(int node1, int node2,int node3)
            {
                nodes = new int[] { node1, node2, node3 };
            }
        }
        class Element2N : Element
        {
            public Element2N(int node1, int node2)
            {
                nodes = new int[] { node1, node2 };
            }
        }
        class Element1N : Element
        {
            public Element1N(int node)
            {
                nodes = new int[] { node };
            }
        }
        class Load:Element1N
        {
            public float power;
            public float rPower;
            public Load(int node,float power,float rPower):base(node)
            {
                this.power = power;
                this.rPower = rPower;
            }
        }
        class Generator : Element1N
        {
            public float power;
            public Generator(int node, float power):base(node)
            {
                this.power = power;
            }
        }
        class TwoWindingTransformer:Element2N
        {
            float ratio;
            public TwoWindingTransformer(int node1,int node2, float ratio):base(node1,node2)
            {
                this.ratio = ratio;
            }
        }
        class ConnectionLine : Element2N
        {
            public ConnectionLine(int node1, int node2) : base(node1, node2)
            {
            }
        }
        class AirLine : Element2N
        {
            float resistance;
            public AirLine(int node1, int node2, float resistance) : base(node1, node2)
            {
                this.resistance = resistance;
            }
        }
    }
}
