using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ElectricalPowerSystems.PowerModel.NewModel.ModelInterpreter;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    class LinePi:Element,ITransientElement,ISteadyStateElement
    {
        float r;
        float l;
        float b;
        float g;
        float bp;
        Pin3Phase in_pin;
        Pin3Phase out_pin;
        public string IA1 { get { return $"I_{ID}a1"; } }
        public string IB1 { get { return $"I_{ID}b1"; } }
        public string IC1 { get { return $"I_{ID}c1"; } }
        public string IA12re { get { return $"I_{ID}a12_re"; } }
        public string IB12re { get { return $"I_{ID}b12_re"; } }
        public string IC12re { get { return $"I_{ID}c12_re"; } }
        public string IA12im { get { return $"I_{ID}a12_im"; } }
        public string IB12im { get { return $"I_{ID}b12_im"; } }
        public string IC12im { get { return $"I_{ID}c12_im"; } }
        public string IA2 { get { return $"I_{ID}a2"; } }
        public string IB2 { get { return $"I_{ID}b2"; } }
        public string IC2 { get { return $"I_{ID}c2"; } }
        public string IAG1re { get { return $"I_{ID}ag1_re"; } }
        public string IBG1re { get { return $"I_{ID}bg1_re"; } }
        public string ICG1re { get { return $"I_{ID}cg1_re"; } }
        public string IAG1im { get { return $"I_{ID}ag1_im"; } }
        public string IBG1im { get { return $"I_{ID}bg1_im"; } }
        public string ICG1im { get { return $"I_{ID}cg1_im"; } }
        public string IAG2re { get { return $"I_{ID}ag2_re"; } }
        public string IBG2re { get { return $"I_{ID}bg2_re"; } }
        public string ICG2re { get { return $"I_{ID}cg2_re"; } }
        public string IAG2im { get { return $"I_{ID}ag2_im"; } }
        public string IBG2im { get { return $"I_{ID}bg2_im"; } }
        public string ICG2im { get { return $"I_{ID}cg2_im"; } }
        public LinePi(float r, float l, float b, float g, float bp, Pin3Phase in_pin, Pin3Phase out_pin) : base()
        {
            this.r = r;
            this.l = l;
            this.b = b;
            this.g = g;
            this.bp = bp;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            throw new NotImplementedException();
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
            equations.Add(new EquationBlock
            {
                Equation = $"constant B_{ID} = {b.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant G_{ID} = {g.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Bp_{ID} = {bp.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }

        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock//IA1re
            {
                Equation = $"{IA12re} + {IAG1re} + Bp_{ID} * frequency * ({in_pin.VBim} - {in_pin.VAim}*2 + {in_pin.VCim})",
                Node1 = in_pin.VAre,
                Node2 = null
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"{IA12im} + {IAG1im} + Bp_{ID} * frequency * ({in_pin.VAre}*2 - {in_pin.VBre} -  {in_pin.VCim})",
                Node1 = in_pin.VAim,
                Node2 = null
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"{IB12re} + {IBG1re} + Bp_{ID} * frequency * ({in_pin.VAim} - {in_pin.VBim}*2 + {in_pin.VCim})",
                Node1 = in_pin.VBre,
                Node2 = null
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"{IB12im} + {IBG1im} + Bp_{ID} * frequency * ({in_pin.VBre}*2 - {in_pin.VAre} - {in_pin.VCre})",
                Node1 = in_pin.VBim,
                Node2 = null
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"({IC12re} + {ICG1re} + Bp_{ID} * frequency * ({in_pin.VAim} - {in_pin.VCim}*2 + {in_pin.VBim}))",
                Node1 = in_pin.VCre,
                Node2 = null
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"({IC12im} + {ICG1im} + Bp_{ID} * frequency * ({in_pin.VCre}*2 - {in_pin.VAre} - {in_pin.VBre}))",
                Node1 = in_pin.VCim,
                Node2 = null
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"({IAG2re} + {IA12re} + Bp_{ID} * frequency * ({out_pin.VBim} - {out_pin.VAim}*2 + {out_pin.VCim}))",
                Node1 = null,
                Node2 = out_pin.VAre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"{IAG2im} + {IA12im} + Bp_{ID} * frequency * ({out_pin.VAre}*2 - {out_pin.VBre} -  {out_pin.VCim})",
                Node1 = null,
                Node2 = out_pin.VAim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"{IBG2re} + {IB12re} + Bp_{ID} * frequency * ({out_pin.VAim} - {out_pin.VBim}*2 + {out_pin.VCim})",
                Node1 = null,
                Node2 = out_pin.VBre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"{IBG2im} + {IB12im} + Bp_{ID} * frequency * ({out_pin.VBre}*2 - {out_pin.VAre} - {out_pin.VCre})",
                Node1 = null,
                Node2 = out_pin.VBim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"({ICG1re} + {IC12re} + Bp_{ID} * frequency * ({out_pin.VAim} - {out_pin.VCim}*2 + {out_pin.VBim}))",
                Node1 = null,
                Node2 = out_pin.VCre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"({ICG2im} + {IC12im}  + Bp_{ID} * frequency * ({in_pin.VCre}*2 - {in_pin.VAre} - {in_pin.VBre}))",
                Node1 = null,
                Node2 = out_pin.VCim
            });


            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VAre} - {out_pin.VAre} = {IA12re} * R_{ID} - {IA12im} * frequency * L_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VAim} - {out_pin.VAim} = {IA12im} * R_{ID} + {IA12re} * frequency * L_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VBre} - {out_pin.VBre} = {IB12re} * R_{ID} - {IB12im} * frequency * L_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VBim} - {out_pin.VBim} = {IB12im} * R_{ID} + {IB12re} * frequency * L_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VCre} - {out_pin.VCre} = {IC12re} * R_{ID} - {IC12im} * frequency * L_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VCim} - {out_pin.VCim} = {IC12im} * R_{ID} + {IC12re} * frequency * L_{ID};"
            });


            equations.Add(new EquationBlock
            {
                Equation = $"{IAG1re} * G_{ID} = {in_pin.VAre} - {in_pin.VAim} * G_{ID} * B_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{IAG1im} * G_{ID} = {in_pin.VAim} + {in_pin.VAre} * G_{ID} * B_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{IAG2re} * G_{ID} = {out_pin.VAre} - {out_pin.VAim} * G_{ID} * B_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{IAG2im} * G_{ID} = {out_pin.VAim} + {out_pin.VAre} * G_{ID} * B_{ID} * frequency;"
            });

            equations.Add(new EquationBlock
            {
                Equation = $"{IBG1re} * G_{ID} = {in_pin.VBre} - {in_pin.VBim} * G_{ID} * B_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{IBG1im} * G_{ID} = {in_pin.VBim} + {in_pin.VBre} * G_{ID} * B_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{IBG2re} * G_{ID} = {out_pin.VBre} - {out_pin.VBim} * G_{ID} * B_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{IBG2im} * G_{ID} = {out_pin.VBim} + {out_pin.VBre} * G_{ID} * B_{ID} * frequency;"
            });

            equations.Add(new EquationBlock
            {
                Equation = $"{ICG1re} * G_{ID} = {in_pin.VCre} - {in_pin.VCim} * G_{ID} * B_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{ICG1im} * G_{ID} = {in_pin.VCim} + {in_pin.VCre} * G_{ID} * B_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{ICG2re} * G_{ID} = {out_pin.VCre} - {out_pin.VCim} * G_{ID} * B_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{ICG2im} * G_{ID} = {out_pin.VCim} + {out_pin.VCre} * G_{ID} * B_{ID} * frequency;"
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
            equations.Add(new EquationBlock
            {
                Equation = $"constant B_{ID} = {b.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant G_{ID} = {g.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Bp_{ID} = {bp.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    public class SteadyStateLinePiModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double r = (elementObject.GetValue("R") as FloatValue).Value;
            double l = (elementObject.GetValue("L") as FloatValue).Value;
            double b = (elementObject.GetValue("B") as FloatValue).Value;
            double g = (elementObject.GetValue("G") as FloatValue).Value;
            double bp = (elementObject.GetValue("Bp") as FloatValue).Value;
            return new LinePi((float)r, (float)l, (float)b, (float)g, (float)bp, elementNodes["in"] as Pin3Phase, elementNodes["out"] as Pin3Phase);
        }
    }
}
