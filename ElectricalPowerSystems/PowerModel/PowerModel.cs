using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel
{
    public enum ElementType
    {
        Generator,
        AirLine,
        ConnectionLine,
        TwoWindingTransformer,
        Load
    }
    public abstract class Element
    {
        public int[] nodes;
        public Element()
        {
            nodes = null;
        }
        public abstract ElementType getType();
    }
    abstract class Element3N : Element
    {
        public Element3N(int node1, int node2,int node3)
        {
            nodes = new int[] { node1, node2, node3 };
        }
    }
    abstract class Element2N : Element
    {
        public Element2N(int node1, int node2)
        {
            nodes = new int[] { node1, node2 };
        }
    }
    abstract class Element1N : Element
    {
        public Element1N(int node)
        {
            nodes = new int[] { node };
        }
    }
    class Load:Element1N
    {
        public Complex32 power;
        public Load(int node, Complex32 power):base(node)
        {
            this.power = power;
        }
        public override ElementType getType()
        {
            return ElementType.Load;
        }
    }
    class Generator : Element1N
    {
        public float power;
        public Generator(int node, float power):base(node)
        {
            this.power = power;
        }
        public override ElementType getType()
        {
            return ElementType.Generator;
        }
    }
    class TwoWindingTransformer:Element2N
    {
        float ratio;
        public TwoWindingTransformer(int node1,int node2, float ratio):base(node1,node2)
        {
            this.ratio = ratio;
        }
        public override ElementType getType()
        {
            return ElementType.TwoWindingTransformer;
        }
    }
    class ConnectionLine : Element2N
    {
        public ConnectionLine(int node1, int node2) : base(node1, node2)
        {
        }
        public override ElementType getType()
        {
            return ElementType.ConnectionLine;
        }
    }
    class AirLine : Element2N
    {
        float resistance;
        public AirLine(int node1, int node2, float resistance) : base(node1, node2)
        {
            this.resistance = resistance;
        }
        public override ElementType getType()
        {
            return ElementType.AirLine;
        }
    }
    class PowerModelGraph
    {
        List<Element> elements;
        Dictionary<string, int> nodeDictionary;
        public List<Node> nodesList;
        public PowerModelGraph()
        {
            nodeDictionary = new Dictionary<string, int>();
            elements = new List<Element>();
        }
        public int addLoad(string node)
        {
            throw new NotImplementedException();
        }
        public int addAirLine()
        {
            throw new NotImplementedException();
        }
        public int addConnectionLine()
        {
            throw new NotImplementedException();
        }
        public int addGenerator()
        {
            throw new NotImplementedException();
        }
        public int addTwoWindingTransformer()
        {
            throw new NotImplementedException();
        }
    }
}
