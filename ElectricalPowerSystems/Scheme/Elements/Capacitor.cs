using MathNet.Numerics;
using System;
using System.Collections.Generic;
using ElectricalPowerSystems.Scheme.Transient;
using ElectricalPowerSystems.Scheme.Interpreter;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ElectricalPowerSystems.Scheme.Interpreter.Interpreter;

namespace ElectricalPowerSystems.Scheme.Elements
{
    class Capacitor:Element, ITransientElement, ISteadyStateElement
    {
        float capacitance;
        Pin1Phase in_pin;
        Pin1Phase out_pin;
        public string I { get { return $"I_{ID}"; } }
        public string Ire { get { return $"I_{ID}_re"; } }
        public string Iim { get { return $"I_{ID}_im"; } }
        public Capacitor(float capacitance, Pin1Phase in_pin, Pin1Phase out_pin) : base()
        {
            this.capacitance = capacitance;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            string C = $"C_{ID}";
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = $" {C} * (der({in_pin.V}) - der({out_pin.V}))",
                Node1 = in_pin.V,
                Node2 = out_pin.V
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant C_{ID} = {capacitance.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            string C = $"C_{ID}";
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = $" {C} * frequency * (- {in_pin.Vim} + {out_pin.Vim})",
                Node1 = in_pin.Vre,
                Node2 = out_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $" {C} * frequency * ({in_pin.Vre} - {out_pin.Vre})",
                Node1 = in_pin.Vim,
                Node2 = out_pin.Vim
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant C_{ID} = {capacitance.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    public class SteadyStateCapacitorModel : ISteadyStateElementModel
    {
        ISteadyStateElement ISteadyStateElementModel.CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double capacitance = (elementObject.GetValue("C") as FloatValue).Value;
            return new Capacitor((float)capacitance, elementNodes["in"] as Pin1Phase, elementNodes["out"] as Pin1Phase);
        }
    }
    public class TransientCapacitorModel : ITransientElementModel
    {
        ITransientElement ITransientElementModel.CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double capacitance = (elementObject.GetValue("C") as FloatValue).Value;
            return new Capacitor((float)capacitance, elementNodes["in"] as Pin1Phase, elementNodes["out"] as Pin1Phase);
        }
    }
}
