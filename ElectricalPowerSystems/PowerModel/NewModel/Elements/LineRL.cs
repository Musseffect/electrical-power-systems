using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ElectricalPowerSystems.PowerModel.NewModel.ModelInterpreter;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    class LineRL:Element,ISteadyStateElement,ITransientElement
    {
        float r;
        float l;
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
        public LineRL(float r,float l,Pin3Phase in_pin,Pin3Phase out_pin) : base()
        {
            this.r = r;
            this.l = l;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
        }

        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
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
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VA} - {out_pin.VA} = {IA} * R_{ID} + der({IA}) * L_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VB} - {out_pin.VB} = {IB} * R_{ID} + der({IB}) * L_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VC} - {out_pin.VC} = {IC} * R_{ID} + der({IC}) * L_{ID};"
            });
            return equations;
        }

        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
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
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VAre} - {out_pin.VAre} = {IAre} * R_{ID} - frequency * {IAim} * L_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VAim} - {out_pin.VAim} = {IAim} * R_{ID} + frequency * {IAre} * L_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VBre} - {out_pin.VBre} = {IBre} * R_{ID} - frequency * {IBim} * L_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VBim} - {out_pin.VBim} = {IBim} * R_{ID} - frequency * {IBre} * L_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VCre} - {out_pin.VCre} = {ICre} * R_{ID} - frequency * {ICim} * L_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VCim} - {out_pin.VCim} = {ICim} * R_{ID} - frequency * {ICre} * L_{ID};"
            });
            return equations;
        }

        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID} = {r.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant L_{ID} = {l.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }

        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID} = {r.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant L_{ID} = {l.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    public class SteadyStateLineRLModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double r = (elementObject.GetValue("R") as FloatValue).Value;
            double l = (elementObject.GetValue("L") as FloatValue).Value;
            return new LineRL((float)r, (float)l, elementNodes["in"] as Pin3Phase, elementNodes["out"] as Pin3Phase);
        }
    }
}
