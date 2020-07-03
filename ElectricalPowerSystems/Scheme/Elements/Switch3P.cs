using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.Scheme.Interpreter;
using MathNet.Numerics;
using static ElectricalPowerSystems.Scheme.Interpreter.Interpreter;

namespace ElectricalPowerSystems.Scheme.Elements
{
    class Switch3P:Element,ITransientElement,ISteadyStateElement
    {
        bool state;
        Pin3Phase in_pin;
        Pin3Phase out_pin;
        public string IA { get { return $"I_{ID}a"; } }
        public string IB { get { return $"I_{ID}b"; } }
        public string IC { get { return $"I_{ID}c"; } }
        public string IAre { get { return $"I_{ID}a_re"; } }
        public string IBre { get { return $"I_{ID}b_re"; } }
        public string ICre { get { return $"I_{ID}c_re"; } }
        public string IAim { get { return $"I_{ID}a_im"; } }
        public string IBim { get { return $"I_{ID}b_im"; } }
        public string ICim { get { return $"I_{ID}c_im"; } }
        public Switch3P(bool state,Pin3Phase in_pin,Pin3Phase out_pin) : base()
        {
            this.state = state;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string state = $"state_{ID}";
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VA} - {out_pin.VA}) * {state} = (1.0 - {state}) * {IA};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VB} - {out_pin.VB}) * {state} = (1.0 - {state}) * {IB};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VC} - {out_pin.VC}) * {state} = (1.0 - {state}) * {IC};"
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IA,
                Node1 = in_pin.VA,
                Node2 = out_pin.VA
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IB,
                Node1 = in_pin.VB,
                Node2 = out_pin.VB
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IC,
                Node1 = in_pin.VC,
                Node2 = out_pin.VC
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string state = $"state_{ID}";
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VAre} - {out_pin.VAre}) * {state} = (1.0 - {state}) * {IAre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VAim} - {out_pin.VAim}) * {state} = (1.0 - {state}) * {IAim};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VBre} - {out_pin.VBre}) * {state} = (1.0 - {state}) * {IBre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VBim} - {out_pin.VBim}) * {state} = (1.0 - {state}) * {IBim};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VCre} - {out_pin.VCre}) * {state} = (1.0 - {state}) * {ICre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VCim} - {out_pin.VCim}) * {state} = (1.0 - {state}) * {ICim};"
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IAre,
                Node1 = in_pin.VAre,
                Node2 = out_pin.VAre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IAim,
                Node1 = in_pin.VAim,
                Node2 = out_pin.VAim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBre,
                Node1 = in_pin.VBre,
                Node2 = out_pin.VBre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBim,
                Node1 = in_pin.VBim,
                Node2 = out_pin.VBim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICre,
                Node1 = in_pin.VCre,
                Node2 = out_pin.VCre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICim,
                Node1 = in_pin.VCim,
                Node2 = out_pin.VCim
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            double stateValue = (this.state ? 1.0 : 0.0);
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant state_{ID} = {stateValue.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            double stateValue = (this.state ? 1.0 : 0.0);
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant state_{ID} = {stateValue.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    public class SteadyStateSwitch3PModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            bool state = (elementObject.GetValue("State") as BoolValue).Value;
            return new Switch3P(state, elementNodes["in"] as Pin3Phase, elementNodes["out"] as Pin3Phase);
        }
    }
    public class TransientSwitch3PModel : ITransientElementModel
    {
        public ITransientElement CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            bool state = (elementObject.GetValue("State") as BoolValue).Value;
            return new Switch3P(state, elementNodes["in"] as Pin3Phase, elementNodes["out"] as Pin3Phase);
        }
    }
}

