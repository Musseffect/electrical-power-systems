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
    class LoadD : Element, ITransientElement, ISteadyStateElement
    {
        Pin3Phase in_pin;
        Complex32 zAB;
        Complex32 zBC;
        Complex32 zCA;
        public string IAB { get { return $"I_{ID}a"; } }
        public string IBC { get { return $"I_{ID}b"; } }
        public string ICA { get { return $"I_{ID}c"; } }
        public string IABre { get { return $"I_{ID}ab_re"; } }
        public string IBCre { get { return $"I_{ID}bc_re"; } }
        public string ICAre { get { return $"I_{ID}ca_re"; } }
        public string IABim { get { return $"I_{ID}ab_im"; } }
        public string IBCim { get { return $"I_{ID}bc_im"; } }
        public string ICAim { get { return $"I_{ID}ca_im"; } }
        public LoadD(Complex32 zAB, Complex32 zBC, Complex32 zCA, Pin3Phase in_pin) : base()
        {
            this.zAB = zAB;
            this.zBC = zBC;
            this.zCA = zCA;
            this.in_pin = in_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string RAB = $"R_{ID}ab";
            string LAB = $"L_{ID}ab";
            string RBC = $"R_{ID}bc";
            string LBC = $"L_{ID}bc";
            string RCA = $"R_{ID}ca";
            string LCA = $"L_{ID}ca";

            equations.Add(new CurrentFlowBlock
            {
                Equation = IAB,
                Node1 = in_pin.VA,
                Node2 = in_pin.VB
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBC,
                Node1 = in_pin.VB,
                Node2 = in_pin.VC
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICA,
                Node1 = in_pin.VC,
                Node2 = in_pin.VA
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VA} - {in_pin.VB} - ({IAB} * {RAB} + der({IAB}) * {LAB}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VB} - {in_pin.VC} - ({IBC} * {RBC} + der({IBC}) * {LBC}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VC} - {in_pin.VA} - ({ICA} * {RCA} + der({ICA}) * {LCA}) = 0;"
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}ab = {zAB.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant L_{ID}ab = {(zAB.Imaginary).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}bc = {zBC.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant L_{ID}bc = {(zBC.Imaginary).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}ca = {zCA.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant L_{ID}ca = {(zCA.Imaginary).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string RAB = $"R_{ID}ab";
            string XAB = $"X_{ID}ab";
            string RBC = $"R_{ID}bc";
            string XBC = $"X_{ID}bc";
            string RCA = $"R_{ID}ca";
            string XCA = $"X_{ID}ca";

            equations.Add(new CurrentFlowBlock
            {
                Equation = IABre,
                Node1 = in_pin.VAre,
                Node2 = in_pin.VBre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IABim,
                Node1 = in_pin.VAim,
                Node2 = in_pin.VBim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBCre,
                Node1 = in_pin.VBre,
                Node2 = in_pin.VCre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBCim,
                Node1 = in_pin.VBim,
                Node2 = in_pin.VCim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICAre,
                Node1 = in_pin.VCre,
                Node2 = in_pin.VAre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICAim,
                Node1 = in_pin.VCim,
                Node2 = in_pin.VAim
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VAre} - {in_pin.VBre} - ({IABre} * {RAB} - {IABim} * {XAB}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VAim} - {in_pin.VBim} - ({IABre} * {XAB} + {IABim} * {RAB}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VBre} - {in_pin.VCre} - ({IBCre} * {RBC} - {IBCim} * {XBC}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VBim} - {in_pin.VCim} - ({IBCre} * {XBC} + {IBCim} * {RBC}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VCre} - {in_pin.VAre} - ({ICAre} * {RCA} - {ICAim} * {XCA}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VCim} - {in_pin.VAim} - ({ICAre} * {XCA} + {ICAim} * {RCA}) = 0;"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}ab = {zAB.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant X_{ID}ab = {(zAB.Imaginary * frequency).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}bc = {zBC.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant X_{ID}bc = {(zBC.Imaginary * frequency).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}ca = {zCA.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant X_{ID}ca = {(zCA.Imaginary * frequency).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            return equations;
        }
    }
    public class SteadyStateLoadDModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            Complex32 zab = (elementObject.GetValue("ZAB") as ComplexValue).Value;
            Complex32 zbc = (elementObject.GetValue("ZBC") as ComplexValue).Value;
            Complex32 zca = (elementObject.GetValue("ZCA") as ComplexValue).Value;
            return new LoadD(zab, zbc, zca, elementNodes["in"] as Pin3Phase);
        }
    }
}
