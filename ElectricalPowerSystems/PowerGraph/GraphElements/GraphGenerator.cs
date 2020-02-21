using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.ACGraph;
using MathNet.Numerics;

namespace ElectricalPowerSystems.PowerGraph
{
    public class GraphGeneratorPQWye : GraphElement
    {
        public Complex32 S;
        public GroundingType grounding;
        public GraphGeneratorPQWye(string node, Complex32 S, GroundingType grounding) : base()
        {
            nodes.Add(node);
            this.S = S;
            this.grounding = grounding;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            throw new NotImplementedException();
        }
    }
    public class GraphGeneratorPQDelta : GraphElement
    {
        public Complex32 S;
        public GraphGeneratorPQDelta(string node, Complex32 S) : base()
        {
            nodes.Add(node);
            this.S = S;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            throw new NotImplementedException();
        }
    }
    public class GraphGeneratorPVWye:GraphElement
    {
        public float power;
        public float voltage;
        public GroundingType grounding;
        public GraphGeneratorPVWye(string node, float voltage, float power, GroundingType grounding):base()
        {
            nodes.Add(node);
            this.voltage = voltage;
            this.power = power;
            this.grounding = grounding;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            throw new NotImplementedException();
        }
    }
    public class GraphGeneratorPVDelta : GraphElement
    {
        public float power;
        public float voltage;
        public GraphGeneratorPVDelta(string node, float voltage, float power) : base()
        {
            nodes.Add(node);
            this.voltage = voltage;
            this.power = power;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            throw new NotImplementedException();
        }
    }
    public class GraphGeneratorVWye:GraphElement
    {
        public float voltage;
        public float phase;
        public Complex32 impedance;
        public GroundingType grounding;
        public GraphGeneratorVWye(string node,float voltage,float phase,Complex32 impedance, GroundingType grounding) :base()
        {
            nodes.Add(node);
            this.voltage = voltage;
            this.phase = phase;
            this.impedance = impedance;
            this.grounding = grounding;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes,ACGraph.ACGraph acGraph)
        {
            return new GeneratorSchemeVWye(nodes, acGraph, this);
        }
    }
    public class GraphGeneratorVDelta : GraphElement
    {
        public float voltage;
        public float phase;
        public Complex32 impedance;
        public GraphGeneratorVDelta(string node, float voltage, float phase, Complex32 impedance) : base()
        {
            nodes.Add(node);
            this.voltage = voltage;
            this.phase = phase;
            this.impedance = impedance;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new GeneratorSchemeVDelta(nodes, acGraph, this);
        }
    }
    public class GeneratorSchemeVWye : PowerElementScheme
    {
        int outA;
        int outB;
        int outC;
        int outN;
        public GeneratorSchemeVWye(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphGeneratorVWye generator) : base()
        {
            outA = nodes[0].A;
            outB = nodes[0].B;
            outC = nodes[0].C;
            outN = acGraph.allocateNode();
            generate(acGraph, generator);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphGeneratorVWye generator)
        {
            int wA = acGraph.allocateNode();
            int wB = acGraph.allocateNode();
            int wC = acGraph.allocateNode();
            AddElement(acGraph.createImpedanceWithCurrent(wA, outA, generator.impedance));
            AddElement(acGraph.createImpedanceWithCurrent(wB, outB, generator.impedance));
            AddElement(acGraph.createImpedanceWithCurrent(wC, outC, generator.impedance));
            AddElement(acGraph.createVoltageSource(outN, wA, generator.voltage, 0.0f, PowerGraphManager.powerFrequency));
            AddElement(acGraph.createVoltageSource(outN, wB, generator.voltage, (float)(Math.PI) * 2.0f / 3.0f, PowerGraphManager.powerFrequency));
            AddElement(acGraph.createVoltageSource(outN, wC, generator.voltage, (float)(Math.PI) * 4.0f / 3.0f, PowerGraphManager.powerFrequency));
            generator.grounding.createScheme(acGraph,outN);
        }
    }
    public class GeneratorSchemeVDelta : PowerElementScheme
    {
        int outA;
        int outB;
        int outC;
        public GeneratorSchemeVDelta(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphGeneratorVDelta generator) : base()
        {
            outA = nodes[0].A;
            outB = nodes[0].B;
            outC = nodes[0].C;
            generate(acGraph, generator);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphGeneratorVDelta generator)
        {
            int wAB = acGraph.allocateNode();
            int wBC = acGraph.allocateNode();
            int wCA = acGraph.allocateNode();
            AddElement(acGraph.createImpedanceWithCurrent(wCA, outA, generator.impedance));
            AddElement(acGraph.createImpedanceWithCurrent(wAB, outB, generator.impedance));
            AddElement(acGraph.createImpedanceWithCurrent(wBC, outC, generator.impedance));
            AddElement(acGraph.createVoltageSource(outC, wCA, generator.voltage, 0.0f, PowerGraphManager.powerFrequency));
            AddElement(acGraph.createVoltageSource(outA, wAB, generator.voltage, (float)(Math.PI) * 2.0f / 3.0f, PowerGraphManager.powerFrequency));
            AddElement(acGraph.createVoltageSource(outB, wBC, generator.voltage, (float)(Math.PI) * 4.0f / 3.0f, PowerGraphManager.powerFrequency));
        }
    }
}
