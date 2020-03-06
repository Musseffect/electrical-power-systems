using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.PowerModel
{
#if MODELINTERPRETER
    public partial class ModelInterpreter
    {
        /*
            Elements:
            Resistor
            Inductor
            1pTransformer
            1pAutotransformer
            GeneratorWye
            GeneratorDelta
            LoadWye
            LoadDelta
            TransformerDeltaDelta
            TransformerDeltaWye
            TransformerWyeWye
            TransmissionLine
            Ground
            Capacitor
            Switch
            Wattmeter
            Fault
            Line
            3Phase1PhaseAdapter
                */
        public class ABCNode
        {
            public int A;
            public int B;
            public int C;
        }
        public class EquationBlock
        {
            public string Equation;
        }
        //left side of f(x)=I
        //for source voltage it is Ie=Ielement
        //for resistance it is Y(v1-v2)=Ielement
        //each current equation block creates two equation
        public class CurrentFlowBlock : EquationBlock
        {
            public string Node1;
            public string Node2;
        }
        public abstract class Element
        {
            int elementIndex;
            public Element(int elementIndex)
            {
                this.elementIndex = elementIndex;
            }
        }
        public interface ITransientElement
        {
            List<EquationBlock> GenerateEquations();
            List<EquationBlock> GenerateParameters();
        }
        public interface ISteadyStateElement
        {
            List<EquationBlock> GenerateEquations();
            List<EquationBlock> GenerateParameters();
            Complex32 GetPower(SteadyStateSolution solution);
        }
        /*
        private abstract class Inductor : Element, ITransientElement, ISteadyStateElement
        {
        }
        private abstract class Ground : Element, ITransientElement, ISteadyStateElement
        {
        }
        private abstract class Inductor : Element, ITransientElement, ISteadyStateElement
        {
        }
        private abstract class Capacitor : Element, ITransientElement, ISteadyStateElement
        {
        }
        private abstract class VoltageSource : Element, ITransientElement, ISteadyStateElement
        {
        }
        private abstract class Transformer2w : Element, ITransientElement, ISteadyStateElement
        {
        }
        private abstract class CurrentSource : Element, ITransientElement, ISteadyStateElement
        {
        }
        */
        public abstract class Resistor : Element, ITransientElement, ISteadyStateElement
        {
            float resistance;
            int in_node;
            int out_node;
            public Resistor(float resistance, int in_node, int out_node, int elementIndex) : base(elementIndex)
            {
                this.resistance = resistance;
                this.in_node = in_node;
                this.out_node = out_node;
            }
            List<EquationBlock> ITransientElement.GenerateEquations()
            {
                throw new NotImplementedException();
            }
            List<EquationBlock> ITransientElement.GenerateParameters()
            {
                throw new NotImplementedException();
            }
            List<EquationBlock> ISteadyStateElement.GenerateEquations()
            {
                throw new NotImplementedException();
            }
            List<EquationBlock> ISteadyStateElement.GenerateParameters()
            {
                throw new NotImplementedException();
            }
            public Complex32 ISteadyStateElement.GetPower(SteadyStateSolution solution)
            {
                throw new NotImplementedException();
            }
        }
        public class SteadyStateResistorModel : ISteadyStateElementModel
        {
            public ISteadyStateElement ISteadyStateElementModel.CreateElement(Object elementObject)
            {
                throw new NotImplementedException();
            }
        }
        public abstract class VoltageSource : Element, ITransientElement, ISteadyStateElement
        {
            float amp;
            float phase;
            float frequency;
            int in_node;
            int out_node;
            public VoltageSource(float amp, float phase, float frequency, int in_node, int out_node, int elementIndex) : base(elementIndex)
            {
                this.amp = amp;
                this.phase = phase;
                this.frequency = frequency;
                this.in_node = in_node;
                this.out_node = out_node;
            }
            List<EquationBlock> ITransientElement.GenerateEquations()
            {
                throw new NotImplementedException();
            }
            List<EquationBlock> ITransientElement.GenerateParameters()
            {
                throw new NotImplementedException();
            }
            List<EquationBlock> ISteadyStateElement.GenerateEquations()
            {
                throw new NotImplementedException();
            }
            List<EquationBlock> ISteadyStateElement.GenerateParameters()
            {
                throw new NotImplementedException();
            }
            public Complex32 ISteadyStateElement.GetPower(SteadyStateSolution solution)
            {
                throw new NotImplementedException();
            }
        }
        public class SteadyStateVoltageSourceModel : ISteadyStateElementModel
        {
            public ISteadyStateElement CreateElement(Object elementObject)
            {
                throw new NotImplementedException();
            }
        }
    }
#endif
}