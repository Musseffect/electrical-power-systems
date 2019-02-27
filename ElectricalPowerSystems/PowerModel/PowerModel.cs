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
        float distance;
        public AirLine(int node1, int node2, float distance) : base(node1, node2)
        {
            this.distance = distance;
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
        public int nodeId;
        private int retrieveNodeId(string key)
        {
            int node = nodeId;
            try
            {
                node = nodeDictionary[key];
            }
            catch (KeyNotFoundException)
            {
                nodeDictionary.Add(key, node);
                nodeId++;
                Node nd = new Node();
                nd.label = key;
                nodesList.Add(nd);
            }
            return node;
        }
        public PowerModelGraph()
        {
            nodeDictionary = new Dictionary<string, int>();
            elements = new List<Element>();
        }
        public int addLoad(string node,Complex32 resistance)
        {
            int node1Id = retrieveNodeId(node);
            nodesList[node1Id].connectedElements.Add(elements.Count);
            elements.Add(new Load(node1Id,resistance));
            return elements.Count - 1;
        }
        public int addAirLine(string node1,string node2,float distance)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            nodesList[node1Id].connectedElements.Add(elements.Count);
            nodesList[node2Id].connectedElements.Add(elements.Count);
            elements.Add(new AirLine(node1Id,node2Id,distance));
            return elements.Count - 1;
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
