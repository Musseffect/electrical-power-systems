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
    class TransformerYd:Element,ISteadyStateElement,ITransientElement
    {
        enum Group
        {
            Yd1 = 0,
            Yd3 = 1,
            Yd5 = 2,
            Yd7 = 3,
            Yd9 = 4,
            Yd11 = 5
        }
        int group;
        Complex32 zp;
        Complex32 zs;
        double xm;
        double rc;
        double k;
        Pin3Phase in_pin;
        Pin3Phase out_pin;
        Pin1Phase in_n_pin;
        public TransformerYd(Complex32 zp, Complex32 zs, double xm, double rc, double k, int group, Pin3Phase in_pin, Pin3Phase out_pin, Pin1Phase in_n_pin) : base()
        {
            if (group < 0 || group > 5)
            {
                throw new Exception($"Некорректное значение для параметра \"Group\": {group}");
            }
            this.group = group;
            this.zp = zp;
            this.zs = zs;
            this.xm = xm;
            this.rc = rc;
            this.k = k;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
            this.in_n_pin = in_n_pin;
        }
        private string[] GetNodes()
        {
            switch (group)
            {
                case (int)Group.Yd1:
                    return new string[] {
                        in_n_pin.V ,in_pin.VC,
                        out_pin.VA, out_pin.VC,
                        in_n_pin.V, in_pin.VA,
                        out_pin.VB, out_pin.VA,
                        in_n_pin.V, in_pin.VB,
                        out_pin.VC, out_pin.VB
                    };
                case (int)Group.Yd3:
                    return new string[] {
                        in_pin.VB, in_n_pin.V,
                        out_pin.VA, out_pin.VC,
                        in_pin.VC, in_n_pin.V,
                        out_pin.VB, out_pin.VA,
                        in_pin.VA, in_n_pin.V,
                        out_pin.VC, out_pin.VB
                    };
                case (int)Group.Yd5:
                    return new string[] {
                        in_n_pin.V ,in_pin.VA,
                        out_pin.VA, out_pin.VC,
                        in_n_pin.V, in_pin.VB,
                        out_pin.VB, out_pin.VA,
                        in_n_pin.V, in_pin.VC,
                        out_pin.VC, out_pin.VB
                    };
                case (int)Group.Yd7:
                    return new string[] {
                        in_pin.VC, in_n_pin.V,
                        out_pin.VA, out_pin.VC,
                        in_pin.VA, in_n_pin.V,
                        out_pin.VB, out_pin.VA,
                        in_pin.VB, in_n_pin.V,
                        out_pin.VC, out_pin.VB
                    };
                case (int)Group.Yd9:
                    return new string[] {
                        in_n_pin.V ,in_pin.VB,
                        out_pin.VA, out_pin.VC,
                        in_n_pin.V, in_pin.VC,
                        out_pin.VB, out_pin.VA,
                        in_n_pin.V, in_pin.VA,
                        out_pin.VC, out_pin.VB
                    };
                case (int)Group.Yd11:
                    return new string[] {
                        in_pin.VA, in_n_pin.V,
                        out_pin.VA, out_pin.VC,
                        in_pin.VB, in_n_pin.V,
                        out_pin.VB, out_pin.VA,
                        in_pin.VC, in_n_pin.V,
                        out_pin.VC, out_pin.VB
                    };
            }
            throw new NotImplementedException();
        }

        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string[] nodes = GetNodes();
            string[] in_p = new string[] { nodes[0], nodes[4], nodes[8] };
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
                    Equation = $"{in_s[i - 1]}_re - {out_s[i - 1]}_re = k_{ID} * I_{ID}c{i}_re * Rc_{ID} + I_{ID}s{i}_re * Rs_{ID} - I_{ID}s{i}_im * Ls_{ID} * frequency;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{in_s[i - 1]}_im - {out_s[i - 1]}_im = k_{ID} * I_{ID}c{i}_im * Rc_{ID} + I_{ID}s{i}_im * Rs_{ID} + I_{ID}s{i}_re * Ls_{ID} * frequency;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{in_p[i - 1]}_re - {out_p[i - 1]}_re = I_{ID}c{i}_re * Rc_{ID} + I_{ID}p{i}_re * Rp_{ID} - I_{ID}p{i}_im * Lp_{ID} * frequency;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{in_p[i - 1]}_im - {out_p[i - 1]}_im = I_{ID}c{i}_im * Rc_{ID} + I_{ID}p{i}_im * Rp_{ID} + I_{ID}p{i}_re * Lp_{ID} * frequency;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"I_{ID}p{i}_re = I_{ID}s{i}_re * k_{ID} + I_{ID}m{i}_re + I_{ID}c{i}_re;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"I_{ID}p{i}_im = I_{ID}s{i}_im * k_{ID} + I_{ID}m{i}_im + I_{ID}c{i}_im;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"I_{ID}c{i}_re * Rc_{ID} = - I_{ID}m{i}_im * Lm_{ID} * frequency;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"I_{ID}c{i}_im * Rc_{ID} = I_{ID}m{i}_re * Lm_{ID} * frequency;"
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
                Equation = $"constant Lm_{ID} = {xm.ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant k_{ID} = {k.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }

        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string[] nodes = GetNodes();
            string[] in_p = new string[] { nodes[0], nodes[4], nodes[8] };
            string[] out_p = new string[] { nodes[1], nodes[5], nodes[9] };
            string[] in_s = new string[] { nodes[2], nodes[6], nodes[10] };
            string[] out_s = new string[] { nodes[3], nodes[7], nodes[11] };
            //current equations for a,as1 ... 
            for (int i = 1; i < 4; i++)
            {
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{ID}p{i}",
                    Node1 = $"{in_p[i - 1]}",
                    Node2 = $"{out_p[i - 1]}"
                });
            }
            for (int i = 1; i < 4; i++)
            {
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{ID}s{i}",
                    Node1 = $"{in_s[i - 1]}",
                    Node2 = $"{out_s[i - 1]}"
                });
            }
            for (int i = 1; i < 4; i++)
            {
                equations.Add(new EquationBlock
                {
                    Equation = $"{in_s[i - 1]} - {out_s[i - 1]} = k_{ID} * I_{ID}c{i} * Rc_{ID} + I_{ID}s{i} * Rs_{ID} + Ls_{ID} * der(I_{ID}s{i});"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{in_p[i - 1]} - {out_p[i - 1]} = I_{ID}c{i} * Rc_{ID} + I_{ID}p{i} * Rp_{ID} + Lp_{ID} * der(I_{ID}p{i});"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"I_{ID}p{i} = I_{ID}s{i} * k_{ID} + I_{ID}m{i} + I_{ID}c{i};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"I_{ID}c{i} * Rc_{ID} = - Lm_{ID} * der(I_{ID}m{i});"
                });
            }
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
                Equation = $"constant Lm_{ID} = {xm.ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant k_{ID} = {k.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    public class SteadyStateTransformerYdModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            Complex32 zp = (elementObject.GetValue("Zp") as ComplexValue).Value;
            Complex32 zs = (elementObject.GetValue("Zs") as ComplexValue).Value;
            double xm = (elementObject.GetValue("Xm") as FloatValue).Value;
            double rc = (elementObject.GetValue("Rc") as FloatValue).Value;
            double k = (elementObject.GetValue("K") as FloatValue).Value;
            int group = (elementObject.GetValue("Group") as IntValue).Value;
            return new TransformerYd(zp, zs, (float)xm, (float)rc, (float)k, group, elementNodes["in"] as Pin3Phase, elementNodes["out"] as Pin3Phase, elementNodes["in_n"] as Pin1Phase);
        }
    }
    public class TransientTransformYdModel : ITransientElementModel
    {
        public ITransientElement CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            Complex32 zp = (elementObject.GetValue("Zp") as ComplexValue).Value;
            Complex32 zs = (elementObject.GetValue("Zs") as ComplexValue).Value;
            double xm = (elementObject.GetValue("Xm") as FloatValue).Value;
            double rc = (elementObject.GetValue("Rc") as FloatValue).Value;
            double k = (elementObject.GetValue("K") as FloatValue).Value;
            int group = (elementObject.GetValue("Group") as IntValue).Value;
            return new TransformerYd(zp, zs, (float)xm, (float)rc, (float)k, group, elementNodes["in"] as Pin3Phase, elementNodes["out"] as Pin3Phase, elementNodes["in_n"] as Pin1Phase);
        }
    }
}
