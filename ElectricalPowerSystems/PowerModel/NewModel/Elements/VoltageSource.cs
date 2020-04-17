using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using static ElectricalPowerSystems.PowerModel.NewModel.ModelInterpreter;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    class VoltageSource : Element, ITransientElement, ISteadyStateElement, IACSourceElement
    {
        float amp;
        float phase;
        float frequency;
        Pin1Phase in_pin;
        Pin1Phase out_pin;
        public string I { get { return $"I_{ID}"; } }
        public string Ire { get { return $"I_{ID}_re"; } }
        public string Iim { get { return $"I_{ID}_im"; } }
        public VoltageSource(float amp, float phase, float frequency, Pin1Phase in_pin, Pin1Phase out_pin) : base()
        {
            this.amp = amp;
            this.phase = phase;
            this.frequency = frequency;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = I,
                Node1 = out_pin.V,
                Node2 = in_pin.V
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{out_pin.V} - {in_pin.V} = E_{ID} * sin(w_{ID} * t  + ph_{ID});",
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant E_{ID} = {(amp).ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant w_{ID} = {frequency.ToString(new CultureInfo("en-US"))}* 2.0 * pi();"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant ph_{ID} = {phase.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = Ire,
                Node1 = out_pin.Vre,
                Node2 = in_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = Iim,
                Node1 = out_pin.Vim,
                Node2 = in_pin.Vim
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{out_pin.Vre} - {in_pin.Vre} = E_{ID}_re;",
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{out_pin.Vim} - {in_pin.Vim} = E_{ID}_im;",
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            double reValue = frequency == this.frequency ? amp * Math.Cos(phase) : 0.0;
            double imValue = frequency == this.frequency ? amp * Math.Sin(phase) : 0.0;
            equations.Add(new EquationBlock
            {
                Equation = $"constant E_{ID}_re = {reValue.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant E_{ID}_im = {imValue.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        public double[] GetFrequencies()
        {
            return new double[]{ frequency};
        }
    }
    public class SteadyStateVoltageSourceModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double amp = (elementObject.GetValue("Peak") as FloatValue).Value;
            double phase = (elementObject.GetValue("Phase") as FloatValue).Value;
            double frequency = (elementObject.GetValue("Frequency") as FloatValue).Value;
            return new VoltageSource((float)amp, (float)phase, (float)frequency, elementNodes["in"] as Pin1Phase, elementNodes["out"] as Pin1Phase);
        }
    }
    public class TransienteVoltageSourceModel : ITransientElementModel
    {
        ITransientElement ITransientElementModel.CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double amp = (elementObject.GetValue("Peak") as FloatValue).Value;
            double phase = (elementObject.GetValue("Phase") as FloatValue).Value;
            double frequency = (elementObject.GetValue("Frequency") as FloatValue).Value;
            return new VoltageSource((float)amp, (float)phase, (float)frequency, elementNodes["in"] as Pin1Phase, elementNodes["out"] as Pin1Phase);
        }
    }
}