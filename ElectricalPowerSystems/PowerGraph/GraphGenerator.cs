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
    public class GraphGeneratorVZWye : GraphElement
    {
        public float voltage;
        public float phase;
        public GroundingType grounding;
        public Complex32 impedance;
        public GraphGeneratorVZWye(string node, float voltage, float phase,Complex32 impedance, GroundingType grounding):base()
        {
            nodes.Add(node);
            this.voltage = voltage;
            this.phase = phase;
            this.impedance = impedance;
            this.grounding = grounding;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            throw new NotImplementedException();
        }

    }
    public class GraphGeneratorVZDelta : GraphElement
    {
        public float voltage;
        public float phase;
        public Complex32 impedance;
        public GraphGeneratorVZDelta(string node, float voltage, float phase, Complex32 impedance) : base()
        {
            nodes.Add(node);
            this.voltage = voltage;
            this.phase = phase;
            this.impedance = impedance;
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
        public GroundingType grounding;
        public GraphGeneratorVWye(string node,float voltage,float phase, GroundingType grounding) :base()
        {
            nodes.Add(node);
            this.voltage = voltage;
            this.phase = phase;
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
        public GraphGeneratorVDelta(string node, float voltage, float phase) : base()
        {
            nodes.Add(node);
            this.voltage = voltage;
            this.phase = phase;
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

        int VA;
        int VB;
        int VC;
        public GeneratorSchemeVWye(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphGeneratorVWye generator)
        {
            outA = nodes[0].A;
            outB = nodes[0].B;
            outC = nodes[0].C;
            outN = acGraph.allocateNode();
            generate(acGraph, generator);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphGeneratorVWye generator)
        {
            VA = acGraph.createVoltageSource(outN, outA, generator.voltage, 0.0f, PowerGraphManager.powerFrequency);
            VB = acGraph.createVoltageSource(outN, outB, generator.voltage, (float)(Math.PI) * 2.0f / 3.0f, PowerGraphManager.powerFrequency);
            VC = acGraph.createVoltageSource(outN, outC, generator.voltage, (float)(Math.PI) * 4.0f / 3.0f, PowerGraphManager.powerFrequency);
            generator.grounding.createScheme(acGraph,outN);
        }
        override public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution)
        {
            Complex32 opA = solution.currents[VA] * (solution.voltages[outA] - solution.voltages[outN]);
            Complex32 opB = solution.currents[VB] * (solution.voltages[outB] - solution.voltages[outN]);
            Complex32 opC = solution.currents[VC] * (solution.voltages[outC] - solution.voltages[outN]);
            PowerGraphManager.ABCValue power = new PowerGraphManager.ABCValue();
            power.A = opA;
            power.B = opB;
            power.C = opC;
            result.powers.Add(power);
            result.currents.Add(new PowerGraphManager.ABCValue[] {
                new PowerGraphManager.ABCValue{
                A=solution.currents[VA],
                B=solution.currents[VB],
                C=solution.currents[VC]
                }
            });
        }
    }
    public class GeneratorSchemeVDelta : PowerElementScheme
    {
        int outA;
        int outB;
        int outC;
        int VAB;
        int VBC;
        int VCA;
        public GeneratorSchemeVDelta(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphGeneratorVDelta generator)
        {
            outA = nodes[0].A;
            outB = nodes[0].B;
            outC = nodes[0].C;
            generate(acGraph, generator);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphGeneratorVDelta generator)
        {
            acGraph.createVoltageSource(outC, outA, generator.voltage, 0.0f, PowerGraphManager.powerFrequency);
            acGraph.createVoltageSource(outA, outB, generator.voltage, (float)(Math.PI) * 2.0f / 3.0f, PowerGraphManager.powerFrequency);
            acGraph.createVoltageSource(outB, outC, generator.voltage, (float)(Math.PI) * 4.0f / 3.0f, PowerGraphManager.powerFrequency);
        }
        override public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution)
        {
            Complex32 opA = solution.currents[VCA] * (solution.voltages[outA] - solution.voltages[outC]);
            Complex32 opB = solution.currents[VAB] * (solution.voltages[outB] - solution.voltages[outA]);
            Complex32 opC = solution.currents[VBC] * (solution.voltages[outC] - solution.voltages[outB]);
            PowerGraphManager.ABCValue power = new PowerGraphManager.ABCValue();
            power.A = opA;
            power.B = opB;
            power.C = opC;
            result.powers.Add(power);
            result.currents.Add(new PowerGraphManager.ABCValue[] {
                new PowerGraphManager.ABCValue{
                A= solution.currents[VCA] - solution.currents[VAB],
                B= solution.currents[VAB] - solution.currents[VBC],
                C= solution.currents[VBC] - solution.currents[VCA]
                }
            });
        }
    }
}
