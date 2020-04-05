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
    class TransformerYy:Element,ISteadyStateElement,ITransientElement
    {
        enum Group
        {
            Yy0 = 0,//y0y0
            Yy2 = 1,
            Yy4 = 2,
            Yy6 = 3,
            Yy8 = 4,
            Yy10 = 5
        };
        int group;
        Complex32 zp;
        Complex32 zs;
        double xm;
        double rc;
        double k;
        Pin3Phase in_pin;
        Pin3Phase out_pin;
        Pin1Phase in_n_pin;
        Pin1Phase out_n_pin;
        public string IAP { get { return $"I_{ID}pa"; } }
        public string IBP { get { return $"I_{ID}pb"; } }
        public string ICP { get { return $"I_{ID}pc"; } }
        public string IAPre { get { return $"I_{ID}pa_re"; } }
        public string IBPre { get { return $"I_{ID}pb_re"; } }
        public string ICPre { get { return $"I_{ID}pc_re"; } }
        public string IAPim { get { return $"I_{ID}pa_im"; } }
        public string IBPim { get { return $"I_{ID}pb_im"; } }
        public string ICPim { get { return $"I_{ID}pc_im"; } }
        public string IAS { get { return $"I_{ID}sa"; } }
        public string IBS { get { return $"I_{ID}sb"; } }
        public string ICS { get { return $"I_{ID}sc"; } }
        public string IASre { get { return $"I_{ID}sa_re"; } }
        public string IBSre { get { return $"I_{ID}sb_re"; } }
        public string ICSre { get { return $"I_{ID}sc_re"; } }
        public string IASim { get { return $"I_{ID}sa_im"; } }
        public string IBSim { get { return $"I_{ID}sb_im"; } }
        public string ICSim { get { return $"I_{ID}sc_im"; } }
        public TransformerYy(Complex32 zp, Complex32 zs, double xm, double rc, double k, int group, Pin3Phase in_pin, Pin3Phase out_pin,Pin1Phase in_n_pin,Pin1Phase out_n_pin) :base()
        {
            if (group < 0 || group > 5)
            {
                throw new Exception("Incorrect value for parameter \"group\"");
            }
            this.group = group;
            this.zp = zp;
            this.zs = zs;
            this.xm = xm;
            this.rc = rc;
            this.k = k;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
            this.out_n_pin = out_n_pin;
            this.in_n_pin = in_n_pin;
        }
        private string[] GetNodes()
        {
            switch (group)
            {
                case (int)Group.Yy0:
                    return new string[] {
                        in_pin.VA, in_n_pin.V,
                        out_pin.VA, out_n_pin.V,
                        in_pin.VB, in_n_pin.V,
                        out_pin.VB, out_n_pin.V,
                        in_pin.VC, in_n_pin.V,
                        out_pin.VC, out_n_pin.V
                    };//
                case (int)Group.Yy2:
                    return new string[] {
                        in_pin.VA, in_n_pin.V,
                        out_n_pin.V, out_pin.VB,
                        in_pin.VB, in_n_pin.V,
                        out_n_pin.V, out_pin.VC,
                        in_pin.VC, in_n_pin.V,
                        out_n_pin.V, out_pin.VA
                    };
                case (int)Group.Yy4:
                    return new string[] {
                        in_pin.VA, in_n_pin.V,
                        out_pin.VC, out_n_pin.V,
                        in_pin.VB, in_n_pin.V,
                        out_pin.VA, out_n_pin.V,
                        in_pin.VC, in_n_pin.V,
                        out_pin.VB, out_n_pin.V
                    };//
                case (int)Group.Yy6:
                    return new string[] {
                        in_pin.VA, in_n_pin.V,
                        out_n_pin.V, out_pin.VA,
                        in_pin.VB, in_n_pin.V,
                        out_n_pin.V, out_pin.VB,
                        in_pin.VC, in_n_pin.V,
                        out_n_pin.V, out_pin.VC
                    };
                case (int)Group.Yy8:
                    return new string[] {
                        in_pin.VC, in_n_pin.V,
                        out_pin.VA, out_n_pin.V,
                        in_pin.VA, in_n_pin.V,
                        out_pin.VB, out_n_pin.V,
                        in_pin.VB, in_n_pin.V,
                        out_pin.VC, out_n_pin.V
                    };//
                case (int)Group.Yy10:
                    return new string[] {
                        in_n_pin.V, in_pin.VB,
                        out_pin.VA, out_n_pin.V,
                        in_n_pin.V, in_pin.VC,
                        out_pin.VB, out_n_pin.V,
                        in_n_pin.V, in_pin.VA,
                        out_pin.VC, out_n_pin.V
                    };//
            }
            throw new NotImplementedException();
        }
    List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string[] nodes = GetNodes(); string[] in_p = new string[] { nodes[0], nodes[4], nodes[8] };
            string[] out_p = new string[] { nodes[1], nodes[5], nodes[9] };
            string[] in_s = new string[] { nodes[2], nodes[6], nodes[10] };
            string[] out_s = new string[] { nodes[3], nodes[7], nodes[11] };

            //current equations for a,as1 ... 

            for (int i = 1; i < 4; i++)
            {
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{ID}p{i}_re",
                    Node1 = $"{in_p[i - 1]}_re",
                    Node2 = $"{out_p[i - 1]}_re"
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{ID}p{i}_im",
                    Node1 = $"{in_p[i - 1]}_im",
                    Node2 = $"{out_p[i - 1]}_im"
                });
            }
            for (int i = 1; i < 4; i++)
            {
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{ID}s{i}_re",
                    Node1 = $"{in_s[i - 1]}_re",
                    Node2 = $"{out_s[i - 1]}_re"
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{ID}s{i}_im",
                    Node1 = $"{in_s[i - 1]}_im",
                    Node2 = $"{out_s[i - 1]}_im"
                });
            }
            for (int i = 1; i < 4; i++)
            {
                equations.Add(new EquationBlock
                {
                    Equation = $"{in_s[i - 1]}_re - {out_s[i - 1]}_re - k_{ID} * (I_{ID}m{i}_re * Rm_{ID} - I_{ID}m{i}_im * Zm_{ID}_im * frequency) = I_{ID}s{i}_re * Rs_{ID} - I_{ID}s{i}_im * Ls_{ID} * frequency;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{in_s[i - 1]}_im - {out_s[i - 1]}_im - k_{ID} * (I_{ID}m{i}_re * Zm_{ID}_im + I_{ID}m{i}_im * Rm_{ID}) = I_{ID}s{i}_im * Rp_{ID} + I_{ID}s{i}_re * Lp_{ID} * frequency;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{in_p[i - 1]}_re - {out_p[i - 1]}_re - (I_{ID}m{i}_re * Rm_{ID} - I_{ID}m{i}_im * Zm_{ID}_im) = I_{ID}p{i}_re * Rp_{ID} - I_{ID}p{i}_im * Lp_{ID} * frequency;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{in_p[i - 1]}_im - {out_p[i - 1]}_im - (I_{ID}m{i}_re * Zm_{ID}_im - I_{ID}m{i}_im * Rm_{ID}) = I_{ID}p{i}_im * Rp_{ID} + I_{ID}p{i}_re * Lp_{ID} * frequency;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"I_{ID}p{i}_re = I_{ID}s{i}_re * k_{ID} + I_{ID}m{i}_re;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"I_{ID}p{i}_im = I_{ID}s{i}_im * k_{ID} + I_{ID}m{i}_im;"
                });
            }
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
                Equation = $"constant Lp_{ID} = {zp.Imaginary.ToString(new CultureInfo("en-US"))} / baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Rs_{ID} = {zs.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Ls_{ID} = {zs.Imaginary.ToString(new CultureInfo("en-US"))} / baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Rc_{ID} = {rc.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Xmf_{ID} = {(xm * frequency).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Rm_{ID} = Rc_{ID} * Xmf_{ID} * Xmf_{ID} / (Rc_{ID} * Rc_{ID} + Xmf_{ID} * Xmf_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Xm_{ID} = Xmf_{ID} * Rc_{ID} * Rc_{ID} * {frequency.ToString(new CultureInfo("en-US"))} / (Rc_{ID} * Rc_{ID} + Xmf_{ID} * Xmf_{ID}) / baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant k_{ID} = {k.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
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
                Equation = $"constant Rp_{ID} = {zp.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Lp_{ID} = {zp.Imaginary.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Rs_{ID} = {zs.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Ls_{ID} = {zs.Imaginary.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Rc_{ID} = {rc.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant Lm_{ID} = {xm.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant k_{ID} = {k.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    public class SteadyStateTransformerYyModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            Complex32 zp = (elementObject.GetValue("Zp") as ComplexValue).Value;
            Complex32 zs = (elementObject.GetValue("Zs") as ComplexValue).Value;
            double xm = (elementObject.GetValue("Xm") as FloatValue).Value;
            double rc = (elementObject.GetValue("Rc") as FloatValue).Value;
            double k = (elementObject.GetValue("K") as FloatValue).Value;
            int group = (elementObject.GetValue("Group") as IntValue).Value;
            return new TransformerYy(zp, zs, (float)xm, (float)rc, (float)k, group, elementNodes["in"] as Pin3Phase, elementNodes["out"] as Pin3Phase, elementNodes["in_n"] as Pin1Phase, elementNodes["out_n"] as Pin1Phase);
        }
    }
}
