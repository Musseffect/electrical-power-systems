using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.ACGraph
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
            Transformer,
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
        public abstract class Element4N : Element
        {
            public Element4N(int node1, int node2, int node3, int node4)
            {
                nodes = new int[] { node1, node2, node3, node4 };
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
        public class Transformer : Element4N
        {
            public float b;
            public Transformer(int node1, int node2, int node3, int node4, float b) : base(node1, node2, node3, node4)
            {
                this.b = b;
                this.ElementType = ElementTypeEnum.Transformer;
            }
            public override string toString()
            {
                return $"Transformer{{n1 = {nodes[0]}, n2 = {nodes[1]}, n3 = {nodes[2]}, n4 = {nodes[3]}, b = {b} }}";
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
}
