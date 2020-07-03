using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.Scheme.Interpreter;
using static ElectricalPowerSystems.Scheme.Interpreter.Interpreter;

namespace ElectricalPowerSystems.Scheme.Elements
{
    class Switch2Way1P : Element, ISteadyStateElement, ITransientElement
    {
        bool state;
        Pin1Phase in_pin;
        Pin1Phase out1_pin;
        Pin1Phase out2_pin;
        string I12 { get { return $"I_{ID}12"; } }
        string I13 { get { return $"I_{ID}13"; } }
        string I12re { get { return $"I_{ID}12_re"; } }
        string I13re { get { return $"I_{ID}13_re"; } }
        string I12im { get { return $"I_{ID}12_im"; } }
        string I13im { get { return $"I_{ID}13_im"; } }
        public Switch2Way1P(bool state, Pin1Phase in_pin, Pin1Phase out1_pin, Pin1Phase out2_pin) :base()
        {
            this.state = state;
            this.in_pin = in_pin;
            this.out1_pin = out1_pin;
            this.out2_pin = out2_pin;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string state = $"state_{ID}";
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.Vre} - {out1_pin.Vre}) * {state} = (1.0 - {state}) * {I12re};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.Vim} - {out1_pin.Vim}) * {state} = (1.0 - {state}) * {I12im};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.Vre} - {out2_pin.Vre}) * (1.0 - {state}) = {state} * {I13re};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.Vim} - {out2_pin.Vim}) * (1.0 - {state}) = {state} * {I13im};"
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"{I12re}",
                Node1 = in_pin.Vre,
                Node2 = out1_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"{I12im}",
                Node1 = in_pin.Vim,
                Node2 = out1_pin.Vim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"{I13re}",
                Node1 = in_pin.Vre,
                Node2 = out2_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"{I13im}",
                Node1 = in_pin.Vim,
                Node2 = out2_pin.Vim
            });
            return equations;
        }

        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string state = $"state_{ID}";
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.V} - {out1_pin.V}) * {state} = (1.0 - {state}) * {I12};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.V} - {out2_pin.V}) * (1.0 - {state}) = {state} * {I13};"
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"{I12}",
                Node1 = in_pin.V,
                Node2 = out1_pin.V
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"{I13}",
                Node1 = in_pin.V,
                Node2 = out2_pin.V
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
    }
    public class SteadyStateSwitch2Way1PModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            bool state = (elementObject.GetValue("State") as BoolValue).Value;
            return new Switch2Way1P(state, elementNodes["in"] as Pin1Phase, elementNodes["out1"] as Pin1Phase, elementNodes["out2"] as Pin1Phase);
        }
    }
    public class TransientSwitch2Way1PModel : ITransientElementModel
    {
        public ITransientElement CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            bool state = (elementObject.GetValue("State") as BoolValue).Value;
            return new Switch2Way1P(state, elementNodes["in"] as Pin1Phase, elementNodes["out1"] as Pin1Phase, elementNodes["out2"] as Pin1Phase);
        }
    }
}
