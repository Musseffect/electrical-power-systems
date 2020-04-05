using ElectricalPowerSystems.Equations.Nonlinear;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ElectricalPowerSystems.PowerModel.NewModel.ModelInterpreter;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    class Scope1P : Element, ITransientElement, ISteadyStateElement, IScopeElement
    {
        Pin1Phase in_pin;
        Pin1Phase out_pin;
        string label;
        public string I { get { return $"I_{ID}"; } }
        public string Ire { get { return $"I_{ID}_re"; } }
        public string Iim { get { return $"I_{ID}_im"; } }
        public Scope1P(Pin1Phase in_pin, Pin1Phase out_pin,string label) : base()
        {
            this.in_pin = in_pin;
            this.out_pin = out_pin;
            this.label = label;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = I,
                Node1 = in_pin.V,
                Node2 = out_pin.V
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.V} - {out_pin.V} = 0;",
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            return new List<EquationBlock>();
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
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
                Equation = $"{in_pin.Vre} - {out_pin.Vre} = 0;",
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.Vim} - {out_pin.Vim} = 0;",
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            return new List<EquationBlock>();
        }
        string IScopeElement.FormatReadings(IScopeReading[] readings,double[]frequencies)
        {
            string voltage = "voltage: ";
            string current = "current: ";
            string power = "power: ";
            for (int i = 0; i < readings.Length; i++)
            {
                double frequency = frequencies[i];
                ScopeReading1P reading = readings[i] as ScopeReading1P;
                if (i != 0)
                {
                    voltage += " + ";
                    current += " + ";
                    power += " + ";
                }
                voltage += $"{reading.Voltage}={reading.Voltage.Magnitude}@{MathUtils.MathUtils.Degrees(reading.Voltage.Phase)} [{frequency} Hz]";
                current += $"{reading.Current}={reading.Current.Magnitude}@{MathUtils.MathUtils.Degrees(reading.Current.Phase)} [{frequency} Hz]";
                power += $"{reading.Power}={reading.Power.Magnitude}@{MathUtils.MathUtils.Degrees(reading.Power.Phase)} [{frequency} Hz]";
            }
            string result = $"scope {label}: {Environment.NewLine}";
            result += $"{{{Environment.NewLine}";
            result += $"\t{voltage},{Environment.NewLine}";
            result += $"\t{current},{Environment.NewLine}";
            result += $"\t{power}{Environment.NewLine}";
            result += $"}}";
            return result;

            //return $"scope {label}:{Environment.NewLine}{{ {Environment.NewLine}{voltage}, {Environment.NewLine}{current}, {Environment.NewLine}{power} {Environment.NewLine}}}";
        }
        IScopeReading IScopeElement.GetReading(NonlinearSystemSolution solution)
        {
            return new ScopeReading1P(
                new Complex32((float)solution.GetValue(in_pin.Vre), (float)solution.GetValue(in_pin.Vim)),
                new Complex32((float)solution.GetValue(Ire), (float)solution.GetValue(Iim))
                );
        }
    }
    public class SteadyStateScope1PModel : ISteadyStateElementModel
    {
        ISteadyStateElement ISteadyStateElementModel.CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            string label = (elementObject.GetValue("Label") as StringValue).Value;
            return new Scope1P(elementNodes["in"] as Pin1Phase, elementNodes["out"] as Pin1Phase,label);
        }
    }
    public class ScopeReading1P: IScopeReading
    {
        Complex32 voltage;
        Complex32 current;
        public Complex32 Voltage { get { return voltage; } }
        public Complex32 Current { get { return current; } }
        public Complex32 Power { get { return voltage * current.Conjugate(); } }
        public ScopeReading1P(Complex32 voltage,Complex32 current)
        {
            this.voltage = voltage;
            this.current = current;
        }
    }
}
