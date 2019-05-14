using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    namespace ElementsAC
    {
        public enum ElementTypeEnum
        {
            Resistor,
            Capacitor,
            Inductor,
            VoltageSource,
            CurrentSource,
            Line,
            Ground
        }
        public abstract class Element
        {
            public ElementTypeEnum ElementType { get; protected set; }
            public int[] nodes;
            public Element()
            {
                nodes = null;
            }
            public abstract string toString();
        }
        public abstract class Element2N : Element
        {
            public Element2N(int node1, int node2)
            {
                nodes = new int[] { node1, node2 };
            }
        }
        public abstract class Element1N : Element
        {
            public Element1N(int node)
            {
                nodes = new int[] { node };
            }
        }
        public class Resistor : Element2N
        {
            public float resistance;
            public Resistor(int node1, int node2, float resistance) : base(node1, node2)
            {
                this.resistance = resistance;
                this.ElementType = ElementTypeEnum.Resistor;
            }
            public override string toString()
            {
                return $"Resistor{{n1 = {nodes[0]}, n2 = {nodes[1]}, resistance = {resistance} }}";
            }
        }
        public class Capacitor : Element2N
        {
            public float capacity;
            public Capacitor(int node1, int node2, float capacity) : base(node1, node2)
            {
                this.capacity = capacity;
                this.ElementType = ElementTypeEnum.Capacitor;
            }
            public override string toString()
            {
                return $"Capacitor{{n1 = {nodes[0]}, n2 = {nodes[1]}, capacity = {capacity} }}";
            }
        }
        public class Inductor : Element2N
        {
            public float inductivity;
            public Inductor(int node1, int node2, float inductivity) : base(node1, node2)
            {
                this.inductivity = inductivity;
                this.ElementType = ElementTypeEnum.Inductor;
            }
            public override string toString()
            {
                return $"Inductor{{n1 = {nodes[0]}, n2 = {nodes[1]}, inductivity = {inductivity} }}";
            }
        }
        public class Ground : Element1N
        {
            public Ground(int node) : base(node)
            {
                this.ElementType = ElementTypeEnum.Ground;
            }
            public override string toString()
            {
                return $"Ground{{n = {nodes[0]} }}";
            }
        }
        public class Line : Element2N
        {
            public Line(int node1, int node2) : base(node1, node2)
            {
                this.ElementType = ElementTypeEnum.Line;
            }
            public override string toString()
            {
                return $"Line{{n1 = {nodes[0]}, n2 = {nodes[1]} }}";
            }
        }
        public class VoltageSource : Element2N
        {
            public float voltage;
            public float phase;
            public float frequency;
            public VoltageSource(int node1, int node2, float voltage, float phase, float frequency) : base(node1, node2)
            {
                this.voltage = voltage;
                this.frequency = frequency;
                this.phase = phase;
                this.ElementType = ElementTypeEnum.VoltageSource;
            }
            public override string toString()
            {
                return $"Voltage Source{{n1 = {nodes[0]}, n2 = {nodes[1]}, voltage = {voltage}@{MathUtils.degrees(phase)}, freq = {frequency} }}";
            }
        }
        public class CurrentSource : Element2N
        {
            public float current;
            public float phase;
            public float frequency;
            public CurrentSource(int node1, int node2, float current, float phase, float frequency) : base(node1, node2)
            {
                this.current = current;
                this.frequency = frequency;
                this.phase = phase;
                this.ElementType = ElementTypeEnum.CurrentSource;
            }
            public override string toString()
            {
                return $"Current Source{{n1 = {nodes[0]}, n2 = {nodes[1]}, current = {current}@{MathUtils.degrees(phase)}, freq = {frequency} }}";
            }
        }
    }
    public class ModelGraphCreatorAC
    {
        Dictionary<string, int> nodes;
        public List<ElementsAC.Element> elements;
        public int nodeId;
        public List<int> voltageSources;
        public List<int> currentSources;
        public List<int> inductors;
        public List<Node> nodesList;
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
        public List<NodePair> outputVoltageDifference;
        public List<int> outputNodeVoltage;
        public List<int> outputCurrent;
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
        public string getElementString(int index)
        {
            try {
                return elements[index].toString();
            }
            catch(Exception)
            {
                return "Invalid element index";
            }
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
            outputVoltageDifference = new List<NodePair>();
            outputCurrent = new List<int>();
            outputNodeVoltage = new List<int>();
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
        public int addVoltageSource(string node1, string node2, float voltage,float phase, float freq)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            nodesList[node1Id].connectedElements.Add(elements.Count);
            nodesList[node2Id].connectedElements.Add(elements.Count);
            voltageSources.Add(elements.Count);
            elements.Add(new ElementsAC.VoltageSource(node1Id, node2Id, voltage, phase, freq));
            return elements.Count - 1;
        }
        public int addCurrentSource(string node1, string node2, float current, float phase, float freq)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            nodesList[node1Id].connectedElements.Add(elements.Count);
            nodesList[node2Id].connectedElements.Add(elements.Count);
            currentSources.Add(elements.Count);
            elements.Add(new ElementsAC.CurrentSource(node1Id, node2Id, current, phase, freq));
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
        public int addInductor(string node1, string node2, float inductivity)
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
        public void addCurrentOutput(int elementIndex)
        {

            if (elementIndex < elements.Count)
                throw new Exception("Incorrect element index.");
            outputCurrent.Add(elementIndex);
        }
        public void addVoltageOutput(int elementIndex)
        {
            if (elementIndex < elements.Count)
                throw new Exception("Incorrect element index.");
            outputNodeVoltage.Add(elementIndex);
        }
        public void addVoltageOutput(string node1,string node2)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            outputVoltageDifference.Add(new NodePair(node1Id,node2Id));
        }
    }
}
