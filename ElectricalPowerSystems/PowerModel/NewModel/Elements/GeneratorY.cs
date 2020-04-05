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
    class GeneratorY : Element, ITransientElement, ISteadyStateElement,IACSourceElement
    {
        Pin1Phase n_pin;
        Pin3Phase abc_pin;
        float peak;
        float phase;
        float frequency;
        Complex32 z;
        public string IA { get { return $"I_{ID}a"; } }
        public string IB { get { return $"I_{ID}b"; } }
        public string IC { get { return $"I_{ID}c"; } }
        public string IAre { get { return $"I_{ID}a_re"; } }
        public string IBre { get { return $"I_{ID}b_re"; } }
        public string ICre { get { return $"I_{ID}c_re"; } }
        public string IAim { get { return $"I_{ID}a_im"; } }
        public string IBim { get { return $"I_{ID}b_im"; } }
        public string ICim { get { return $"I_{ID}c_im"; } }
        public GeneratorY(float peak, float phase, float frequency, Complex32 z, Pin3Phase abc_pin, Pin1Phase n_pin) : base()
        {
            this.peak = peak;
            this.phase = phase;
            this.z = z;
            this.abc_pin = abc_pin;
            this.n_pin = n_pin;
            this.frequency = frequency;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            throw new NotImplementedException();
            /*List<EquationBlock> equations = new List<EquationBlock>();
            string E = $"E_{ID}";
            string w = $"w_{ID}";
            string ph = $"ph_{ID}";
            equations.Add(new CurrentFlowBlock
            {
                Equation = IA,
                Node1 = n_pin.V,
                Node2 = abc_pin.VA
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IB,
                Node1 = n_pin.V,
                Node2 = abc_pin.VB
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IC,
                Node1 = n_pin.V,
                Node2 = abc_pin.VC
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{n_pin.V} - {abc_pin.VA} = {E} * sin({w} * time + {ph});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{n_pin.V} - {abc_pin.VB} = {E} * sin({w} * time + {ph} + pi()*2.0/3.0);"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{n_pin.V} - {abc_pin.VC} = {E} * sin({w} * time + {ph} + pi()*4.0/3.0);"
            });
            return equations;*/
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
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string E = $"E_{ID}";
            string ph = $"ph_{ID}";
            equations.Add(new CurrentFlowBlock
            {
                Equation = IAre,
                Node1 = n_pin.Vre,
                Node2 = abc_pin.VAre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IAim,
                Node1 = n_pin.Vim,
                Node2 = abc_pin.VAim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBre,
                Node1 = n_pin.Vre,
                Node2 = abc_pin.VBre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBim,
                Node1 = n_pin.Vim,
                Node2 = abc_pin.VBim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICre,
                Node1 = n_pin.Vre,
                Node2 = abc_pin.VCre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICim,
                Node1 = n_pin.Vim,
                Node2 = abc_pin.VCim
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VAre} - {n_pin.Vre} = {E} * cos({ph}) - ({IAre} * R_{ID} - frequency * {IAim} * L_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VAim} - {n_pin.Vim} = {E} * sin({ph}) - ({IAim} * R_{ID} + frequency * {IAre} * L_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VBre} - {n_pin.Vre} = {E} * cos({ph} + pi()*2.0/3.0) - ({IBre} * R_{ID} - frequency * {IBim} * L_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VBim} - {n_pin.Vim}= {E} * sin({ph} + pi()*2.0/3.0) - ({IBim} * R_{ID} + frequency * {IBre} * L_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VCre} - {n_pin.Vre} = {E} * cos({ph} + pi()*4.0/3.0) - ({ICre} * R_{ID} - frequency * {ICim} * L_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VCim} - {n_pin.Vim} = {E} * sin({ph} + pi()*4.0/3.0) - ({ICim} * R_{ID} + frequency * {ICre} * L_{ID});"
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
        public double[] GetFrequencies()
        {
            return new double[] { frequency };
        }
    }
    public class SteadyStateGeneratorYModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double vPeak = (elementObject.GetValue("Peak") as FloatValue).Value;
            double vPhase = (elementObject.GetValue("Phase") as FloatValue).Value;
            double frequency = (elementObject.GetValue("Frequency") as FloatValue).Value;
            Complex32 z = (elementObject.GetValue("Z") as ComplexValue).Value;
            return new GeneratorY((float)vPeak, (float)vPhase, (float)frequency, z, elementNodes["out"] as Pin3Phase, elementNodes["n"] as Pin1Phase);
        }
    }
}
