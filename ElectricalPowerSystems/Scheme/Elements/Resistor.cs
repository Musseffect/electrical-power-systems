using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using ElectricalPowerSystems.Scheme.Interpreter;
using static ElectricalPowerSystems.Scheme.Interpreter.Interpreter;

namespace ElectricalPowerSystems.Scheme.Elements
{
    class Resistor : Element, ITransientElement, ISteadyStateElement
    {
        float resistance;
        Pin1Phase in_pin;
        Pin1Phase out_pin;
        public Resistor(float resistance, Pin1Phase in_pin, Pin1Phase out_pin) : base()
        {
            this.resistance = resistance;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string G = $"G_{ID}";
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"({in_pin.V} - {out_pin.V}) * {G}",
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
                Equation = $"constant G_{ID} = {(1.0 / resistance).ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string G = $"G_{ID}";
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"({in_pin.Vre} - {out_pin.Vre}) * {G}",
                Node1 = in_pin.Vre,
                Node2 = out_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"({in_pin.Vim} - {out_pin.Vim}) * {G}",
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
                Equation = $"constant G_{ID} = {(1.0 / resistance).ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    public class SteadyStateResistorModel : ISteadyStateElementModel
    {
        ISteadyStateElement ISteadyStateElementModel.CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double resistance = (elementObject.GetValue("R") as FloatValue).Value;
            return new Resistor((float)resistance, elementNodes["in"] as Pin1Phase, elementNodes["out"] as Pin1Phase);
        }
    }
    public class TransientResistorModel : ITransientElementModel
    {
        ITransientElement ITransientElementModel.CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double resistance = (elementObject.GetValue("R") as FloatValue).Value;
            return new Resistor((float)resistance, elementNodes["in"] as Pin1Phase, elementNodes["out"] as Pin1Phase);
        }
    }
}