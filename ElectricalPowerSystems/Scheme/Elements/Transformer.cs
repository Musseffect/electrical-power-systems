using MathNet.Numerics;
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
    class Transformer : Element, ITransientElement, ISteadyStateElement
    {
        float k;
        Pin1Phase inp_pin;
        Pin1Phase ins_pin;
        Pin1Phase outp_pin;
        Pin1Phase outs_pin;
        public string Ip { get { return $"I_{ID}p"; } }
        public string Ipre { get { return $"I_{ID}p_re"; } }
        public string Ipim { get { return $"I_{ID}p_im"; } }
        public string Is { get { return $"I_{ID}s"; } }
        public string Isre { get { return $"I_{ID}s_re"; } }
        public string Isim { get { return $"I_{ID}s_im"; } }
        public Transformer(float k,Pin1Phase inp_pin, Pin1Phase ins_pin, Pin1Phase outp_pin, Pin1Phase outs_pin):base()
        {
            this.k = k;
            this.inp_pin = inp_pin;
            this.ins_pin = ins_pin;
            this.outp_pin = outp_pin;
            this.outs_pin = outs_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = Ip,
                Node1 = inp_pin.V,
                Node2 = outp_pin.V
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = Is,
                Node1 = ins_pin.V,
                Node2 = outs_pin.V
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{Ip} = k_{ID} * {Is};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"k_{ID} * ({inp_pin.V} - {outp_pin.V}) = {ins_pin.V} - {outp_pin.V};"
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant k_{ID} = {k.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = Ipre,
                Node1 = inp_pin.Vre,
                Node2 = outp_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = Ipim,
                Node1 = inp_pin.Vim,
                Node2 = outp_pin.Vim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = Isre,
                Node1 = ins_pin.Vre,
                Node2 = outs_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = Isim,
                Node1 = inp_pin.Vim,
                Node2 = outp_pin.Vim
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{Ipre} = k_{ID} * {Isre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{Ipim} = k_{ID} * {Isim};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"k_{ID} * ({inp_pin.Vre} - {outp_pin.Vre}) = {ins_pin.Vre} - {outp_pin.Vre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"k_{ID} * ({inp_pin.Vim} - {outp_pin.Vim}) = {ins_pin.Vim} - {outp_pin.Vim};"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant k_{ID} = {k.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    public class SteadyStateTransformerModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double k = (elementObject.GetValue("K") as FloatValue).Value;
            return new Transformer((float) k, elementNodes["in_p"] as Pin1Phase, elementNodes["in_s"] as Pin1Phase, elementNodes["out_p"] as Pin1Phase, elementNodes["out_s"] as Pin1Phase);
        }
    }
    public class TransientTransformerModel : ITransientElementModel
    {
        ITransientElement ITransientElementModel.CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double k = (elementObject.GetValue("K") as FloatValue).Value;
            return new Transformer((float)k, elementNodes["in_p"] as Pin1Phase, elementNodes["in_s"] as Pin1Phase, elementNodes["out_p"] as Pin1Phase, elementNodes["out_s"] as Pin1Phase);
        }
    }
    class RealTransformer : Element, ITransientElement, ISteadyStateElement
    {
        Complex32 zp;
        Complex32 zs;
        double rc;
        double xm;
        double k;
        Pin1Phase in_p_pin;
        Pin1Phase out_p_pin;
        Pin1Phase in_s_pin;
        Pin1Phase out_s_pin;
        public RealTransformer(Complex32 zp, Complex32 zs, double xm, double rc, double k, Pin1Phase in_p_pin, Pin1Phase out_p_pin, Pin1Phase in_s_pin, Pin1Phase out_s_pin) : base()
        {
            this.zp = zp;
            this.zs = zs;
            this.xm = xm;
            this.rc = rc;
            this.k = k;
            this.in_p_pin = in_p_pin;
            this.out_p_pin = out_p_pin;
            this.in_s_pin = in_s_pin;
            this.out_s_pin = out_s_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"I_{ID}p",
                Node1 = in_p_pin.V,
                Node2 = out_p_pin.V
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"I_{ID}s",
                Node1 = in_s_pin.V,
                Node2 = out_s_pin.V
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_s_pin.V} - {out_s_pin.V} = k_{ID} * I_{ID}c * Rc_{ID} + I_{ID}s * Rs_{ID} + Ls_{ID} * der(I_{ID}s);"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_p_pin.V} - {out_p_pin.V} = I_{ID}p * Rp_{ID} + Lp_{ID} * der(I_{ID}p) + I_{ID}c * Rc_{ID};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"I_{ID}c * Rc_{ID} = Lm_{ID} * der(I_{ID}m);"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"I_{ID}p = I_{ID}s * k_{ID} + I_{ID}m + I_{ID}c;"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"I_{ID}p_re",
                Node1 = in_p_pin.Vre,
                Node2 = out_p_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"I_{ID}p_im",
                Node1 = in_p_pin.Vim,
                Node2 = out_p_pin.Vim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"I_{ID}s_re",
                Node1 = in_s_pin.Vre,
                Node2 = out_s_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"I_{ID}s_im",
                Node1 = in_s_pin.Vim,
                Node2 = out_s_pin.Vim
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_s_pin.Vre} - {out_s_pin.Vre} = k_{ID} * I_{ID}c_re * Rc_{ID} + I_{ID}s_re * Rs_{ID} - I_{ID}s_im * Ls_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_s_pin.Vim} - {out_s_pin.Vim} = k_{ID} * I_{ID}c_im * Rc_{ID} + I_{ID}s_im * Rs_{ID} + I_{ID}s_re * Ls_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_p_pin.Vre} - {out_p_pin.Vre} = I_{ID}c_re * Rc_{ID} + I_{ID}p_re * Rp_{ID} - I_{ID}p_im * Lp_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_p_pin.Vim} - {out_p_pin.Vim} = I_{ID}c_im * Rc_{ID} + I_{ID}p_im * Rp_{ID} + I_{ID}p_re * Lp_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"I_{ID}p_re = I_{ID}s_re * k_{ID} + I_{ID}m_re + I_{ID}c_re;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"I_{ID}p_im = I_{ID}s_im * k_{ID} + I_{ID}m_im + I_{ID}c_im;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"I_{ID}c_re * Rc_{ID} = - I_{ID}m_im * Lm_{ID} * frequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"I_{ID}c_im * Rc_{ID} = I_{ID}m_re * Lm_{ID} * frequency;"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant Rp_{ID} = {zp.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Lp_{ID} = {zp.Imaginary.ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Rs_{ID} = {zs.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Ls_{ID} = {zs.Imaginary.ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Rc_{ID} = {rc.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Lm_{ID} = {xm .ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant k_{ID} = {k.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant Rp_{ID} = {zp.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Lp_{ID} = {zp.Imaginary.ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Rs_{ID} = {zs.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Ls_{ID} = {zs.Imaginary.ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Rc_{ID} = {rc.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Lm_{ID} = {xm.ToString(new CultureInfo("en-US"))}/ baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant k_{ID} = {k.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    public class SteadyStateRealTransformerModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            Complex32 zp = (elementObject.GetValue("Zp") as ComplexValue).Value;
            Complex32 zs = (elementObject.GetValue("Zs") as ComplexValue).Value;
            double xm = (elementObject.GetValue("Xm") as FloatValue).Value;
            double rc = (elementObject.GetValue("Rc") as FloatValue).Value;
            double k = (elementObject.GetValue("K") as FloatValue).Value;
            return new RealTransformer(zp, zs, xm, rc, (float)k, elementNodes["in_p"] as Pin1Phase, elementNodes["out_p"] as Pin1Phase, elementNodes["in_s"] as Pin1Phase, elementNodes["out_s"] as Pin1Phase);
        }
    }
    public class TransientRealTransformerModel : ITransientElementModel
    {
        ITransientElement ITransientElementModel.CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            Complex32 zp = (elementObject.GetValue("Zp") as ComplexValue).Value;
            Complex32 zs = (elementObject.GetValue("Zs") as ComplexValue).Value;
            double xm = (elementObject.GetValue("Xm") as FloatValue).Value;
            double rc = (elementObject.GetValue("Rc") as FloatValue).Value;
            double k = (elementObject.GetValue("K") as FloatValue).Value;
            return new RealTransformer(zp,zs,xm,rc,(float)k, elementNodes["in_p"] as Pin1Phase,  elementNodes["out_p"] as Pin1Phase, elementNodes["in_s"] as Pin1Phase, elementNodes["out_s"] as Pin1Phase);
        }
    }
}
