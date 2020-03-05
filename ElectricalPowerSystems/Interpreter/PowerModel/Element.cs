using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.PowerModel
{
    public partial class ModelInterpretator
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
        private abstract class Element
        {
            int elementIndex;
            protected int[] nodes;
            public Element(int elementIndex)
            {
                this.elementIndex = elementIndex;
            }
        }
        private interface ITransientElement
        {
            List<EquationBlock> GenerateEquations();
            List<EquationBlock> GenerateParameters();
        }
        private interface ISteadyStateElement
        {
            List<EquationBlock> GenerateEquations();
            List<EquationBlock> GenerateParameters();
            /*Complex32f GetPower(SteadyStateSolution solution)
            {

            }*/
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
        private abstract class Resistor : Element,ITransientElement, ISteadyStateElement
        {
            float resistance;
            public Resistor(float resistance,int in_node,int out_node,int elementIndex):base(elementIndex)
            {
                this.resistance = resistance;
                this.nodes = new int[] { in_node, out_node };
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
        }
        /*
         
         
         */
    }
}
