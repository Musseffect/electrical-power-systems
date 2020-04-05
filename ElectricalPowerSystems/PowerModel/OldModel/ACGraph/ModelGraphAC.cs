using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
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
            inductors.Add(elements.Count);
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
