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
    class LoadY : Element, ITransientElement, ISteadyStateElement
    {
        Pin1Phase n_pin;
        Pin3Phase in_pin;
        Complex32 zA;
        Complex32 zB;
        Complex32 zC;
        public string IA { get { return $"I_{ID}a"; } }
        public string IB { get { return $"I_{ID}b"; } }
        public string IC { get { return $"I_{ID}c"; } }
        public string IAre { get { return $"I_{ID}a_re";} }
        public string IBre { get { return $"I_{ID}b_re";} }
        public string ICre { get { return $"I_{ID}c_re";} }
        public string IAim { get { return $"I_{ID}a_im";} }
        public string IBim { get { return $"I_{ID}b_im";} }
        public string ICim { get { return $"I_{ID}c_im"; } }
        public LoadY(Complex32 zA, Complex32 zB, Complex32 zC, Pin3Phase in_pin, Pin1Phase n_pin) : base()
        {
            this.zA = zA;
            this.zB = zB;
            this.zC = zC;
            this.in_pin = in_pin;
            this.n_pin = n_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string RA = $"R_{ID}a";
            string LA = $"L_{ID}a";
            string RB = $"R_{ID}b";
            string LB = $"L_{ID}b";
            string RC = $"R_{ID}c";
            string LC = $"L_{ID}c";

            equations.Add(new CurrentFlowBlock
            {
                Equation = IA,
                Node1 = in_pin.VA,
                Node2 = n_pin.V
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IB,
                Node1 = in_pin.VB,
                Node2 = n_pin.V
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IC,
                Node1 = in_pin.VC,
                Node2 = n_pin.V
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VA} - {n_pin.V} - ({IA} * {RA} + der({IA}) * {LA}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VB} - {n_pin.V} - ({IB} * {RB} + der({IB}) * {LB}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VC} - {n_pin.V} - ({IC} * {RC} + der({IC}) * {LC}) = 0;"
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}a = {zA.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant L_{ID}a = {(zA.Imaginary).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}b = {zB.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant L_{ID}b = {(zB.Imaginary).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}c = {zC.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant L_{ID}c = {(zC.Imaginary).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string RA = $"R_{ID}a";
            string XA = $"X_{ID}a";
            string RB = $"R_{ID}b";
            string XB = $"X_{ID}b";
            string RC = $"R_{ID}c";
            string XC = $"X_{ID}c";

            equations.Add(new CurrentFlowBlock
            {
                Equation = IAre,
                Node1 = in_pin.VAre,
                Node2 = n_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IAim,
                Node1 = in_pin.VAim,
                Node2 = n_pin.Vim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBre,
                Node1 = in_pin.VBre,
                Node2 = n_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBim,
                Node1 = in_pin.VBim,
                Node2 = n_pin.Vim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICre,
                Node1 = in_pin.VCre,
                Node2 = n_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICim,
                Node1 = in_pin.VCim,
                Node2 = n_pin.Vim
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VAre} - {n_pin.Vre} - ({IAre} * {RA} - {IAim} * {XA}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VAim} - {n_pin.Vim} - ({IAre} * {XA} + {IAim} * {RA}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VBre} - {n_pin.Vre} - ({IBre} * {RB} - {IBim} * {XB}) = 0;"
            });                        
            equations.Add(new EquationBlock
            {                          
                Equation = $"{in_pin.VBim} - {n_pin.Vim} - ({IBre} * {XB} + {IBim} * {RB}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VCre} - {n_pin.Vre} - ({ICre} * {RC} - {ICim} * {XC}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VCim} - {n_pin.Vim} - ({ICre} * {XC} + {ICim} * {RC}) = 0;"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}a = {zA.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant X_{ID}a = {(zA.Imaginary * frequency).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}b = {zB.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant X_{ID}b = {(zB.Imaginary * frequency).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}c = {zC.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant X_{ID}c = {(zC.Imaginary * frequency).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            return equations;
        }
    }
    public class SteadyStateLoadYModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            Complex32 za = (elementObject.GetValue("ZA") as ComplexValue).Value;
            Complex32 zb = (elementObject.GetValue("ZB") as ComplexValue).Value;
            Complex32 zc = (elementObject.GetValue("ZC") as ComplexValue).Value;
            return new LoadY(za,zb,zc, elementNodes["in"] as Pin3Phase, elementNodes["n"] as Pin1Phase);
        }
    }
}
