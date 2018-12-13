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
    }
    class ModelGraphAC
    {
    }
}
