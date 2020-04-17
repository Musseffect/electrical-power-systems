using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ElectricalPowerSystems.PowerModel.NewModel.ModelInterpreter;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    class Inductor:Element,ITransientElement,ISteadyStateElement
    {
        float inductance;
        Pin1Phase in_pin;
        Pin1Phase out_pin;
        public string I { get { return $"I_{ID}"; } }
        public string Ire { get { return $"I_{ID}_re"; } }
        public string Iim { get { return $"I_{ID}_im"; } }
        public Inductor(float inductance, Pin1Phase in_pin, Pin1Phase out_pin) : base()
        {
            this.inductance = inductance;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            string L = $"L_{ID}";
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = I,
                Node1 = in_pin.V,
                Node2 = out_pin.V
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.V} - {out_pin.V} = {L} * der({I});"
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant L_{ID} = {inductance.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            string L = $"L_{ID}";
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = Ire,
                Node1 = in_pin.Vre,
                Node2 = out_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = Iim,
                Node1 = in_pin.Vim,
                Node2 = out_pin.Vim
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.Vre} - {out_pin.Vre} = - frequency * {L} * {Iim};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.Vim} - {out_pin.Vim} = frequency * {L} * {Ire};"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant L_{ID} = {inductance.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    public class SteadyStateInductorModel : ISteadyStateElementModel
    {
        ISteadyStateElement ISteadyStateElementModel.CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double inductance = (elementObject.GetValue("L") as FloatValue).Value;
            return new Inductor((float)inductance, elementNodes["in"] as Pin1Phase, elementNodes["out"] as Pin1Phase);
        }
    }
    public class TransientInductorModel : ITransientElementModel
    {
        ITransientElement ITransientElementModel.CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double inductance = (elementObject.GetValue("L") as FloatValue).Value;
            return new Inductor((float)inductance, elementNodes["in"] as Pin1Phase, elementNodes["out"] as Pin1Phase);
        }
    }
}
