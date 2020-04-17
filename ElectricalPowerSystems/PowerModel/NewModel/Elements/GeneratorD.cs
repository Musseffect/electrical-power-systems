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
    class GeneratorD : Element, ITransientElement, ISteadyStateElement, IACSourceElement
    {
        Pin3Phase out_pin;
        float peak;
        float phase;
        float frequency;
        Complex32 z;
        public string IAB { get { return $"I_{ID}a"; } }
        public string IBC { get { return $"I_{ID}b"; } }
        public string ICA { get { return $"I_{ID}c"; } }
        public string IABre { get { return $"I_{ID}a_re"; } }
        public string IBCre { get { return $"I_{ID}b_re"; } }
        public string ICAre { get { return $"I_{ID}c_re"; } }
        public string IABim { get { return $"I_{ID}a_im"; } }
        public string IBCim { get { return $"I_{ID}b_im"; } }
        public string ICAim { get { return $"I_{ID}c_im"; } }
        public GeneratorD(float peak, float phase, float frequency, Complex32 z, Pin3Phase out_pin) : base()
        {
            this.peak = peak;
            this.phase = phase;
            this.z = z;
            this.out_pin = out_pin;
            this.frequency = frequency;
        }

        double[] IACSourceElement.GetFrequencies()
        {
            return new double[] { frequency};
        }

        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = IABre,
                Node1 = out_pin.VAre,
                Node2 = out_pin.VBre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IABim,
                Node1 = out_pin.VAim,
                Node2 = out_pin.VBim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBCre,
                Node1 = out_pin.VBre,
                Node2 = out_pin.VCre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBCim,
                Node1 = out_pin.VBim,
                Node2 = out_pin.VCim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICAre,
                Node1 = out_pin.VCre,
                Node2 = out_pin.VAre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICAim,
                Node1 = out_pin.VCim,
                Node2 = out_pin.VAim
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{out_pin.VBre} - {out_pin.VAre} = E_{ID} * cos(ph_{ID}) - ({IABre} * R_{ID} - frequency * {IABim} * L_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{out_pin.VBim} - {out_pin.VAim} = E_{ID} * sin(ph_{ID}) - ({IABim} * R_{ID} + frequency * {IABre} * L_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{out_pin.VCre} - {out_pin.VBre} = E_{ID} * cos(ph_{ID} + pi()*2.0/3.0) - ({IBCre} * R_{ID} - frequency * {IBCim} * L_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{out_pin.VCim} - {out_pin.VBim} = E_{ID} * sin(ph_{ID} + pi()*2.0/3.0) - ({IBCim} * R_{ID} + frequency * {IBCre} * L_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{out_pin.VAre} - {out_pin.VCre} = E_{ID} * cos(ph_{ID} + pi()*4.0/3.0) - ({ICAre} * R_{ID} - frequency * {ICAim} * L_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{out_pin.VAim} - {out_pin.VCim} = E_{ID} * sin(ph_{ID} + pi()*4.0/3.0) - ({ICAim} * R_{ID} + frequency * {ICAre} * L_{ID});"
            });
            return equations;
        }

        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            double E_peak = this.frequency == frequency ? peak : 0.0;
            equations.Add(new EquationBlock
            {
                Equation = $"constant E_{ID} = {E_peak.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant ph_{ID} = {phase.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID} = {z.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant L_{ID} = {z.Imaginary.ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            return equations;
        }

        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = IAB,
                Node1 = out_pin.VA,
                Node2 = out_pin.VB
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBC,
                Node1 = out_pin.VB,
                Node2 = out_pin.VC
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICA,
                Node1 = out_pin.VC,
                Node2 = out_pin.VA
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{out_pin.VB} - {out_pin.VA} = E_{ID} * sin(ph_{ID}) - ({IAB} * R_{ID} + der({IAB}) * L_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{out_pin.VC} - {out_pin.VB} = E_{ID} * sin(ph_{ID} + pi()*2.0/3.0) - ({IBC} * R_{ID} + der({IBC}) * L_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{out_pin.VA} - {out_pin.VC} = E_{ID} * sin(ph_{ID} + pi()*4.0/3.0) - ({ICA} * R_{ID} + der({ICA}) * L_{ID});"
            });
            return equations;
        }

        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant E_{ID} = {peak.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant w_{ID} = {frequency.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant ph_{ID} = {phase.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    public class SteadyStateGeneratorDModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double vPeak = (elementObject.GetValue("Peak") as FloatValue).Value;
            double vPhase = (elementObject.GetValue("Phase") as FloatValue).Value;
            double frequency = (elementObject.GetValue("Frequency") as FloatValue).Value;
            Complex32 z = (elementObject.GetValue("Z") as ComplexValue).Value;
            return new GeneratorD((float)vPeak, (float)vPhase, (float)frequency, z, elementNodes["out"] as Pin3Phase);
        }
    }
}
