using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    namespace ElementsAC
    {
        class Element
        {
            public int[] nodes;
            public Element()
            {
                nodes = null;
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
        class Resistor : Element2N
        {
            public float resistance;
            public Resistor(int node1, int node2, float resistance) : base(node1, node2)
            {
                this.resistance = resistance;
            }
        }
        class Capacitor : Element2N
        {
            public float capacity;
            public Capacitor(int node1, int node2, float capacity) : base(node1, node2)
            {
                this.capacity = capacity;
            }
        }
        class Inductor : Element2N
        {
            public float inductivity;
            public Inductor(int node1, int node2, float inductivity) : base(node1, node2)
            {
                this.inductivity = inductivity;
            }
        }
        class Ground : Element1N
        {
            public Ground(int node) : base(node)
            {
            }
        }
        class Line : Element2N
        {
            public Line(int node1, int node2) : base(node1, node2)
            {
            }
        }
        class VoltageSource : Element2N
        {
            public float voltage;
            public float phase;
            public float frequency;
            public VoltageSource(int node1, int node2, float voltage,float frequency, float phase) : base(node1, node2)
            {
                this.voltage = voltage;
                this.frequency = frequency;
                this.phase = phase;
            }
        }
        class CurrentSource : Element2N
        {
            public float current;
            public float phase;
            public float frequency;
            public CurrentSource(int node1, int node2, float current,float frequency,float phase) : base(node1, node2)
            {
                this.current = current;
                this.frequency = frequency;
                this.phase = phase;
            }
        }
    }
    class ModelGraphCreatorAC
    {
        Dictionary<string, int> nodes;
        public List<ElementsAC.Element> elements;
        public int nodeId;
        public List<int> voltageSources;
        public List<int> currentSources;
        public List<int> inductors;
        public List<Node> nodesList;
        public List<int> outputCurrents;
        public List<int> lines;
        public int groundsCount;
        public class NodePair
        {
            public int node1;
            public int node2;
            public NodePair(int node1, int node2)
            {
                this.node1 = node1;
                this.node2 = node2;
            }
        }
        List<NodePair> outputVoltageDifference;
        List<int> outputNodeVoltage;
        private int retrieveNodeId(string key)
        {
            int node = nodeId;
            try
            {
                node = nodes[key];
            }
            catch (KeyNotFoundException)
            {
                nodes.Add(key, node);
                nodeId++;
                Node nd = new Node();
                nd.label = key;
                nodesList.Add(nd);
            }
            return node;
        }
        public ModelGraphCreatorAC()
        {
            nodes = new Dictionary<string, int>();
            elements = new List<ElementsAC.Element>();
            lines = new List<int>();
            inductors = new List<int>();
            nodeId = 0;
            voltageSources = new List<int>();
            currentSources = new List<int>();
            nodesList = new List<Node>();
            groundsCount = 0;
        }
        public int addResistor(string node1, string node2, float resistance)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            nodesList[node1Id].connectedElements.Add(elements.Count);
            nodesList[node2Id].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.Resistor(node1Id, node2Id, resistance));
            return elements.Count - 1;
        }
        public int addLine(string node1, string node2)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            nodesList[node1Id].connectedElements.Add(elements.Count);
            nodesList[node2Id].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.Line(node1Id, node2Id));
            return elements.Count - 1;
        }
        public int addCapacitor(string node1, string node2, float capacity)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            nodesList[node1Id].connectedElements.Add(elements.Count);
            nodesList[node2Id].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.Capacitor(node1Id, node2Id, capacity));
            return elements.Count - 1;
        }
        public int addVoltageSource(string node1, string node2, float voltage,float freq,float phase)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            nodesList[node1Id].connectedElements.Add(elements.Count);
            nodesList[node2Id].connectedElements.Add(elements.Count);
            voltageSources.Add(elements.Count);
            elements.Add(new ElementsAC.VoltageSource(node1Id, node2Id, voltage,freq,MathUtils.radians(phase)));
            return elements.Count - 1;
        }
        public int addCurrentSource(string node1, string node2, float current, float freq, float phase)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            nodesList[node1Id].connectedElements.Add(elements.Count);
            nodesList[node2Id].connectedElements.Add(elements.Count);
            currentSources.Add(elements.Count);
            elements.Add(new ElementsAC.CurrentSource(node1Id, node2Id, current,freq,phase));
            return elements.Count - 1;
        }
        public void addGround(string node)
        {
            int nodeId = retrieveNodeId(node);
            nodesList[nodeId].connectedElements.Add(elements.Count);
            nodesList[nodeId].grounded = true;
            elements.Add(new ElementsAC.Ground(nodeId));
            groundsCount++;
        }
        public int addInductor(string node1, string node2, int inductivity)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            nodesList[node1Id].connectedElements.Add(elements.Count);
            nodesList[node2Id].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.Inductor(node1Id, node2Id, inductivity));
            return elements.Count - 1;
        }
        public bool validate(ref List<string> errors)
        {
            return true;
        }
    }
}
